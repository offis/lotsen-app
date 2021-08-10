# Changelog

## v2.0.6

### Visuell

  * In Auswahllisten wird immer der Anzeigewert angezeigt und nicht mehr der interne Wert, wenn die Auswahlliste eigene Werte zulässt
  * Auswahllisten lassen immer eigene Werte zu
  * Dropdown Menü als Hilfe zur Benutzung der Teilnehmersuche
  * Metainformationen (Name, Farbe, Icon) können bearbeitet werden

### Inhaltlich

  * Suchfunktion für Auswahllisten. Es können immer eigene Werte angegeben werden und die Auswahllisten können durchsucht werden. Suchkriterien sind alle Anzeige- und internen Werte.
  * Datenfelder können für Teilnehmer zum Header hinzugefügt werden, sodass nach diesen Werten gesucht werden kann.
  * Datenfelder können für Projekte zum Header hinzugefügt werden, sodass für alle Teilnehmer des Projekts nach diesem Wert gesucht werden kann.
  * Tastatursteuerung im Editor verbessert.

### Technisch

  * Update auf Toast UI Editor 3.0.1
  * Update auf ElectronNet 13.5.1
  * Auswahllisten nutzen Virtual Scrolling. Dadurch können wesentlich mehr Elemente der Auswahlliste hinzugefügt werden. Nach einem Test sollten ohne große Performanzprobleme 16000 Elemente in der Auswahlliste dargestellt werden können.
  * Änderung der Updatesite zu Github Releases als Vorbereitung für die Open Source Stellung

### Bugfixes

  * Seiteneffekte von einigen Unit-Tests entfernt, sodass diese verlässlich laufen
  * Der automatisierte Build-Prozess funktioniert nach dem Update von Electron

## v2.0.5

### Visuell

  * Die Anwendung kann durch den Benutzer konfiguriert werden
    * Die Sprache, Darstellung, Lokalisation und Updates lassen sich konfigurieren
  * Die LotsenApp nutzt an wenigen Stellen Push-Benachrichtigungen
  * Alle verfügbaren Datenformate werden auf der Benutzerseite angezeigt.

### Inhaltlich

  * Die Darstellung der Datumsangaben wird auch im Editor angewandt
  * Erinnerungen besitzen eigene Push-Benachrichtigungen
  * Ein Benutzer kann über die LotsenApp Datenformate und Lokalisationen hochladen

### Technisch

  * Update auf Angular 12
  * Flächendeckende Unit-Tests für Teilnehmer, Projekte und Anwendungsfunktionen, sowie Sonstiges. Es fehlen noch Tests für die Konfiguration, Tools, Authentifizierung und Autorisierung sowie Integrationstests. Insgesamt beträgt die Testabdeckung der Anwendungslogik 55%.

### Bugfixes

  * Die Tastaturnavigation des Menüs funktioniert wieder korrekt.
  * Diverse Bugfixes nach Unit-Tests.
  * Erinnerungen werden wieder im System gespeichert.

## v2.0.4

### Visuell

  * Im Editor können über das Datenfeld-Kontextmenü verschiedene Darstellungswerte ausgewählt werden, wenn sie im Datenformat konfiguriert sind.
  * In der oberen Leiste des Fenster wird neben dem Minimieren-Knopf ein Knopf zum Update der LotsenApp angezeigt, falls ein Update vorhanden ist
  * In "Über die LotsenApp" gibt es einen Abschnitt, über den manuell nach Updates gesucht werden kann.
  * Unter "Über die LotsenApp" gibt es eine Seite, in der diese Änderungshistorie angezeigt wird. Die Historie ist nicht lokalisiert.

### Inhaltlich

  * Konfigurierbares Dashboard für die Benutzerübersicht hinzugefügt
    * Die maximale Größe beträgt 100x100 Kacheln
    * Drag & Drop zum Verschieben der Kacheln im Gitter
    * Unten und Rechts an der Kachel kann ihre Größe im Gitter angepasst werden
    * Hinzufügen und Entfernen von Kacheln (Entfernen über Kontext-Menü)
  * Programmkachel für das Dashboard hinzugefügt
    * Die Kachel zeigt eine konfigurierbare Liste von Programmen an.
    * Diese Kachel dient als "Favoritenleiste" für andere Programme
    * Die Liste kann per Drag & Drop umsortiert werden
  * Erinnerungen haben neben einem Startzeitpunkt auch einen Endzeitpunkt erhalten

### Technisch

  * Überarbeitung des Update-Mechanismus
    * Es wird nicht mehr beim Start des Updates nach Updates
    * Es werden keine Updates automatisch heruntergeladen
    * Updates werden automatisch nach Beenden der App installiert, wenn sie heruntergeladen wurden oder Updates können installiert und die App neu gestartet werden.
  * Austausch des Frontend Lintings
    * TSLint (deprecated) wurde durch ESLint ausgetauscht
  * Verschieben der Datei-Ordner
    * Anstatt die Dateien relativ zum Ausführungsverzeichnis zu speichern werden diese im UserData-Verzeichnis (`%appdata%/lotsen-app` unter Windows) gespeichert.
    * Es wird nicht migriert. Die Anwendung muss erneut aufgesetzt werden.
  * Das Starten der Anwendung wurde beschleunigt
    * Verschiedene Hintergrundprozesse werden asynchron und nicht mehr synchron gestartet
    * Reduktion der Initial-Bundle Größe um 20%
  * Automatisierte Tests zur Prüfung der Korrektheit der Operationen auf den Studien
    * Alle Operationen auf den Studiendaten werden durch automatisierte Tests (Unit-Tests) überprüft, sodass die Korrektheit der Operationen zwischen Updates gewährleistet wird.

