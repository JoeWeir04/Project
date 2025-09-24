import hid
import time
import numpy as np
import sounddevice as sd
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import audio
from mediapipe.tasks.python.components import containers


MODEL_PATH = "../classifier.tflite"
SAMPLE_RATE = 16000
BUFFER_DURATION = 0.5

last_classification = "Waiting"
last_vad = 0
last_angle = 0
latest_mic = 0
timestamp = 0


def print_result(result: mp.tasks.audio.AudioClassifierResult, timestamp_ms: int):
    global last_classification
    if result and result.classifications:
        top = result.classifications[0].categories[0]
        last_classification = f"{top.category_name} ({top.score:.2f})"


options = audio.AudioClassifierOptions(
    base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
    running_mode=mp.tasks.audio.RunningMode.AUDIO_STREAM,
    max_results=2,
    result_callback=print_result
)


def audio_callback(indata, frames, time_info, status):
    global timestamp
    audio_data = containers.AudioData.create_from_array(indata[:, 0], SAMPLE_RATE)
    classifier.classify_async(audio_data, timestamp)
    timestamp += 975


h = hid.Device(0x2752, 0x01C)

with audio.AudioClassifier.create_from_options(options) as classifier:
    with sd.InputStream(channels=1, samplerate=SAMPLE_RATE,
                        blocksize=int(SAMPLE_RATE * BUFFER_DURATION),
                        callback=audio_callback):
        while True:
            data = h.read(6, timeout=500)
            if len(data) == 6 and data[0] == 0x06 and data[1] == 0x36:
                vad = data[2]
                angle = (data[3] << 8) | data[4]
                mic = data[5]
                print(f"VAD={vad} | Angle={angle}° | Mic={mic} | classification={last_classification}", end='\r')
