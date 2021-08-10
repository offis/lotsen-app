# Release veröffentlichen

Folgende Schritte müssen unternommen werden, um ein Release vorzubereiten

  1. Der Versions-Manager sollte mit der neuen Version ausgeführt werden
  2. Fehlende Assembly-Infos in neuen Projekten sollen ergänzt werden.
  3. ~~Das Release muss signiert werden.~~
  4. Das Changelog muss weitergeführt werden.
  5. Den LicenseManager ausführen


## Versions-Manager

Über `LotsenApp.VersionManager.exe` kann die aktuelle Version herausgefunden werden. Zum Setzen einer Version wird der Version-Manager mit folgender Kommandozeile aufgerufen:

```bash
LotsenApp.VersionManager -a Set -v <Version nach SemVer>
```

In der Solution sind auch Konfigurationen für das Ausführen des Version-Managers vorhanden. Der Version-Manager muss innerhalb des Git-Repositories ausgeführt werden.

## Assembly-Informationen

Feld                | Wert
--------------------|-----------------------------------------
Version             | Initiale Version: 1.0.0, sonst Inkrement nach SemVer
Autoren             | Autor des Projekts
Copyright           | © `<Jahr>` OFFIS e.V.
Firma               | OFFIS e.V.
Assembly-Version    | Initiale Version: 1.0.0.0, sonst Inkrement nach SemVer + Build
Datei-Version       | Initiale Version 1.0.0.0, sonst Inkrement nach SemVer + Build

## Signieren des Release

Das Signieren geschieht automatisch, wenn die Umgebungsvariablen `CSC_LINK`, und optional `CSC_KEY_PASSWORD` gesetzt werden. Für die Signatur soll das Stroke Owl Entwicklerszertifikat verwendet werden.