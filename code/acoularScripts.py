"""
Realtime multi-source DOA tracker for UMA-8 (no beamforming),
with SRP "strength" values for each detected direction.

Requirements:
  pip install sounddevice pyroomacoustics numpy scipy
"""

import numpy as np
import sounddevice as sd
import soundfile as sf
import pyroomacoustics as pra
from scipy.signal import find_peaks
from datetime import datetime
import queue
import threading
import asyncio
import websockets
import json
from mediapipe.tasks import python
from mediapipe.tasks.python import audio
from mediapipe.tasks.python.components import containers
import mediapipe as mp
import time
from collections import defaultdict, deque

# ------------------- USER PARAMETERS -------------------
SAVE_BEAMFORMED = True
ENERGY_THRESHOLD = 0.00008
FS = 16000
N_MICS = 8
FRAME_SIZE = 2048
HOP_SIZE = FRAME_SIZE // 2
N_FFT = FRAME_SIZE
SPEED_OF_SOUND = 343.0
RADIUS = 0.04
AZ_RES_DEG = 2.0
MAX_SOURCES = 1
PEAK_THRESHOLD = 0.5
DEVICE = None
CENTER_INDEX = 0
ANGLE_OFFSET = 90
MODEL_PATH = "../classifier.tflite"
ANGLE_BIN_WIDTH = 30
MIN_CLIP_DURATION = 1

beam_buffers = defaultdict(lambda: deque (maxlen=int(FS * MIN_CLIP_DURATION)))
last_classification = {}

audio_q = queue.Queue(maxsize=40)

#print(sd.query_devices())

latest_sources = []
latest_classification = "Waiting"


def normalize_angle_deg(a):
    return (a + 360.0) % 360.0


# ---------- mic geometry ----------
def mic_positions_circle(m, radius):
    angles = np.linspace(0, 2*np.pi, m, endpoint=False)
    xs = radius * np.cos(angles)
    ys = radius * np.sin(angles)
    zs = np.zeros_like(xs)
    return np.vstack((xs, ys, zs))  # (3, M)


mic_positions = mic_positions_circle(N_MICS-1, RADIUS)

# ---------- DOA object ----------
azimuths = np.deg2rad(np.arange(0.0, 360.0, AZ_RES_DEG))
doa = pra.doa.SRP(mic_positions, fs=FS, nfft=N_FFT, c=SPEED_OF_SOUND,
                  azimuth=azimuths, num_src=MAX_SOURCES, mode='stft')

# ---------- helpers ----------
win = np.hanning(FRAME_SIZE)
audio_q = queue.Queue(maxsize=40)
ring_buffer = np.zeros((N_MICS-1, FRAME_SIZE), dtype=np.float32)


def create_classifier():
    options = audio.AudioClassifierOptions(
        base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
        max_results=1,
        running_mode=mp.tasks.audio.RunningMode.AUDIO_CLIPS,
    )
    return audio.AudioClassifier.create_from_options(options)


classifier = create_classifier()


def classify_beamformed(beamformed, fs=FS):
    import pyroomacoustics as pra
    beamformed = beamformed / (np.max(np.abs(beamformed)) + 1e-6)
    beamformed = np.clip(beamformed * 5.0, -1.0, 1.0)

    # resample to 16k for classifier
    beamformed_16k = pra.resample(beamformed, fs, 16000)
    audio_data = containers.AudioData.create_from_array(beamformed_16k.astype(np.float32), 16000)
    result = classifier.classify(audio_data)

    # Handle case where result might be a list
    if isinstance(result, list):
        if len(result) == 0:
            return "Unknown"
        result = result[0]

    if result and result.classifications:
        top = result.classifications[0].categories[0]
        return f"{top.category_name} ({top.score:.2f})"
    return "Unknown"


def start_classifier_thread():
    """Runs MediaPipe AudioClassifier in a background thread."""
    global latest_classification
    SAMPLE_RATE = 16000
    BUFFER_SIZE = 0.1
    TIME_INCREMENT = int(BUFFER_SIZE * 1000)
    timestamp = 0

    def print_result(result: mp.tasks.audio.AudioClassifierResult, timestamp_ms: int):
        global latest_classification
        if result and result.classifications:
            top = result.classifications[0].categories[0]
            latest_classification = f"{top.category_name} ({top.score:.2f})"

    options = audio.AudioClassifierOptions(
        base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=mp.tasks.audio.RunningMode.AUDIO_STREAM,
        max_results=1,
        result_callback=print_result
    )

    def audio_callback(indata, frames, time_info, status):
        nonlocal timestamp
        GAIN = 20.0
        boosted = np.clip(indata[:, 0] * GAIN, -1.0, 1.0)
        audio_data = containers.AudioData.create_from_array(boosted, SAMPLE_RATE)
        classifier.classify_async(audio_data, timestamp)
        timestamp += TIME_INCREMENT

    with audio.AudioClassifier.create_from_options(options) as classifier:
        with sd.InputStream(channels=1, samplerate=SAMPLE_RATE,
                            blocksize=int(SAMPLE_RATE * BUFFER_SIZE),
                            callback=audio_callback):
            while True:
                time.sleep(0.1)


