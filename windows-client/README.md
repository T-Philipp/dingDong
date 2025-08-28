# dingDong Windows Client

Eine leichte, portable Windows-App (Tray-Only) zum automatischen Abspielen einer MP3 zu den in `zus-zeiten.json` definierten Zeiten. Die App lädt die Zeiten direkt von `https://dingdong.tavra.de/zus-zeiten.json` (anpassbar) und ermittelt die aktuelle Zeit selbst via NTP.

Funktionen:

- Startet mit Windows (optional) und minimiert sich in die Taskleiste (Tray).
- Kleines Konfig-Fenster über Tray-Icon-Klick.
- Countdown bis zum nächsten Abspielzeitpunkt.
- Auswahl der lokalen MP3-Datei.
- Eigene Abspiel-Lautstärke (unabhängig von der Systemlautstärke; nur für die App).
- Anzeige der aktuell vom Server gelieferten Zeiten.
- Optionales Autostart.
- Minimal-Logging mit Rotation (max. ~256 KB, letzte 5 Archive).

Technik:

- .NET 9, Windows Forms, single-file, self-contained x64.
- MP3-Wiedergabe via NAudio.
- NTP-basierte Zeitsynchronisation (UTC → Europe/Berlin inkl. DST).
- Einstellungen werden pro User in `%AppData%/dingDong/windows-client/settings.json` gespeichert.

# dingDong Windows Client

Die vollständige Anleitung (Bau, Einrichtung, Betrieb) findest du zentral in der Root‑`README.md` im Abschnitt „Windows (Alternative zur Pico‑Variante)“.

Kurzüberblick:

- Build: .NET 9, portable Single‑File EXE; Output unter `windows-client\publish\dingdong-win.exe`.
- Zeiten‑URL: Standard `https://dingdong.tavra.de/zus-zeiten.json` (änderbar im Fenster).
- Zeitquelle: NTP (UTC → Europe/Berlin inkl. DST).
- Einstellungen/Logs: `%AppData%/dingDong/windows-client/`.

```powershell

```
