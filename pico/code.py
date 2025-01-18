import board
import os
import audiomp3
import audiobusio
import adafruit_requests
import socketpool
import wifi
import ssl
from adafruit_datetime import datetime, date, time

TIME_URL = os.getenv("TIME_URL")
ZUSZEITEN_URL = os.getenv("ZUSZEITEN_URL")

wifi.radio.connect(
    os.getenv("CIRCUITPY_WIFI_SSID"),
    os.getenv("CIRCUITPY_WIFI_PASSWORD")
)
audio = audiobusio.I2SOut(board.GP27, board.GP28, board.GP26)
pool = socketpool.SocketPool(wifi.radio)
session = adafruit_requests.Session(pool, ssl.create_default_context())

starte = audiomp3.MP3Decoder(open("audio/starte.mp3", "rb"))
audio.play(starte)
while audio.playing:
    pass

print("Current time (GMT +1):", datetime.now())

resp_zuszeiten = session.get(ZUSZEITEN_URL)
zus_zeiten = resp_zuszeiten.json()

for zus in zus_zeiten:
    print(zus["day"], zus["time"])
