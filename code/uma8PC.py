import hid
import sounddevice as sd
import mediapipe as mp
from mediapipe.tasks import python
from mediapipe.tasks.python import audio
from mediapipe.tasks.python.components import containers
import threading
import asyncio
import json
import sys


MODEL_PATH = "../classifier.tflite"
SAMPLE_RATE = 16000
BUFFER_SIZE = 0.1
TIME_INCREMENT = int(BUFFER_SIZE*1000)


BUFFER_SAMPLES = int()

last_classification = "Waiting"
last_transcript = ""
last_vad = 0
last_angle = 0
timestamp = 0
classifier = None

clients = set()

print(sd.query_devices())



async def broadcast_loop():
    global last_classification, last_transcript, last_vad, last_angle
    while True:
        if clients:
            data = {
                "vad": last_vad,
                "angle": last_angle,
                "classification": last_classification,
                "transcript": last_transcript
            }
            message = json.dumps(data)
            await asyncio.gather(*[client.send(message) for client in clients])
        await asyncio.sleep(0.05)


async def websocket_handler(websocket):
    clients.add(websocket)
    try:
        await websocket.wait_closed()
    finally:
        clients.remove(websocket)


def print_result(result: mp.tasks.audio.AudioClassifierResult, timestamp_ms: int):
    global last_classification
    if result and result.classifications:
        top = result.classifications[0].categories[0]
        last_classification = f"{top.category_name} ({top.score:.2f})"


def process_text(text):
    global last_transcript
    last_transcript = text


async def main_loop():
    global classifier, timestamp, last_vad, last_angle

    def audio_callback(indata, frames, time_info, status):
        global timestamp
        audio_data = containers.AudioData.create_from_array(indata[:, 0], SAMPLE_RATE)
        classifier.classify_async(audio_data, timestamp)
        timestamp += TIME_INCREMENT

    options = audio.AudioClassifierOptions(
        base_options=python.BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=mp.tasks.audio.RunningMode.AUDIO_STREAM,
        max_results=1,
        result_callback=print_result
    )

    h = hid.Device(0x2752, 0x01C)

    with audio.AudioClassifier.create_from_options(options) as classifier:
        with sd.InputStream(channels=1, samplerate=SAMPLE_RATE,
                            blocksize=int(SAMPLE_RATE * BUFFER_SIZE),
                            callback=audio_callback):
            try:
                print()
                print("--- Program ---")
                while True:
                    data = h.read(6, timeout=00)
                    if len(data) == 6 and data[0] == 0x06 and data[1] == 0x36:
                        last_vad = data[2]
                        last_angle = (data[3] << 8) | data[4]
                        #print("\033[K", end='')
                        #print(f" VAD={vad} | Angle={angle}° | classification={last_classification} | Transcript = {last_transcript}" , end='\r', flush=True)
                    await asyncio.sleep(0.01)
            except KeyboardInterrupt:
                print()
                print("--- Exiting Program ---")
                sys.exit()


async def main():
    import websockets
    server = await websockets.serve(websocket_handler, "0.0.0.0", 8765)
    await asyncio.gather(main_loop(), broadcast_loop())

if __name__ == "__main__":
    asyncio.run(main())