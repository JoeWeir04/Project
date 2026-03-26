"""
Realtime multi-source DOA tracker for UMA-8 (no beamforming),
with SRP "strength" values for each detected direction.

Requirements:
  pip install sounddevice pyroomacoustics numpy scipy
"""

import numpy as np
import sounddevice as sd
import pyroomacoustics as pra
from scipy.signal import find_peaks
import queue
import threading
import asyncio
import websockets
import json
import time
import statistics
import csv
import sys

# ------------------- USER PARAMETERS -------------------
ENERGY_THRESHOLD = 0.00008
FS = 16000
N_MICS = 8
FRAME_SIZE = 1024
HOP_SIZE = 512
N_FFT = FRAME_SIZE
SPEED_OF_SOUND = 343.0
RADIUS = 0.04
AZ_RES_DEG = 2.0
MAX_SOURCES = 1
PEAK_THRESHOLD = 0.5
DEVICE = None
CENTER_INDEX = 0
ANGLE_OFFSET = 90
UMA8_NAME = "miniDSP micArray"

last_classification = "Waiting"
last_transcript = ""
last_vad = 0
last_angle = 0
last_distance = 0
RMS_SCALE = 900

RECONNECT_DELAY = 2

clients = set()

#print(sd.query_devices())

SAVE_ANGLES_BOOL = False
angles = []


def find_uma8_device():
    """Return the device index of the UMA-8, or None if not found."""
    devices = sd.query_devices()
    for i, dev in enumerate(devices):
        if UMA8_NAME.lower() in dev['name'].lower() and dev['max_input_channels'] >= N_MICS:
            print(f"Found UMA-8: [{i}] {dev['name']}")
            return i
    print("UMA-8 not found in device list.")
    return None


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


def save_angles_to_csv(angles, std_dev=None, angle_range=None):
    with open("angles.csv", "w", newline="") as f:
        writer = csv.writer(f)
        writer.writerow(["sample", "timestamp", "angle"])
        for i, (ts, angle) in enumerate(angles):
            writer.writerow([i, ts, angle])

        if std_dev is not None:
            writer.writerow([])
            writer.writerow(["STATISTICS"])
            writer.writerow(["samples", len(angles)])
            writer.writerow(["std_dev", f"{std_dev:.4f}"])
            writer.writerow(["range", f"{angle_range:.4f}"])


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


def sd_callback(indata, frames, time_info, status):
    if status:
        print("Sounddevice status:", status, flush=True)
        if status.input_overflow:
            pass
        elif status.prematurely_stopped or status.input_underflow:
            return
    try:
        outer_mics = indata.T[1:, :]
        audio_q.put_nowait(outer_mics.copy())
    except queue.Full:
        pass


async def websocket_handler(websocket):
    clients.add(websocket)
    print("Client connected")
    try:
        await websocket.wait_closed()
    finally:
        print("Client disconnected")
        clients.discard(websocket)


async def broadcast_loop():
    global last_classification, last_transcript, last_vad, last_angle, last_distance
    while True:
        if clients:
            data = {
                "vad": last_vad,
                "angle": last_angle,
                "classification": last_classification,
                "transcript": last_transcript,
                "distance": last_distance     
            }
            message = json.dumps(data)
            dead_clients = set()
            for client in list(clients):
                try:
                    await client.send(message)
                except Exception as e:
                    print("Client send failed: removing the client:", e)
                    dead_clients.add(client)
            for client in dead_clients:
                clients.discard(client)
        await asyncio.sleep(0.05)


def processing_thread():
    global last_angle, last_distance, last_vad
    try:
        while True:
            block = audio_q.get()
            if block.shape[1] != HOP_SIZE:
                continue
            rms = np.sqrt(np.mean(block**2))

            dist_temp = min(1.0, rms * RMS_SCALE)

            if dist_temp < 0.4:
                last_distance = 0.2
            elif dist_temp < 0.7:
                last_distance = 0.5
            else:
                last_distance = 1

            if rms < ENERGY_THRESHOLD:
                last_vad = 0
                continue
            else:
                last_vad = 1

            ring_buffer[:, :-HOP_SIZE] = ring_buffer[:, HOP_SIZE:]
            ring_buffer[:, -HOP_SIZE:] = block

            X = stft_frame(ring_buffer)
            angles_deg, strengths = srp_phat_detect(X)

            if len(angles_deg) > 0:
                for a, s in zip(angles_deg, strengths):
                    a_norm = normalize_angle_deg(float(a) + ANGLE_OFFSET)
                    last_angle = a_norm
                    #print(last_angle, last_distance)
                    if SAVE_ANGLES_BOOL:
                        if int(time.time() * 10) % 5 == 0:
                            angles.append((time.time(), a_norm))
            else:
                last_vad = 0
    except KeyboardInterrupt:
        print("Processing stopped by user.")


def create_stream():
    device_index = find_uma8_device()
    if device_index is None:
        return None
    try:
        stream = sd.InputStream(
            samplerate=FS,
            blocksize=HOP_SIZE,
            dtype='float32',
            channels=N_MICS,
            callback=sd_callback,
            device=device_index
        )
        stream.start()
        print(f"Audio stream opened successfully on device {device_index}")
        return stream
    except Exception as e:
        print(f"Failed to open audio stream: {e}")
        return None


def stream_watchdog():
    """
    Runs in its own thread. Owns the stream lifecycle — opens it,
    monitors it, and reopens it if it dies or the device disconnects.
    """
    stream = None
    while True:
        # --- Open stream if we don't have one ---
        if stream is None or not stream.active:
            if stream is not None:
                try:
                    stream.close()
                except Exception:
                    pass
                print(f"Stream lost. Retrying in {RECONNECT_DELAY}s...")
                time.sleep(RECONNECT_DELAY)

            stream = create_stream()
            if stream is None:
                time.sleep(RECONNECT_DELAY)
                continue  # retry loop

        # --- Poll stream health ---
        time.sleep(0.5)


async def main_async():
    server = await websockets.serve(
        websocket_handler,
        "0.0.0.0",
        8765,
        ping_interval=None
    )

    await asyncio.gather(
        broadcast_loop(), asyncio.Future()
    )


def main():
    print("Realtime UMA-8 multi-source DOA detection (with strength)")
    print(f"FS={FS}, N_MICS={N_MICS}, FRAME={FRAME_SIZE}, HOP={HOP_SIZE}")
    print("Ensure UMA-8 outputs raw multichannel audio (no onboard beamforming).")

    sd.default.device = DEVICE
    t = threading.Thread(target=processing_thread, daemon=True)
    t.start()

    w = threading.Thread(target=stream_watchdog, daemon=True)
    w.start()
    try:
        asyncio.run(main_async())
    except KeyboardInterrupt:
        print("\n--- Exiting Program ---")
        if len(angles) > 1:
            angle_values = [angle for (_, angle) in angles]
            std_dev = statistics.stdev(angle_values)
            angle_range = max(angle_values) - min(angle_values)
            print(f"Samples collected: {len(angles)} \nStd: {std_dev:.2f} \nAngles range: {angle_range:.2f}")
            save_angles_to_csv(angles, std_dev, angle_range)
        else:
            print("Not enough samples to save.")
        sys.exit()


if __name__ == "__main__":
    main()
