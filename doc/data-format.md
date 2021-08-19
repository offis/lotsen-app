# Inhaltsverzeichnis
1. [Metadaten-Format der LotsenApp](#metadaten-format-der-lotsenapp)
2. [Projektdefinition](#projektdefinition)
3. [Lokalisation](#lokalisation)
4. [Datendefinition](#datendefinition)
   1. [Dokumentationsereignisse](#dokumentationsereignisse)
   2. [Dokumente](#dokumente)
   3. [Gruppen](#gruppen)
   4. [Datenfelder](#datenfelder)
   5. [Datentypen](#datentypen)
   6. [LotsenApp Data Definition Language](#lotsenapp-data-definition-language)
5. [Datendarstellung](#datendarstellung)
   1. [Darstellung der Dokumentationsereignisse](#darstellung-der-dokumentationsereignisse)
   2. [Darstellung der Dokumente](#darstellung-der-dokumente)
   3. [Darstellung der Gruppen](#darstellung-der-gruppen)
   4. [Darstellung der Datenfelder](#darstellung-der-datenfelder)
   5. [Darstellung der Datentypen](#darstellung-der-datentypen)
   6. [LotsenApp Data Display Language](#lotsenapp-data-display-language)
6. [Datenvalidierung](#datenvalidierung)
7. [Datenverarbeitung](#datenverarbeitung)

# Metadaten-Format der LotsenApp

Die LotsenApp definiert an Stelle eines Datenformats ein Meta-Datenformat, um der Definition des Belotsungsprozesses möglichst viele Freiheiten zu geben. Generell wird das Meta-Datenformat von 4 Säulen gestützt: die Datendefinition, die Datendarstellung, die Datenvalidierung und die Datenverarbeitung. Die aktuelle Revision des Datenformats ist `mk3`.

Die Definition eines Belotsungspfads besteht aus 2 Arten von Dateien: einer Projektdefinition und Lokalisierungen für das Projekt. Beide Dateien werden im JSON-Format erstellt. Das Schema der Dateien wird in diesem Dokument beschrieben. Diese Dateien können unverschlüsselt und verschlüsselt in der LotsenApp vorliegen. Die Bezeichner folgen der `PascalCase`-Konvention

## Projektdefinition

Die Projektdefinition baut sich nach dem Schema aus dem folgenden Listing auf. Die Definitionsdatei muss nach dem Schema `{ProjectId}.json` benannt werden. Identifikatoren können frei gewählt werden, aber GUIDs sind eine gute Möglichkeit die globale Eindeutigkeit ohne Prüfung vorhandener Identifikatoren zu erhalten, auch wenn das GUID-Verfahren keine Garantie für globale Eindeutigkeit gibt, da die Bildung ein probabilistischer Algorithmus ist. Generell verlangt das Datenformat eine kontextuelle Eindeutigkeit von Identifikatoren, bspw. müssen alle Projekte einen eindeutigen Identifikator enthalten.

```json
{
  "Id": string, // Eindeutige Identifizierung für das Projekt
  "Name": string, // Name des Projekts. Wird verwendet, wenn 
                  // in der Lokalisation kein Wert vorhanden ist.
  "I18NKey": string, // Internationalisierung-Schlüssel für 
                     // die Lokalisation in Punkt-Notation
  "OpenForDocumentation": boolean, // false: Benutzer muss 
        // Teil des Projekts sein, um auf den Pfad zugreifen 
        // zu können. Eine Serverkomponente zur Zuweisung des 
        // Benutzers zum Projekt ist erforderlich. true: 
        // Jeder, der den Pfad hat, kann ihn benutzen.
  "DataFormatVersion": number, // Referenz zum verwendeten Datenformat
  "DataDefinition": DataDefinition, // Objekt zur Definition der Daten
  "DataDisplay": DataDisplay, // Objekt zur Darstellung der Daten
  "DataValidation": DataValidation, // Noch nicht umgesetzt
  "DataProcess": DataProcess // Noch nicht umgesetzt
}
```

Beispiel für eine Projektdefinition:

```json
{
  "Id": "01e59baa-775a-4dc1-bb51-ef618b7b505b",
  "Name": "Beispiel-Belotsung",
  "I18NKey": "Sample.Name",
  "OpenForDocumentation": true,
}
```

## Lokalisation

Die Lokalisationsdateien sind ebenfalls JSON-Dateien nach der Namenskonvention `{ProjectId}_{ISO 639-1 Sprachidentifikation}.json`, d.h. für das Beispielprojekt wäre der Dateiname für eine deutsche Lokalisation `01e59baa-775a-4dc1-bb51-ef618b7b505b_de.json`. Als Konvention (siehe Beispiel unten) sollte jedes Projekt einen Hauptknoten für die Punktnotation definieren. Interpolation ist für diese Werte nicht vorgesehen. Der Hauptknoten für das Beispielprojekt ist `Sample` und durch den Punkt werden die Hierarchien von einander getrennt.

```json
{
  "Sample": {
    "Name": "Beispiel" // Adressierbar in der Projektdefinition 
                       // durch Sample.Name
  }
}
```

## Datendefinition

Das Datendefinitionsobjekt dient als Hauptknoten für die Datendefinition. Die Datendefinition setzt sich aus 5 Bausteinen zusammen: Dokumentationsereignisse, Dokumente, Gruppen, Datenfelder und Datentypen. Im Folgenden werden diese Bausteine nach dem Top-Down Prinzip näher erläutert.

```json
DataDefinition: {
  "DocumentationEvents": DocumentationEvent[], // Feld für 
          // Dokumentationsereignisse
  "Documents": Document[], // Feld zur Dokumentendefinition
  "Groups": Group[], // Feld zur Gruppendefinition
  "DataFields": DataField[], // Feld zur Definition von Datenfeldern
  "DataTypes": DataType[] // Feld zur Datentyp-Definition
}
```

### Dokumentationsereignisse

Die Dokumentationsereignisse bilden das oberste Element der Datendefinition und sollen angeben, wann oder aus welchem Grund etwas dokumentiert wird. Dokumentationsereignisse können vom Benutzer erstellt werden.

```json
DocumentationEvent: {
  "Id": string, // Identifikator des Dokumentationsereignisses
  "DocumentId": string, // Identifikator für das Dokument, das für 
      // das Dokumentationsereignis angezeigt werden soll. Diese 
      // ID muss im Dokumenten-Feld der Datendefinition vorhanden 
      // sein
  "Name": string, // Name des Dokumentationsereignisses. Wird 
      // angezeigt, wenn keine Lokalisation vorhanden ist 
      // (siehe Darstellung)
  "DaysAfterIncident": integer | null, // Angabe, wann das 
      // Dokumentationsereignis auftritt. Diese Angabe erfolgt 
      // in Tagen nach der Erstellung(=Rekrutierung) des 
      // Patienten. Falls der Wert null angegeben wird, 
      // existiert kein fester Zeitpunkt für das 
      // Dokumentationsereignis.
  "ValidDocuments": string[] // Liste von IDs für Dokumente, 
      // die als Teil des Dokumentationsereignisses erstellt 
      // werden können. Diese IDs müssen im Dokumentenfeld 
      // der Datendefinition vorhanden sein.
}
```

### Dokumente

Dokumente sollen thematisch Informationen zusammenfassen, analog zu den papierbasierten Formularen. Die Dokumente selber definieren noch keine Daten. Dokumente können vom Benutzer erstellt werden.

```json
Document: {
  "Id": string, // Identifikator des Dokuments. Wird von 
      // Dokumentationsereignissen zur Referenzierung benutzt.
  "Name": string, // Name des Dokuments. Wird 
      // angezeigt, wenn keine Lokalisation vorhanden ist 
      // (siehe Darstellung)
  "DocumentType": 0 | 1, // Art des Dokument: 0 gibt ein 
      // Projektdokument an, 1 gibt ein Dokument an, das 
      // der Lotse selber erstellt hat.
  "DataFields": string[], // Referenzierung aller im 
      // Dokument vorkommender Datenfelder
  "Groups": string[], // Referenzierung alle im 
      // Dokument vorkommender Gruppen
}
```

### Gruppen

Gruppen fassen andere Gruppen und Datenfelder zusammen. Es können häufig zusammen vorhandene Datenfelder durch eine Gruppe zusammengefasst werden, um die Zahl der Referenzen zu reduzieren und bei Erweiterungen dieser Datenfelder alle Vorkommen gleichzeitig zu aktualisieren. Gruppen können außerdem auch andere Gruppen enthalten, wodurch eine maximale Flexibilität erreicht werden kann (bspw. zum Erstellen von Kontakten und Adressen der Kontakte). Die Schachtelung der Gruppen sollte maximal 99 Gruppen betragen und Endlosschleifen sollten vermieden werden. Ab einer Tiefe von 1000 Gruppen wird ein Fehler geworfen.

```json
Group: {
  "Id": string, // Identifikator der Gruppe. Wird von anderen 
      // Gruppen oder Dokumenten zur Referenzierung benutzt.
  "Name": string, // Name des Dokuments. Wird angezeigt, wenn 
      // keine Lokalisation vorhanden ist (siehe Darstellung)
  "Cardinality": 0 | 1, // Angabe wie oft die Gruppe vorkommen darf. 
      // 0: die Gruppe kommt exakt einmal vor und wird auch 
      // automatisch erstellt, wenn das Dokument erstellt wird. 
      // 1: die Gruppen kann mehr als einmal vorkommen und wird 
      // nicht automatisch erstellt.
  "Children": string[], // Referenz zu anderen Gruppen, sodass 
      // diese als Kindgruppe angezeigt werden.
  "Fields": string[] // Referenz zu Datenfeldern, die als Teil 
      // dieser Gruppe angezeigt werden sollen.
}
```

### Datenfelder

Datenfelder sollen einem Benutzer Hinweise dazu geben, was eingegeben werden soll. Das Datenfeld ist insbesondere für die Darstellung gedacht.

```json
DataField: {
  "Id": string, // Identifikator des Datenfelds. Wird von Gruppen 
      // und Dokumenten zur Referenzierung benutzt. 
  "Name": string, // Name des Datenfelds. Wird angezeigt, wenn 
      // keine Lokalisation vorhanden ist (siehe Darstellung)
  "Expression": string, // Ausdruck in LotsenApp DataDefinition 
      // Language (LADDL). Der Parser ist noch nicht implementiert 
      // und dieses Feld wird ignoriert.
  "DataType": string // Referenz zum Datentyp des Feldes.
}
```

### Datentypen

Die Datentypen definieren welche Daten für ein Datenfeld eingegeben werden können. Die beiden Felder `Type` und `Values` hängen eng zusammen und definieren welche Werte eingegeben und später für Auswertungen benutzt werden können.

```json
DataType: {
  "Id": string, // Identifikator des Datentyps. Wird von 
      // Datenfeldern zur Referenzierung benutzt
  "Name": string, // Name des Datentyps. Dieser Name wird 
      // in der LotsenApp nicht angezeigt, ist aber 
      // insbesondere zur Kommunikation als menschenlesbarer 
      // Bezeichner gedacht.
  "Type": 0 | 1 | 2 | 3 | 4 | 5 | 6, // Basistyp des Datentyps. 
      // Diese werden später erläutert
  "Values": string // Basistyp abhängige Definition oder 
      // Einschränkung von Werten
}
```

Type | Basisdatentyp | Values (Beispiel) | Beschreibung Values
-----|---------------|-------------------|-------------
0    | String        |                   | Nicht anwendbar, da Freitextfeld
1    | Integer       | `0+,[1;5],[7;9)`  | Ganzzahlen: Positive Zahlen inkl. 0, Intervall 1 bis 5, Halboffenes Intervall von 7 bis 9. Einzelne Ausdrücke können kommasepariert kombiniert (ODER-Verknüpfung) werden.
2    | Double        | `0+,[1;5],[0;1)`  | Gleitkommazahlen: Positive Zahlen inkl. 0, Intervall 1 bis 5, halboffenes Intervall 0 bis 1. Einzelne Ausdrücke können kommasepariert kombiniert (ODER-Verknüpfung) werden.
3    | Enumerable    | `yes,no,maybe`    | Kommaseparierte Aufzählung von Werten einer Auswahlliste. Der Wert `custom` bedeutet, dass zu den vorgegebenen Werten auch eine Freitexteingabe möglich ist.
4    | Boolean       |                   | Nicht anwendbar, da Ja/Nein Auswahl
5    | Date          | `date-only`       | Datumsangabe mit Zeit. Wenn unter Values `date-only` angegeben wurde, dann fällt die Auswahl der Zeit weg. 
6    | Custom        | `{id}`          | Falls ein Wert nicht durch die Basisdatentypen dargestellt werden kann oder besondere Anforderungen an den Datentyp bestehen, dann kann auf eine besondere Implementierung mit einem Identifikator des Adapter-Frameworks hingwiesen werden. 

### LotsenApp Data Definition Language
TBD

## Datendarstellung
Die zweite Säule der Daten ist die Darstellung. Das Darstellungsobjekt ist analog zum Definitionsobjekt. Es befindet sich lediglich ein weiterer Knoten in dem Objekt, der für die Anzeige von Dokumenten benutzt wird, die ein Benutzer ohne Dokumentationsereignisse erstellen können soll. In dieser Säule werden nur Darstellungsinformationen definiert und keine Daten.

> Diese Säule befindet sich noch im Aufbau und die Möglichkeiten der Darstellung kann noch erweitert werden.

```json
DataDisplay: {
  "DocumentationEvents": DocumentationEventDisplay[], 
      // Darstellungsinformationen zu Dokumentationsereignissen
  "Documents": DocumentDisplay[], // Darstellungsinformationen zu 
      // Dokumenten
  "Groups": GroupDisplay[], // Darstellungsinformationen zu Gruppen
  "DataFields": DataFieldDisplay[], // Darstellungsinformationen zu 
      // Datenfeldern
  "DataTypes": DataTypeDisplay[], // Darstellungsinformationen zu 
      // Datentypen
  "TopLevelDocuments": string[], // Referenzierung zu Dokumenten, 
      // die auf der gleichen Ebene wie Dokumentationsereignisse 
      // angezeigt werden sollen
  "Colors": string[] // Farben, die in der LotsenApp einem
      // Teilnehmer zugeordnet werden können
}
```

### Darstellung der Dokumentationsereignisse

Wie die meisten Objekte der Darstellung existiert eine Referenz zur Datendefinition, hier zu einem Dokumentationsereignis zusammen mit Information wie das Dokumentationsereignis in den Listen und in verschiedenen Sprachen dargestellt werden soll.

```json
DocumentationEventDisplay: {
  "Id": string, // Identifikator des Dokumentationsereignisse der 
      // Datendefinition
  "Ordinal": integer | null, // Ordnungszahl des 
      // Dokumentationsereignisses, um die Anzeigereihenfolge 
      // festzulegen. Falls der Wert nie gesetzt wird, wird die 
      // Definitionsreihenfolge benutzt
  "I18NKey": string, // Verweis auf den Eintrag der 
      // Lokalisationsdatei in Punktnotation
  "ValidDocuments": DocumentDocumentationEventDisplay[] 
      // Anzeigeinformationen für Dokumente unterhalb des 
      // Dokumentationsereignisses
}
```

Des Weiteren können für jedes Dokumentationsereignis auch die Reihenfolge der Dokumente durch das Feld `ValidDocuments` festgelegt werden. Wenn die Ordnungszahlen im Allgemeinen nicht angegeben werden, wird die Definitionsreihenfolge benutzt, um die Dokumente anzuzeigen. Sollten Ordnungszahlen nur teilweise angegeben werden, werden diese priorisiert und alle Entitäten ohne Ordnungszahlen werden unterhalb der Entitäten mit Ordnungszahlen angezeigt.

```json
DocumentDocumentationsEventDisplay: {
  "Id": string, // Referenz zu einem Dokument aus der 
      // Datendefinition
  "Ordinal": integer | null // Ordnungszahl, um die 
      // Anzeigereihenfolge unterhalb des 
      // Dokumentationsereignisses festzulegen
}
```

### Darstellung der Dokumente

Die Darstellung der Dokumente definiert neben dem Anzeigenamen auch wie die Datenfelder und Gruppen in dem Dokument dargestellt werden.

> Bisher werden zuerst die Datenfelder nach der Ordnungszahl und danach die Gruppen angezeigt. In der Umsetzung der `mk5`-Spezifikation werden Gruppen und Datenfelder in einer Liste angezeigt, sodass die strikte Reihenfolge _Datenfelder, Gruppen_ nicht mehr existiert.

```json
DocumentDisplay: {
  "Id": string, // Referenz zu einem Dokument aus der 
      // Datendefinition
  "I18NKey": string, // Verweis auf den Eintrag der 
      // Lokalisationsdatei in Punktnotation
  "DataFields": DocumentDataFieldDisplay[], 
      // Anzeigeinformationen für Datenfelder des 
      // Dokuments
  "Groups": DocumentGroupDisplay[] // Anzeigeinformation 
      // für Gruppen des Dokuments
}
```

Die folgenden Objekte sind dazu gedacht die Reihenfolge der Datenfelder und Gruppen zu konfigurieren.

> Zukünftig kann hier auch eine Alternative Darstellung ausgewählt werden, sodass bspw. Kontakte eine auf Kontakte spezialisierte Darstellung erhalten.

```json
DocumentDataFieldDisplay: {
  "Id": string, // Referenz zu einem Datenfeld aus der Datendefinition
  "Ordinal": integer | null // Ordnungszahl, um die Anzeigereihenfolge 
      // unterhalb des Dokuments festzulegen
}
```
```json
DocumentGroupDisplay: {
  "Id": string, // Referenz zu eine Gruppe aus der Datendefinition
  "Ordinal": integer | null // Ordnungszahl, um die Anzeigereihenfolge 
      // unterhalb des Dokuments festzulegen
}
```

### Darstellung der Gruppen

Die Darstellung der Gruppen ist insbesondere für die Reihenfolge der Datenfelder und Gruppen, die unter dieser Gruppe liegen, zuständig. Außerdem hat jede Gruppe eine Überschrift, die durch den Schlüssel lokalisiert werden kann.

```json
GroupDisplay: {
  "Id": string, // Referenz zu einer Gruppe der Datendefinition
  "Ordinal": string, // Ordnungszahl, um die Anzeigereihenfolge 
      // festzulegen
  "I18NKey": string, // Verweis auf einen Eintrag in der 
      // Lokalisationsdatei in Punktnotation
  "DataFields": GroupDataFieldDisplay[], // Anordnung der 
      // Datenfelder in der Gruppe
  "Children": GroupDisplay[] // Anordnung der Kind-Gruppen in 
      // der Gruppe
}
```

Die beiden folgenden Objekte legen die Darstellungsreihenfolge analog zur Dokumentendarstellung fest.

```json
GroupDataFieldDisplay: {
  "Id": string, // Referenz zu einem Datenfeld aus der Datendefinition
  "Ordinal": integer | null // Ordnungszahl, um die Anzeigereihenfolge 
      // unterhalb der Gruppe festzulegen
}
```

```json
GroupDisplay: {
  "Id": string, // Referenz zu eine Gruppe aus der Datendefinition
  "Ordinal": integer | null // Ordnungszahl, um die 
      // Anzeigereihenfolge unterhalb des Dokuments festzulegen
}
```

### Darstellung der Datenfelder

Die Datenfelder besitzen neben dem üblichen Lokalisationsschlüssel die Möglichkeit besondere Darstellungsinformation über die LotsenApp Data Display Language zu setzen. Dies umfasst bspw. das Setzen von Prä- und Postfixen für die Datenfelder.

```json
DataFieldDisplay: {
  "Id": string, // Referenz zu einem Datenfeld der Datendefinition
  "I18NKey": string, // Verweis auf einen Eintrag in der 
      // Lokalisationsdatei in Punktnotation
  "Expression": string, // Dynamische Darstellung nach LotsenApp 
      // Data Display Language
}
```

### Darstellung der Datentypen

Die Darstellung der Datentypen soll die Möglichkeit bieten unterschiedliche Darstellungswerte für den gleichen internen Wert anzubieten. Dies ist beispielsweise bei der Darstellung von ICD Codes sinnvoll, da hier der Code, die Beschreibung des Codes und die Beschreibung des Pfads als 3 unterschiedliche Darstellungsmöglichkeiten benutzt werden kann.

```json
DataTypeDisplay: {
  "Id": string, // Referenz zu einem Datentypen der Datendefinition
  "Expression": string, // Dynamische Darstellung nach LotsenApp 
      // Data Display Language
  "DisplayValues": [string]: string[] // Abbildung der internen 
      // Werte auf eine Menge von Verweisen in der 
      // Lokalisationsdatei. Die Einträge werden als 
      // unterschiedliche Darstellungsmöglichkeiten der internen 
      // Werte benutzt
}
```

Die Menge der Darstellungswerte hängt vom ausgewählten Wert ab, d.h. es ist möglich, dass interne Werte auf eine unterschiedliche Menge von Darstellungswerten abgebildet werden können. Es ist aber empfehlenswert, dass es immer die gleiche Menge von Darstellungswerten für jede Ausprägung eines Datentyps existiert.

Im Folgenden soll die Definition der Darstellungswert am Beispiel eines ICD-Codes verdeutlicht werden.

```json
{
  "Id": "random-guid-for-icd",
  "Expression": null,
  "DisplayValues": {
    "C50": [
      "icd.C50.code",
      "icd.c50.short",
      "icd.c50.long"
    ]
  }
}
```

### LotsenApp Data Display Language

TBD

## Datenvalidierung

TBD

## Datenverarbeitung

TBD