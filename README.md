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
  - CircuitPython (die kleine Scriptsprache des `Pi Pico`) kennt z.B. Zeitzonen nicht. Dieses Script auszulagern war der Weg mit dem geringsten Fehlerpotenzial.

Auf dem `Pico` selbst liegen folgende Daten, die angepasst werden müssen:

- WLAN-Einwahldaten (des Königreichssaals)
  - Diese müssen in der Datei `settings.toml` angepasst werden
- URL zu der JSON mit den Zusammenkunftszeiten
  - Diese muss in der Datei `settings.toml` angepasst werden
- (optional) URL zu der API, die die aktuelle Zeit liefert
  - Diese muss in der Datei `settings.toml` angepasst werden

Sämtliche Codes, inkl. Beispielen und der Konfiguration für beide Plattformen, liegen in diesem Repository und können nach Belieben angepasst werden.

### Pico

Auf dem Pico läuft kein vollständiges Betriebssystem. Wir entwickeln direkt darauf mit einer kleinen Sprache namens CircuitPython.

Hat man ihn einmal mit den nötigen Daten konfiguriert, sollte man ihn eigentlich nie wieder anfassen müssen.

#### Installation und Einrichtung

Lade dir die aktuelle Version von CircuitPython von der [offiziellen Seite](https://circuitpython.org/board/raspberry_pi_pico2_w/) herunter.
Halte den `BOOTSEL`-Taster gedrückt, während du den Pico über Micro-USB mit dem PC verbindest. Es öffnet sich ein neues Laufwerk, auf das du die heruntergeladene .UF2-Datei kopieren musst.
Daraufhin wird der Pico geflasht und neu gestartet und du kannst ihn über die `CIRCUITPY`-Partition wie ein normales Laufwerk ansprechen - dieses Laufwerk sollte sich als neuer Ordner automatisch öffnen.

#### Code anpassen und auf den Pico kopieren

1. Kopiere den Inhalt des Ordners `pico` auf die `CIRCUITPY`-Partition
   - Über [diesen Link](https://downgit.github.io/#/home?url=https://github.com/T-Philipp/dingDong/pico) kannst du den Inhalt des Ordners als ZIP herunterladen
2. Öffne dort die Datei `settings.toml` in einem beliebigen Texteditor und passe die Werte an
   - `CIRCUITPY_WIFI_SSID` und `CIRCUITPY_WIFI_PASSWORD` enthalten die WLNA-Zugangsdaten deines Saals
   - `TIME_URL` zeigt auf den API-Endpunkt mit der aktuellen, lokalen Zeit (siehe unten)
   - `ZUSZEITEN_URL` zeigt auf die URL der JSON-Datei mit den Zusammenkunftszeiten (siehe unten)
3. Eine eigene .mp3-Datei kann im Ordner `audio` abgelegt werden. Falls deren Dateiname anders ist, musst du in der `code.py` den Dateinamen anpassen.

### Webserver

Die JSON-Datei mit den Zusammenkunftszeiten muss irgendwo öffentlich abrufbar liegen. Das kann ein eigener Webserver sein oder ein Dienst wie GitHub oder GitLab.

Für den API-Endpunkt, welcher die korrekte, aktuelle Zeit unserer Zeitzone (inkl. Berücksichtigung von Sommer- und Winterzeit) berücksichtigt, ist ein NodeJS-Server nötig. Falls man keine von mir und meinem Server unabhängige Lösung möchte, kann man hier auch einfach meinen verwenden.

Beide Dinge bietet mir `Vercel` als Host mit einem Express-Server gratis an.
Änderst du z.B. hier auf GitHub die Zusammenkunftszeiten in der Repo, wird bei Commit automatisch bei Vercel ein Build angestoßen - die Änderungen sind wenige Sekunden später online. Deshalb zeige ich hier die Vorgehensweise bei diesem Weg.

#### Setup

1. Erstelle einen Fork dieses Repositories
2. Erstelle einen Account bei Netlify
3. Verknüpfe deinen GitHub-Account mit Netlify
4. Wähle das Repository aus, das du gerade geforkt hast, und wähle einen Namen aus (`XYZ`)

Danach sollte unter `XYZ.vercel.app` dein persönliches Repository online sein.
Ein Aufruf von `https://XYZ.vercel.app/time` sollte ein JSON inkl. der aktuellen Zeit zurückgeben.
Ein Aufruf von `https://XYZ.vercel.app/zus-zeiten.json` sollte die Zusammenkunftszeiten ausgeben.

#### Zusammenkunftszeiten anpassen

Ändere dafür einfach den Inhalt der Datei `/sites/zus-zeiten.json` in deinem Fork.
Nachdem du die Änderungen committest, sollte Netlify automatisch ein Build anstoßen und die Änderungen online sein.

#### Eigener NodeJS Zeit-Server

Wenn du den Weg mit Vercel und `express` gegangen bist, ist dein eigener Node-Server schon bereit.
Vielleicht möchtest du noch die Zeitzone in der Datei `/api/index.mts` in deinem geforkten Repo anpassen (falls du nicht in der Zeitzone `Europe/Berlin` lebst).

Selbstverständlich kannst du auch deinen anderen Node-Server verwenden, wenn du Vercel als Host nicht verwenden möchtest.
