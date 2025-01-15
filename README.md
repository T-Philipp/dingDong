# Nachbau und Dokumentation

Ziel war eine kostengünstige und freie, unabhängige Lösung, automatisch eine Minute vor Beginn der Zusammenkunft einen Ton abzuspielen.

Änderungen an der Sommer-/Winterzeit sollten automatisch berücksichtigt werden, die Zusammenkunfts-Zeiten sollten möglichst einfach und flexibel einstellbar sein.
Anhand dieser Dokumentation sollte es möglich sein, das Projekt nachzubauen und für eigene Zwecke anzupassen.

## Hardware

Für den Nachbau nötige Geräte sind:

- Raspberry Pi Pico 2 W
  - Wichtig ist hier vor allem das WiFi-Modul, da der Pico 2 W sonst nicht in der Lage ist, die Zeit zu synchronisieren.
  - Preis: ca. 8€
  - [Link](https://botland.de/module-und-bausatze-fur-raspberry-pi-pico-2/25727-raspberry-pi-pico-2-w-rp2350-arm-cortex-m33-cyw43439-wifi-bluetooth-5056561803975.html)
- Audiomodul inkl. Lautsprecher
  - Wird direkt an den Raspberry Pi Pico gelötet
  - Preis: ca. 14€
  - [Link](https://botland.de/weitere-module-fur-den-raspberry-pi-pico/20096-audioerweiterung-2x-5w-lautsprecher-fur-raspberry-pi-pico-waveshare-20167-5904422351847.html)
- Stecker
  - Damit man die beiden Teile miteinander verlöten kann
  - Preis: ca. 0,50€
  - [Link](https://botland.de/zubehor-fur-den-raspberry-pi-pico/18854-ein-satz-stecker-fur-den-gpio-des-raspberry-pi-pico-5904422328511.html)

Eine Anleitung zum Löten der beiden Teile gibt es [hier](https://www.waveshare.com/wiki/Pico-Audio#Hardware_Connection).

## Software

### Übersicht

Daten, die sich häufig ändern können, werden nicht auf dem `Pico` gespeichert, sondern auf einem Webserver. Der Pico synchronisiert sich regelmäßig mit diesem Server und holt sich die aktuellen Daten.

Der Pico holt vom `Webserver` folgende Daten:

- Zusammenkunftszeiten
  - Diese liegen in Form einer JSON-Datei irgendwo öffentlich über das WWW abrufbar
- Aktuelle, lokale Zeit
  - Das muss nicht selbst zur Verfügung gestellt werden. Wenn man möchte und in der Zeitzone "Europe/Berlin" lebt, kann man gern meine API hier nutzen
  - Diese wird über eine API abgerufen
  - MicroPython (die kleine Scriptsprache des `Pi Pico`) kennt z.B. Zeitzonen nicht. Dieses Script auszulagern war der Weg mit dem geringsten Fehlerpotenzial.

Auf dem `Pico` selbst liegen folgende Daten, die angepasst werden müssen:

- WLAN-Einwahldaten (des Königreichssaals)
  - Diese müssen in der Datei `config.py` angepasst werden
- URL zu der JSON mit den Zusammenkunftszeiten
  - Diese muss in der Datei `config.py` angepasst werden
- (optional) URL zu der API, die die aktuelle Zeit liefert
  - Diese muss in der Datei `config.py` angepasst werden

Sämtliche Codes, inkl. Beispielen und der Konfiguration für beide Plattformen, liegen in diesem Repository und können nach Belieben angepasst werden.

### Pico

Auf dem Pico läuft kein vollständiges Betriebssystem. Wir entwickeln direkt darauf mit einer kleinen Sprache namens MicroPython.
Um diesen Code auf den Pico zu laden, benötigen wir ein Tool namens Thonny. Dieses kann [hier](https://thonny.org/) heruntergeladen werden.

Hat man ihn einmal mit den nötigen Daten konfiguriert, sollte man ihn eigentlich nie wieder anfassen müssen.

#### Installation und Einrichtung

Installiere das Programm, verbinde den Pico via Micro-USB mit deinem Computer und öffne Thonny.

Starte die Thonny Python IDE, wenn die Installation abgeschlossen ist. Danach müssen wir ein paar Dinge einstellen.

Damit Thonny deinen Raspberry Pi Pico erkennt, muss noch die virtuelle Schnittstelle ausgewählt werden. Wähle dazu im Menü "Extras / Optionen..." im Reiter "Interpreter" im Feld "Port" den seriellen Port aus, der deinem Pico entspricht. Vermutlich gibt es nicht so viele Auswahlmöglichkeiten.

In der Regel ist die Thonny Python IDE nicht auf das Programmieren mit MicroPython eingestellt, sondern mit einer anderen Python-Version. Unten rechts kann eine andere Version ausgewählt und nachinstalliert werden.

Diese Einstellungen sind gegebenenfalls zu speichern und die Thonny Python IDE neu zu starten.
Thonny fragt dann auch nach, ob es MicroPython auf dem Pico installieren soll. Das ist zu bestätigen.

Der Thonny-Editor ist dann mit dem Raspberry Pi Pico verbunden und zum Programmieren bereit, wenn im Editor-Feld "Shell" bzw. "Kommandozeile" eine Meldung mit "MicroPython" und "Raspberry Pi Pico 2 W with RP2350" erscheint. Wenn diese Meldung dort nicht steht, dann besteht keine Verbindung. Diese kann man durch Klicken auf "STOP" in der Menü-Leiste manuell herstellen.

#### Code anpassen und auf den Pico kopieren

... (TODO)

### Webserver

Die JSON-Datei mit den Zusammenkunftszeiten muss irgendwo öffentlich abrufbar liegen. Das kann ein eigener Webserver sein oder ein Dienst wie GitHub oder GitLab.

Für den API-Endpunkt, welcher die korrekte, aktuelle Zeit unserer Zeitzone (inkl. Berücksichtigung von Sommer- und Winterzeit) berücksichtigt, ist ein NodeJS-Server nötig. Falls man keine von mir und meinem Server unabhängige Lösung möchte, kann man hier auch einfach meinen verwenden.

Beide Dinge bietet mir `Netlify` als Host mit seinen Netlify-Functions gratis an.
Ändere ich z.B. hier auf GitHub die Zusammenkunftszeiten in der Repo, wird bei Commit automatisch bei Netlify ein Build angestoßen - die Änderungen sind wenige Sekunden später online. Deshalb zeige ich hier die Vorgehensweise bei diesem Weg.

#### Setup

1. Erstelle einen Fork dieses Repositories
2. Erstelle einen Account bei Netlify
3. Verknüpfe deinen GitHub-Account mit Netlify
4. Wähle das Repository aus, das du gerade geforkt hast, und wähle einen Namen aus (`XYZ`)

Danach sollte unter `XYZ.netlify.app` dein persönliches Repository online sein.
Ein Aufruf von `https://XYZ.netlify.app/.netlify/functions/time` sollte ein JSON inkl. der aktuellen Zeit zurückgeben.
Ein Aufruf von `https://XYZ.netlify.app/zus-zeiten.json` sollte die Zusammenkunftszeiten ausgeben.

#### Zusammenkunftszeiten anpassen

Ändere dafür einfach den Inhalt der Datei `/zus-zeiten.json` in deinem Fork.
Nachdem du die Änderungen committest, sollte Netlify automatisch ein Build anstoßen und die Änderungen online sein.

#### Eigener NodeJS Zeit-Server

Wenn du den Weg mit Netlify und den `netlify-functions` gegangen bist, ist dein eigener Node-Server schon bereit.
Vielleicht möchtest du noch die Zeitzone in der Datei `/netlify/functions/time.mts` in deinem geforkten Repo anpassen (falls du nicht in der Zeitzone `Europe/Berlin` lebst).

Selbstverständlich kannst du auch deinen anderen Node-Server verwenden, wenn du Netlify als Host nicht verwenden möchtest.