def start_classification():
    t = threading.Thread(target=start_classifier_thread, daemon=True)
    t.start()


def stft_frame(block_frame):
    windowed = block_frame * win[np.newaxis, :]
    spec = np.fft.rfft(windowed, n=N_FFT, axis=-1)
    return spec.T  # (n_bins, M)


def srp_phat_detect(stft_frame):
    P = np.transpose(stft_frame, (1, 0))[:, :, np.newaxis]
    doa.locate_sources(P)

    try:
        srp_map = doa.grid.values
        azimuths_rad = doa.grid.azimuth
    except AttributeError:
        srp_map = doa.srp
        azimuths_rad = doa.azimuth_grid

    srp_norm = srp_map / np.max(srp_map)
    peaks, props = find_peaks(srp_norm, height=PEAK_THRESHOLD)
    if len(peaks) == 0:
        return np.array([]), np.array([])

    order = np.argsort(srp_norm[peaks])[::-1]
    peaks = peaks[order][:MAX_SOURCES]
    detected_angles_deg = np.degrees(azimuths_rad[peaks])
    strengths = srp_norm[peaks]
    return detected_angles_deg, strengths


# ---------- sounddevice callback ----------
def sd_callback(indata, frames, time_info, status):
    if status:
        print("Sounddevice status:", status, flush=True)
    try:
        outer_mics = indata.T[1:, :]
        audio_q.put_nowait(outer_mics.copy())
    except queue.Full:
        pass


async def ws_server():
    async def handler(websocket):
        global latest_sources
        while True:
            await asyncio.sleep(0.1)
            msg = json.dumps({"sources": latest_sources})
            await websocket.send(msg)
    async with websockets.serve(handler, "localhost", 8765):
        await asyncio.Future()  # run forever


# ---------- processing thread ----------
def processing_thread():
    global latest_sources
    try:
        while True:
            block = audio_q.get()
            if block.shape[1] != HOP_SIZE:
                continue

            rms = np.sqrt(np.mean(block**2))

            if rms < ENERGY_THRESHOLD:
                latest_sources = []
                continue

            ring_buffer[:, :-HOP_SIZE] = ring_buffer[:, HOP_SIZE:]
            ring_buffer[:, -HOP_SIZE:] = block

            X = stft_frame(ring_buffer)
            angles_deg, strengths = srp_phat_detect(X)

            detected = []

            if len(angles_deg) > 0:
                for a, s in zip(angles_deg, strengths):
                    a_norm = normalize_angle_deg(float(a) + ANGLE_OFFSET)
                    #try:
                    # compute steering delays for this azimuth
                    theta = np.deg2rad(a)
                    direction = np.array([np.cos(theta), np.sin(theta), 0.0])
                    delays = np.dot(mic_positions.T, direction) / SPEED_OF_SOUND  # shape (N_mics-1,)

                    # align signals by delay (simple fractional delay via phase shift)
                    spec = np.fft.rfft(ring_buffer, axis=1)
                    freqs = np.fft.rfftfreq(ring_buffer.shape[1], d=1.0 / FS)
                    steer = np.exp(-1j * 2 * np.pi * freqs[:, None] * delays[None, :])
                    beamformed_spec = np.sum(spec * steer.T, axis=0)
                    beamformed = np.fft.irfft(beamformed_spec)
                    
                    key = round(a_norm / ANGLE_BIN_WIDTH) * ANGLE_BIN_WIDTH
                    #beam_buffers[key].extend(beamformed.tolist())
                    
                    if len(beam_buffers[key]) >= FS * MIN_CLIP_DURATION:
                        clip = np.array(beam_buffers[key], dtype=np.float32)
                        """
                        if SAVE_BEAMFORMED:
                            filename = f"beamformed_{int(key)}deg_{datetime.now().strftime('%H%M%S')}.wav"
                            sf.write(filename, clip, FS)
                            print(f"Saved beamformed clip for {key}° -> {filename}")
                        label = classify_beamformed(clip)
                        last_classification[key] = label
                        beam_buffers[key].clear()
                        """
         
                    label = last_classification.get(key, 'Waiting')

                    detected.append({
                        "angle": a_norm,
                        "strength": float(s),
                        "classification": str(label)
                    })

                latest_sources = detected
            else:
                latest_sources = []
    except KeyboardInterrupt:
        print("Processing stopped by user.")


# ---------- main ----------
def main():
    print("Realtime UMA-8 multi-source DOA detection (with strength)")
    print(f"FS={FS}, N_MICS={N_MICS}, FRAME={FRAME_SIZE}, HOP={HOP_SIZE}")
    print("Ensure UMA-8 outputs raw multichannel audio (no onboard beamforming).")

    sd.default.device = DEVICE
    in_stream = sd.InputStream(samplerate=FS, blocksize=HOP_SIZE,
                               dtype='float32', channels=N_MICS,
                               callback=sd_callback, device=DEVICE)
    with in_stream:
        t = threading.Thread(target=processing_thread, daemon=True)
        t.start()
        #start_classification()
        asyncio.run(ws_server())


if __name__ == "__main__":
    main()
