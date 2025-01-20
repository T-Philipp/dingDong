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

# Load environment variables
TIME_URL = os.getenv("TIME_URL")
ZUSZEITEN_URL = os.getenv("ZUSZEITEN_URL")
WIFI_SSID = os.getenv("CIRCUITPY_WIFI_SSID")
WIFI_PASSWORD = os.getenv("CIRCUITPY_WIFI_PASSWORD")

# Initialize RTC and audio output
rtc = rtc.RTC()
audio = audiobusio.I2SOut(board.GP27, board.GP28, board.GP26)

# Function to play an MP3 file
def play_mp3(file_path):
    decoder = audiomp3.MP3Decoder(open(file_path, "rb"))
    audio.play(decoder)
    while audio.playing:
        pass

# Play startup sound
play_mp3("audio/starte.mp3")

# Connect to WiFi
wifi.radio.connect(WIFI_SSID, WIFI_PASSWORD)
pool = socketpool.SocketPool(wifi.radio)
session = adafruit_requests.Session(pool, ssl.create_default_context())

# Weekday names
weekday = ["Mo", "Di", "Mi", "Do", "Fr", "Sa", "So"]
loopcounter = 0

# Fetch and update time and zusammenkunftszeiten twice a day
while True:
    loopcounter += 1

    # Fetch data from URLs
    resp_zuszeiten = session.get(ZUSZEITEN_URL).json()
    resp_time = session.get(TIME_URL).json()
    pTime = resp_time["timeTuple"]

    # Update RTC with fetched time
    rtc.datetime = time.struct_time((
        pTime["year"], pTime["month"], pTime["day"],
        pTime["hour"], pTime["minute"], pTime["second"],
        pTime["weekday"], -1, -1
    ))

    # Play internet connection sound on first loop
    if loopcounter == 1:
        play_mp3("audio/internet.mp3")

    while True:
        t = rtc.datetime

        for zus in resp_zuszeiten:
            if zus["day"] == weekday[t.tm_wday]:
                cur_time_str = f"{t.tm_hour:02}:{t.tm_min:02}:{t.tm_sec:02}"

                if zus["time"] == cur_time_str:
                    play_mp3("audio/Schulgong.mp3")

        # wait for 1 second
        time.sleep(1)

    # wait for 12 hours
    time.sleep(43200)

