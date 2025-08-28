# Nachbau und Dokumentation

Ziel war eine kostengünstige und freie, unabhängige Lösung, automatisch kurz vor Beginn der Zusammenkunft einen Ton abzuspielen.

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
  - Die Lautsprecher sind eventuell zu leise für größere, volle Säle - hier kann es Sinn machen, das Audiomodul direkt an die Anlage anzuschließen
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
  - Diese wird über eine API abgerufen
  - CircuitPython (die kleine Scriptsprache des `Pi Pico`) ist nicht umfangreich. Dieses Script auszulagern war der Weg mit dem geringsten Fehlerpotenzial.
  - Eine solche API muss nicht selbst zur Verfügung gestellt werden. Wenn man möchte und in der Zeitzone "Europe/Berlin" lebt, kann man gern meine API hier nutzen

Auf dem `Pico` selbst liegen folgende Daten, die angepasst werden müssen:

- WLAN-Einwahldaten (des Königreichssaals)
- URL zu der JSON mit den Zusammenkunftszeiten
- (optional) URL zu der API, die die aktuelle Zeit liefert

Das muss nur in der Datei `settings.toml` angepasst werden.

Sämtliche Codes, inkl. Beispielen und der Konfiguration für beide Plattformen, liegen in diesem Repository und können nach Belieben angepasst werden.

Hinweis: Zusätzlich zur Pico-Variante gibt es eine leichte Windows-Alternative unter `windows-client` (Tray-App), die die MP3 zu den gleichen Zeiten automatisch auf einem Windows‑PC abspielt. Sie eignet sich, wenn kein Pico eingesetzt werden soll.

### Windows (Alternative zur Pico‑Variante)

Mit dem Windows‑Client kannst du die MP3 automatisch auf einem Windows‑PC zu den Zeiten aus deiner JSON abspielen lassen – als kleine Tray‑App, die beim Start von Windows mitstarten kann.

Schritte (ohne spezielle IDE, nur die .NET‑CLI):

1. Voraussetzungen

