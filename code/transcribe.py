from RealtimeSTT import AudioToTextRecorder

# This function will run every time new text is detected


def process_text(text):
    print("Transcript:", text)


if __name__ == '__main__':
    print("Wait until you see 'speak now', then start talking...")

    # Create the recorder
    recorder = AudioToTextRecorder(
        model="base.en",                # "tiny.en", "small.en", "base.en", etc.
        enable_realtime_transcription=True,  # Enable real-time updates
        realtime_model_type="base.en",       # Small fast model for real-time
        silero_sensitivity=0.6,              # Controls how sensitive it is to speech
        webrtc_sensitivity=3,                # More aggressive VAD
    )

    # Continuously listen and print as you speak
    while True:
        recorder.text(process_text)
