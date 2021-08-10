# License Manager

Der License Manager dient dazu, dass die Projekt-Lizenz auch für alle neuen Klassen gilt und, wenn nicht vorhanden, in den Kopf der Klassen hinzugefügt wird. Zusätzlich sammelt der License Manager einige statistische Informationen über die Anwendung und stellt diese dar.

## Server

Der Server beseht aus einer Menge von Endpunkten und funktioniert ohne Authentifizierung oder Autorisierung.

## Endpunkte
Die Endpunkte geben an mit welchem Verb sie zu erreichen sind, welche Formate sie akzeptieren und welche Formate sie zurückgeben.

### [GET] /license

Parameter: Keine

Request-Body: Keiner

Response-Body: `application/json`

Beispiel-Antwort:

```json
{
  "identifier": "MIT",
  "fullName": "MIT",
  "licenseText": "<MIT>"
}
```

Beschreibung: Gibt die ausgewählte Lizenz zurück, die für das Repository angewandt werden soll

### [POST] /license

Parameter: Keine

Request-Body: `application/json`

Response-Body: Keiner

Beispiel-Anfrage: 
```json
{
  "name": "MIT",
  "license": "<MIT-LICENSE>",
  "header": "<MIT-FILE-HEADER>"
}
```

Beschreibung: Fügt eine neue Lizenz dem Speicher hinzu. Dadurch kann diese Lizenz für weitere Anwendungen erkannt und genutzt werden.

### [GET] /license/crawl

Parameter: Keine

Request-Body: Keiner

Response-Body: `application/json`

Beispiel-Antwort
```json
[
  {
    "folder": "src",
    "files": [
      {
        "folder": "src",
        "file": "ExampleFile.cs",
        "license": "Apache-2.0",
        "licenseOkay": false
      }
    ],
    "folders": [
      {
        "folder": "example",
        "files": [],
        "folder": []
      }
    ]
  }
]
```

Beschreibung: Durchsucht alle unterstützen Dateien nach ihrer Lizenz und gibt das Ergebnis zurück. Für unterstützte Lizenzen wird generell der [SPDX-Identifikator](https://spdx.org/licenses/) benutzt. Dieser Endpunkt verändert die Dateien nicht. Falls keine Lizenz in den Dateien vorhanden ist, dann wird für das Feld License `null` angegeben.

### [GET] /license/apply

Parameter: Keine

Request-Body: Keiner

Response: Keine

Beschreibung: Wendet die konfigurierte Lizenz an. Alle unterstützen Dateien erhalten den entsprechenden Header und eine `LICENSE` Datei wird im Root-Verzeichnis des Repositories angelegt, das diese Lizenz enthält. Im Fehlerfall wird die Datei wiedergegeben, in der der Fehler aufgetreten ist.

### [GET] /license/third-party

Parameter: Keine

Request-Body: Keiner

Response: `application/json`

Beispiel-Antwort:

```json
[
  {
    "name": "Angular Core",
    "license": "MIT",
  }
]
```

Beschreibung: Durchsucht alle unterstützen Projektformate nach externen Abhängigkeiten und gibt deren Lizenz als [SPDX-Identifikator](https://spdx.org/licenses/) zurück, falls möglich. Wenn die Lizenz nicht gefunden wurde, dann wird `null` im Lizenzfeld angegeben. Wenn eine Lizenz nicht identifiziert werden konnte, dann wird der Wert `unknown` angegeben. Dieser Endpunkt verändert das Repository nicht.

### [GET] /license/third-party/create

Parameter: Keine

Request-Body: Keiner

Response: Keine

Beispiel-Antwort:

Beschreibung: Erstellt eine `THIRD_PARTY_LICENSES`-Datei im Root-Verzeichnis des Repositories, in der alle Abhängigkeiten mit ihren Lizenzen aufgeführt werden.