- Windows 11
- .NET 9 SDK installiert (https://dotnet.microsoft.com/download)
- Internetzugang (für Download der Zeiten‑JSON und NTP Zeitabgleich)

2. Quellcode holen

- Dieses Repository herunterladen oder klonen (z. B. als ZIP entpacken).

3. EXE bauen (portable Einzeldatei)

- Öffne PowerShell im Projektordner und führe aus:

```powershell
# Abhängigkeiten wiederherstellen
 dotnet restore .\windows-client\src\dingdong-win.csproj

# Publish als portable, self-contained Einzeldatei
 dotnet publish .\windows-client\src\dingdong-win.csproj `
  -c Release `
  -r win-x64 `
  -p:PublishSingleFile=true `
  -p:SelfContained=true `
  -p:IncludeNativeLibrariesForSelfExtract=true `
  -o .\windows-client\publish
```

- Ergebnis: `windows-client\publish\dingdong-win.exe`

4. Erste Inbetriebnahme

- Starte `dingdong-win.exe`. Die App läuft im Tray (rechts unten). Ein Linksklick öffnet ein kleines Fenster.
- Zeiten‑URL prüfen: Standard ist `https://dingdong.tavra.de/zus-zeiten.json` (kannst du lassen).
- MP3 auswählen: Im Fenster findest du eine Auswahl aus dem lokalen Ordner `audio` (neben der EXE). Standard‑MP3s werden beim Build mit ausgeliefert. Du kannst beliebige zusätzliche MP3s einfach in diesen Ordner kopieren; die App aktualisiert die Liste automatisch.
- Countdown zeigt die verbleibende Zeit bis zum nächsten Abspielzeitpunkt.

5. Autostart (optional)

- Native AOT: Autostart bitte manuell setzen – lege eine Verknüpfung zu `dingdong-win.exe` in `%AppData%\Microsoft\Windows\Start Menu\Programs\Startup` ab.

6. Optionales Icon

- Lege `windows-client/assets/bell.svg` ab (liegt bereits bei dir). Die CI erzeugt daraus automatisch eine `bell.ico` (mehrere Größen) und bettet sie als App-/Tray‑Icon ein. Alternativ kannst du auch direkt eine `bell.ico` bereitstellen.

7. Betrieb und Hinweise

- Die App lädt die Zeiten aus der konfigurierten JSON und synchronisiert die Uhrzeit per NTP (UTC → Europe/Berlin inkl. Sommer-/Winterzeit).
- Polling der Zeiten ca. alle 60 Sekunden.
- Verpasste Termine werden nicht nachgeholt (nur zukünftige Abspielzeiten).
- Minimal‑Log mit Rotation liegt unter `%AppData%/dingDong/windows-client/app.log` (max. ~256 KB, letzte 5 Archive).
- Für NTP muss ausgehend UDP Port 123 erlaubt sein. Ist NTP vorübergehend blockiert, nutzt die App die lokale Systemzeit.

8. CI‑Build (Download)

- In diesem Repo ist ein GitHub Actions Workflow enthalten, der auf einem Windows‑Runner die AOT‑EXE baut und als Artifact/Release bereitstellt. Liegt eine `bell.svg` vor, wird automatisch eine hochwertige `bell.ico` erzeugt und in die EXE eingebettet. Du findest das fertige Binary im Actions‑Tab oder bei Releases (bei Tags).

9. Einstellungen und Portabilität

- Die App speichert Benutzer‑Einstellungen unter `%AppData%/dingDong/windows-client/settings.json` (Zeiten‑URL, MP3‑Pfad, Lautstärke).
- Die EXE ist portabel und kann in jeden beliebigen Ordner kopiert werden. Lege ein optionales `bell.ico` direkt daneben, falls du ein eigenes Icon möchtest.

10. Deinstallation

- Tray‑Menü „Beenden“ wählen, dann den Ordner `windows-client\publish` löschen.
- Optional die Einstellungen entfernen: `%AppData%/dingDong/windows-client` löschen.
- Wenn Autostart gesetzt wurde: Die Verknüpfung `dingdong-win.lnk` aus `%AppData%\Microsoft\Windows\Start Menu\Programs\Startup` entfernen.

### Pico

Auf dem Pico läuft kein vollständiges Betriebssystem (wie z.B. das `Raspberry Pi OS` seines großen Bruders). Wir entwickeln direkt darauf mit einer kleinen Sprache namens `CircuitPython` (C oder MicroPython wäre auch möglich).

Hat man ihn einmal mit den nötigen Daten konfiguriert, sollte man ihn eigentlich nie wieder anfassen müssen.

#### Installation und Einrichtung

1. Lade dir die aktuelle Version von CircuitPython von der [offiziellen Seite](https://circuitpython.org/board/raspberry_pi_pico2_w/) herunter.
2. Halte den `BOOTSEL`-Taster gedrückt, während du den Pico über Micro-USB mit dem PC verbindest. Es öffnet sich ein neues Laufwerk, auf das du die heruntergeladene .UF2-Datei kopieren musst.
3. Daraufhin wird der Pico geflasht und neu gestartet und du kannst ihn über die `CIRCUITPY`-Partition wie ein normales Laufwerk ansprechen - dieses Laufwerk sollte sich als neuer Ordner automatisch öffnen.

#### Code anpassen und auf den Pico kopieren

1. Kopiere den Inhalt des Ordners `pico` auf die `CIRCUITPY`-Partition.
   - Über [diesen Link](https://downgit.github.io/#/home?url=https://github.com/T-Philipp/dingDong/pico/) kannst du den Inhalt des Ordners als ZIP herunterladen.
2. Öffne dort die Datei `settings.toml` in einem beliebigen Texteditor und passe die Werte an:
   - `CIRCUITPY_WIFI_SSID` und `CIRCUITPY_WIFI_PASSWORD` enthalten die WLAN-Zugangsdaten deines Saals.
   - `TIME_URL` zeigt auf den API-Endpunkt mit der aktuellen, lokalen Zeit (siehe unten).
   - `ZUSZEITEN_URL` zeigt auf die URL der JSON-Datei mit den Zusammenkunftszeiten (siehe unten).
3. Eine eigene .mp3-Datei kann im Ordner `audio` abgelegt werden. Falls deren Dateiname anders ist, musst du in der `code.py` den Dateinamen anpassen.

### Webserver

Die JSON-Datei mit den Zusammenkunftszeiten muss irgendwo öffentlich abrufbar liegen. Das kann ein eigener Webserver sein oder ein Dienst wie GitHub oder GitLab.

Für den API-Endpunkt, welcher die korrekte, aktuelle Zeit unserer Zeitzone (inkl. Berücksichtigung von Sommer- und Winterzeit) berücksichtigt, ist ein NodeJS-Server nötig. Falls man keine von mir und meinem Server unabhängige Lösung möchte, kann man hier auch einfach meinen verwenden.

Beide Dinge bietet mir `Vercel` als Host mit einem Express-Server gratis an.
Änderst du z.B. hier auf GitHub die Zusammenkunftszeiten in der Repo, wird bei Commit automatisch bei Vercel ein Build angestoßen - die Änderungen sind wenige Sekunden später online. Deshalb zeige ich hier die Vorgehensweise bei diesem Weg.

#### Setup

1. Erstelle einen Fork dieses Repositories.
2. Erstelle einen Account bei Vercel.
3. Verknüpfe deinen GitHub-Account mit Vercel.
4. Wähle das Repository aus, das du gerade geforkt hast, und wähle einen Namen aus (`XYZ`).

Danach sollte unter `XYZ.vercel.app` dein persönliches Repository online sein.
Ein Aufruf von `https://XYZ.vercel.app/time` sollte ein JSON inkl. der aktuellen Zeit zurückgeben.
Ein Aufruf von `https://dingdong.tavra.de/zus-zeiten.json` sollte die Zusammenkunftszeiten ausgeben.

#### Zusammenkunftszeiten anpassen

Ändere dafür einfach den Inhalt der Datei `/sites/zus-zeiten.json` in deinem Fork.
Nachdem du die Änderungen committest, sollte Vercel automatisch ein Build anstoßen und die Änderungen online sein.

#### Eigener NodeJS Zeit-Server

Wenn du den Weg mit Vercel und `express` gegangen bist, ist dein eigener Node-Server schon bereit.
Vielleicht möchtest du noch die Zeitzone in der Datei `/api/index.ts` in deinem geforkten Repo anpassen (falls du nicht in der Zeitzone `Europe/Berlin` lebst).

Selbstverständlich kannst du auch deinen anderen Node-Server verwenden, wenn du Vercel als Host nicht verwenden möchtest.
