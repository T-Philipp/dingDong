# Nachbau und Dokumentation

Ziel war eine kostengünstige und freie, unabhängige Lösung, automatisch eine Minute vor Beginn der Zusammenkunft einen Ton abzuspielen.
Änderungen an der Sommer-/Winterzeit sollten keine Rolle spielen, die Zusammenkunfts-Zeiten sollten möglichst einfach und flexibel einstellbar sein.
Anhand dieser Dokumentation sollte es möglich sein, das Projekt nachzubauen und für eigene Zwecke anzupassen.

## Hardware

Für den Nachbau nötige Geräte sind:

- Raspberry Pi Pico 2 W
  - Wichtig ist hier vor allem das WiFi-Modul, da der Pico 2 W sonst nicht in der Lage ist, die Zeit zu synchronisieren.
  - Preis: ca 8€
  - [Link](https://botland.de/module-und-bausatze-fur-raspberry-pi-pico-2/25727-raspberry-pi-pico-2-w-rp2350-arm-cortex-m33-cyw43439-wifi-bluetooth-5056561803975.html)
- Audiomodul inkl. Lautsprecher
  - Wird direkt an den Raspberry Pi Pico gelötet
  - Preis: ca 14€
  - [Link](https://botland.de/weitere-module-fur-den-raspberry-pi-pico/20096-audioerweiterung-2x-5w-lautsprecher-fur-raspberry-pi-pico-waveshare-20167-5904422351847.html)
- Stecker
  - Damit man die beiden Teile miteinander verlöten kann
  - Preis: ca 0,50€
  - [Link](https://botland.de/zubehor-fur-den-raspberry-pi-pico/18854-ein-satz-stecker-fur-den-gpio-des-raspberry-pi-pico-5904422328511.html)

Eine Anleitung zum Löten der beiden Teile gibt es [hier](https://www.waveshare.com/wiki/Pico-Audio#Hardware_Connection).

## Software

Auf dem Pico läuft kein vollständiges Betriebssystem. Wir entwickeln direkt darauf mit einer kleinen Sprache namens MicroPython.
Um diesen Code auf den Pico zu laden, benötigen wir ein Tool namens Thonny. Dieses kann [hier](https://thonny.org/) heruntergeladen werden.

In der Regel musst du nur die WLAN-Einwahldaten deines Königreichssaals in der Datei `config.py` anpassen und den Code auf den Pico laden.

### Installation und Einrichtung

Installiere das Programm, verbinde den Pico via Micro-USB mit deinem Computer und öffne Thonny.

Starte die Thonny Python IDE, wenn die Installation abgeschlossen ist. Danach müssen wir ein paar Dinge einstellen.

Damit Thonny Deinen Raspberry Pi Pico erkennt, muss noch die virtuelle Schnittstelle ausgewählt werden. Wähle dazu im Menü „Extras / Optionen...“ im Kartenreiter „Interpreter“ im Feld „Port“ den seriellen Port aus, der Deinem Pico entspricht. Vermutlich gibt es nicht so viele Auswahlmöglichkeiten.

In der Regel ist die Thonny Python IDE nicht auf das Programmieren mit MicroPython eingestellt, sondern mit einer anderen Python-Version. Unten Rechts kann eine andere Version ausgewählt und nachinstalliert werden.

Diese Einstellungen sind gegebenenfalls zu speichern und die Thonny Python IDE neu zu starten.
Thonny fragt dann auch nach, ob es MicroPython auf dem Pico installieren soll. Das ist zu bestätigen.

Der Thonny-Editor ist dann mit dem Raspberry Pi Pico verbunden und zum Programmieren bereit, wenn im Editor-Feld „Shell“ bzw. „Kommandozeile“ eine Meldung mit „MicroPython“ und „Raspberry Pi Pico 2 W with RP2350 erscheint. Wenn diese Meldung dort nicht steht, dann besteht keine Verbindung. Die kann man durch Klicken auf „STOP“ in der Menü-Leiste manuell herstellen.

### Code anpassen und auf den Pico kopieren

... (TODO)

### Zusammenkunftszeiten anpassen

... (TODO)

### Eigener NodeJS Zeit-Server

... (TODO)
