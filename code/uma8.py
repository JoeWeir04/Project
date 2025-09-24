import hid

h = hid.Device(0x2752, 0x01C)

while True:
    data = h.read(6, timeout=500)
    if len(data) == 6 and data[0] == 0x06 and data[1] == 0x36:
        vad = data[2]
        angle = (data[3] << 8) | data[4]
        mic = data[5]
        print(f"VAD={vad} | Angle={angle}° | Mic={mic}", end='\r')