### Bugfixes

  * Das Dashboard passt sich dem Theme korrekt an
  * In der englischen Lokalisation wird der Dokumentenname im Dokument-Löschen Dialog korrekt interpoliert.

## v2.0.3

### Visuell

  * Überarbeitung der Anzeige von Dashboard-Kacheln
  * Überarbeitung der Anzeige von Erinnerungen
    * Es gibt 4 verschiedene Anzeigemöglichkeiten
      * Alle Erinnerungen im Monat
      * Alle Erinnerungen in der Woche
      * Alle Erinnerungen an einem Tag
      * Alle Erinnerungen in einer Liste

### Inhaltlich

  * Konfigurierbares Dashboard für die Benutzerübersicht hinzugefügt
    * Die maximale Größe beträgt 100x100 Kacheln
    * Drag & Drop zum Verschieben der Kacheln im Gitter
    * Unten und Rechts an der Kachel kann ihre Größe im Gitter angepasst werden
    * Hinzufügen und Entfernen von Kacheln (Entfernen über Kontext-Menü)
  * Programmkachel für das Dashboard hinzugefügt
    * Die Kachel zeigt eine konfigurierbare Liste von Programmen an.
    * Diese Kachel dient als "Favoritenleiste" für andere Programme
    * Die Liste kann per Drag & Drop umsortiert werden
  * Erinnerungen haben neben einem Startzeitpunkt auch einen Endzeitpunkt erhalten

### Technisch

  * Aktualisierung der Abhängigkeiten auf die letzte kompatible Version
  * Erinnerungen können im iCalendar-Format (`.ics`) angefragt werden, sodass diese zukünftig auch in andere Anwendung exportiert werden können.

### Bugfixes

  * Die Zeitstempel der Erinnerungen werden korrekt als UTC-Zeit gespeichert

## v2.0.2

### Visuell

  * Das Icon der LotsenApp wurde gegen ein temporäres Icon ausgetauscht
  * Das Umbenennen von Dokumenten spiegelt sich sofort im Tab und in der Dokumentenübersicht wieder
  * Bei den Tabs wird, wenn vorhanden, neben dem Dokumentennamen auch der Name des Elterndokuments angezeigt
  * Umbenennen des Patienten-Menüpunkts zu Studien
  * Umbenennen der Patienten zu Teilnehmer

### Inhaltlich

  * Neuer Menüpunkt: Über die LotsenApp
    * Lizenzinformationen zur LotsenApp hinzugefügt
    * Impressumstemplate hinzugefügt
    * Datenschutztemplate hinzugefügt
    * Drittanbieterlizenzen hinzugefügt
  * Patientendatenspeicherung aktiviert

### Technisch

  * Austausch von marked gegen ngx-markdown
  * Hot-Reload für Lokalisationen hinzugefügt
  * Aufheben der Beschränkungen der Drag & Drop Funktionalität in der Dokumentenübersicht. Dokumente können jetzt beliebig angeordnet werden.
  * Der Parser des Datenformats für das Frontend ist robuster gegenüber Fehlern und spiegelt Fehler an das Frontend zurück

### Bugfixes

  * Integer und Double Editoren speichern die eingegeben Werte vollständig und nicht nur das erste Zeichen
  * Drag & Drop Funktionalität bei Gruppen aktualisiert die Felder korrekt anstatt Werte zu überschreiben
  * Probleme mit der Auto-Update Funktion behoben. Ab diesem Release sollte die Auto-Update Funktion greifen.

## v2.0.1

### Visuell

  * Die Versionsnummer in der Fensterleiste wird als Super-Script dargestellt

### Inhaltlich

  * Update auf Datenformat `mk3`

### Technisch

  * Update auf .NET 5
  * Update auf Electron 11
  * Update auf Angular 11
  * Dateispeicherung für Konfiguration
  * Dateispeicherung für Datenformate
  * Austausch der Semaphorenalgorithmen durch Lese-/Schreibschlösser

### Bugfixes

  * Probleme bei der nebenläufigen Bearbeitung in der Patientendatenspeicherung sind behoben.
  * Probleme beim ersten Login sind behoben
  * Ein Fehler in der Initialisierung des Ringschlüsselverfahrens wurde behoben. 

## v2.0.0

  * Initiales Release der LotsenApp in Alpha-Testphase
  * Datenformat `mk2`
  * Erfassung von Patientendaten
  * Erinnerungsfunktion
  * Kryptographisches Konzept
  * Authentifizierung
  * Autorisierung
  * Konfiguration
  * Benutzerkonfiguration