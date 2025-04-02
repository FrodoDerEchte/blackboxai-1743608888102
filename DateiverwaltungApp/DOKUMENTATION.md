# Dateiverwaltung

## Was macht die Anwendung?
Die Dateiverwaltung ist ein einfacher Datei-Explorer. Links sehen Sie Ihre wichtigsten Ordner, in der Mitte die Dateien und Ordner, und rechts Details zur ausgewählten Datei.

## Wie benutze ich die Anwendung?

### Grundlegende Navigation
- Im linken Bereich auf einen Ordner klicken, um dessen Inhalt zu sehen
- Ordner mit dem Plus-Symbol (+) können aufgeklappt werden
- Doppelklick auf einen Ordner in der Mitte öffnet diesen
- Doppelklick auf eine Datei öffnet diese

### Dateien verwalten
1. Neue Datei erstellen:
   - "Datei" → "Neue Datei" klicken
   - Namen eingeben
   - Enter drücken

2. Neuen Ordner erstellen:
   - "Datei" → "Neuer Ordner" klicken
   - Namen eingeben
   - Enter drücken

3. Dateien/Ordner kopieren:
   - Datei/Ordner auswählen
   - "Bearbeiten" → "Kopieren" klicken
   - Zum Zielordner navigieren
   - "Bearbeiten" → "Einfügen" klicken

4. Dateien/Ordner löschen:
   - Datei/Ordner auswählen
   - "Bearbeiten" → "Löschen" klicken
   - Löschen bestätigen

5. Umbenennen:
   - Datei/Ordner auswählen
   - "Bearbeiten" → "Umbenennen" klicken
   - Neuen Namen eingeben
   - Enter drücken

### Suchen
1. Suchfeld oben links verwenden
2. Suchbegriff eingeben
3. Auf "Suchen" klicken oder Enter drücken
4. Die Suche findet Dateien und Ordner in allen Unterordnern

## Technische Details für Entwickler

### Wichtige Methoden

#### Verzeichnisbaum
```csharp
// Erstellt den Verzeichnisbaum links
InitialisiereVerzeichnisbaum()

// Erstellt einen Ordner-Eintrag im Baum
ErstelleVerzeichnisItem(string anzeigeName, string pfad, string symbol)

// Lädt Unterordner beim Aufklappen
Verzeichnis_Expanded(object sender, RoutedEventArgs e)
```

#### Dateioperationen
```csharp
// Lädt Dateien und Ordner
OrdnerLaden()

// Öffnet Dateien oder wechselt Ordner
ÖffneElement(DateiInfo datei)

// Kopiert Ordner mit Unterordnern
KopiereOrdner(string quellPfad, string zielPfad)
```

#### Suche
```csharp
// Startet die Suche
Suchen_Click(object sender, RoutedEventArgs e)

// Durchsucht Ordner rekursiv
SucheDateienRekursiv(string verzeichnis, string suchText)
```

### DateiInfo-Klasse
Speichert Informationen zu Dateien und Ordnern:
- Name
- Ist es ein Ordner?
- Größe
- Änderungsdatum
- Dateityp
- Symbol (📁 oder 📄)

### Fehlerbehandlung
- Alle Dateioperationen sind in try-catch-Blöcke eingepackt
- Benutzer sieht verständliche Fehlermeldungen
- Unzugängliche Ordner werden übersprungen

### Projektstruktur
- MainWindow.xaml: Benutzeroberfläche
- MainWindow.xaml.cs: Programmlogik
- App.xaml: Styles und Design
- DateiInfo.cs: Datei/Ordner-Informationen

## Tipps und Tricks
- Doppelklick öffnet Dateien und Ordner
- Enter-Taste funktioniert wie Doppelklick
- Rechtsklick zeigt zusätzliche Optionen
- Suchfeld durchsucht auch Unterordner
- Vorschau rechts zeigt Details zur ausgewählten Datei

## Bekannte Probleme
- Sehr große Ordner brauchen beim ersten Öffnen etwas Zeit
- Manche Systemordner können nicht durchsucht werden
- Dateien, die gerade benutzt werden, können nicht gelöscht werden

## Verbesserungsvorschläge
- Mehrere Dateien gleichzeitig auswählen
- Drag & Drop zum Kopieren
- Dateivorschau für Bilder und Texte
- Sortierung nach verschiedenen Kriterien
- Filterung nach Dateitypen