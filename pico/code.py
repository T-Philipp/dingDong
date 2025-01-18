import board
import os
import audiomp3
import audiobusio
import adafruit_requests
import socketpool
import wifi
import ssl
import time
import rtc

TIME_URL = os.getenv("TIME_URL")
ZUSZEITEN_URL = os.getenv("ZUSZEITEN_URL")

rtc = rtc.RTC()
audio = audiobusio.I2SOut(board.GP27, board.GP28, board.GP26)

starte = audiomp3.MP3Decoder(open("audio/starte.mp3", "rb"))
audio.play(starte)
while audio.playing:
    pass

wifi.radio.connect(
    os.getenv("CIRCUITPY_WIFI_SSID"),
    os.getenv("CIRCUITPY_WIFI_PASSWORD")
)
pool = socketpool.SocketPool(wifi.radio)
session = adafruit_requests.Session(pool, ssl.create_default_context())
weekday = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"]
loopcounter = 0

# Fetch and update time and zusammenkunftszeiten twice a day
while True:
    loopcounter += 1

    resp_zuszeiten = session.get(ZUSZEITEN_URL).json()
    resp_time = session.get(TIME_URL).json()
    pTime = resp_time["pythonTime"]

    rtc.datetime = time.struct_time((pTime["year"], pTime["month"], pTime["day"], pTime["hour"], pTime["minute"], pTime["second"], pTime["weekday"], -1, -1))

    if loopcounter == 1:
        internet = audiomp3.MP3Decoder(open("audio/internet.mp3", "rb"))
        audio.play(internet)
        while audio.playing:
            pass

    while True:
        t = rtc.datetime

        for zus in resp_zuszeiten:
            if zus["day"] == weekday[t.tm_wday]:
                if t.tm_min < 10:
                    mins = "0" + str(t.tm_min)
                else:
                    mins = str(t.tm_min)
                if t.tm_sec < 10:
                    secs = "0" + str(t.tm_sec)
                else:
                    secs = str(t.tm_sec)
                cur_time_str = str(t.tm_hour) + ':' + mins + ':' + secs

                if zus["time"] == cur_time_str:
                    gong = audiomp3.MP3Decoder(open("audio/Schulgong.mp3", "rb"))
                    audio.play(gong)
                    while audio.playing:
                        pass

        # wait for 1 second
        time.sleep(1)

        # wait for 12 hours
    time.sleep(43200)

