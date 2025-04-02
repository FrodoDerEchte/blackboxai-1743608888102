# Dateiverwaltung - Benutzer- und Entwicklerhandbuch

## Inhaltsverzeichnis
1. [Überblick](#überblick)
2. [Hauptfunktionen](#hauptfunktionen)
3. [Technische Implementation](#technische-implementation)
4. [Detaillierte Bedienungsanleitung](#detaillierte-bedienungsanleitung)
5. [Tastenkombinationen](#tastenkombinationen)
6. [Fehlerbehebung](#fehlerbehebung)

## Überblick

Die Dateiverwaltung ist eine benutzerfreundliche Anwendung zur Verwaltung von Dateien und Ordnern. Sie bietet eine übersichtliche Oberfläche mit drei Hauptbereichen:
- Verzeichnisbaum (links)
- Dateiliste (mitte)
- Detailansicht (rechts)

## Hauptfunktionen und deren Implementation

### Datei-Menü
- **Neue Datei** (📄)
  - UI-Event: `MenuNeueDatai_Click`
  - Erstellt eine neue leere Datei mit `File.WriteAllText()`

- **Neuer Ordner** (📁)
  - UI-Event: `MenuNeuerOrdner_Click`
  - Erstellt einen neuen Ordner mit `Directory.CreateDirectory()`

- **Öffnen** (📂)
  - UI-Event: `MenuÖffnen_Click`
  - Implementiert in `ÖffneElement(DateiInfo)`

- **Zuletzt geöffnet** (🕒)
  - Verwaltet in der `ObservableCollection<string> dateiListe`

- **Beenden** (🚪)
  - UI-Event: `MenuBeenden_Click`

### Bearbeiten-Menü
- **Kopieren** (📋)
  - UI-Event: `MenuKopieren_Click`
  - Speichert Pfad in `kopierterDateiPfad`

- **Einfügen** (📌)
  - UI-Event: `MenuEinfügen_Click`
  - Nutzt `File.Copy()` oder `KopiereOrdner()` für Verzeichnisse

- **Löschen** (🗑️)
  - UI-Event: `MenuLöschen_Click`
  - Verwendet `File.Delete()` oder `Directory.Delete()`

- **Umbenennen** (✏️)
  - UI-Event: `MenuUmbenennen_Click`
  - Nutzt `File.Move()` oder `Directory.Move()`

### Ansicht-Menü
- **Aktualisieren** (🔄)
  - UI-Event: `MenuAktualisieren_Click`
  - Ruft `OrdnerLaden()` auf

### Hilfe-Menü
- **Über** (ℹ️)
  - UI-Event: `MenuÜber_Click`

## Technische Implementation

### Hauptklassen

#### MainWindow
- **Eigenschaften**:
  - `ObservableCollection<DateiInfo> dateiListe`: Liste aller Dateien und Ordner
  - `string aktuellerOrdner`: Aktuelles Verzeichnis
  - `string kopierterDateiPfad`: Pfad der kopierten Datei

- **Hauptmethoden**:
  ```csharp
  private void OrdnerLaden()
  // Lädt alle Dateien und Ordner aus dem aktuellen Verzeichnis

  private void ÖffneElement(DateiInfo datei)
  // Öffnet eine Datei oder wechselt in einen Ordner

  private void KopiereOrdner(string quellPfad, string zielPfad)
  // Kopiert einen Ordner rekursiv mit allen Unterordnern
  ```

#### DateiInfo Klasse
```csharp
public class DateiInfo
{
    public string Name { get; set; }
    public bool IstOrdner { get; set; }
    public long Größe { get; set; }
    public DateTime Änderungsdatum { get; set; }
    public string Dateityp { get; set; }
    public string Symbol => IstOrdner ? "📁" : "📄";

    public string GrößeAnzeige { get; }
    public string ÄnderungsdatumAnzeige { get; }

    public static DateiInfo ErzeugeInfo(FileSystemInfo info)
    // Fabrikmethode zum Erstellen von DateiInfo-Objekten
}
```

### Event-Handler

#### Dateioperationen
```csharp
private void MenuNeueDatai_Click(object sender, RoutedEventArgs e)
private void MenuNeuerOrdner_Click(object sender, RoutedEventArgs e)
private void MenuÖffnen_Click(object sender, RoutedEventArgs e)
private void MenuBeenden_Click(object sender, RoutedEventArgs e)
```

#### Bearbeitungsoperationen
```csharp
private void MenuKopieren_Click(object sender, RoutedEventArgs e)
private void MenuEinfügen_Click(object sender, RoutedEventArgs e)
private void MenuLöschen_Click(object sender, RoutedEventArgs e)
private void MenuUmbenennen_Click(object sender, RoutedEventArgs e)
```

#### Sonstige Events
```csharp
private void Suchen_Click(object sender, RoutedEventArgs e)
private void Dateien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
private void MenuAktualisieren_Click(object sender, RoutedEventArgs e)
private void MenuÜber_Click(object sender, RoutedEventArgs e)
```

## Detaillierte Bedienungsanleitung

### Neue Datei erstellen
1. Klicken Sie auf "Datei" → "Neue Datei" (`MenuNeueDatai_Click`)
2. Geben Sie einen Namen für die neue Datei ein
3. Bestätigen Sie mit Enter

### Neuen Ordner erstellen
1. Klicken Sie auf "Datei" → "Neuer Ordner" (`MenuNeuerOrdner_Click`)
2. Geben Sie einen Namen für den neuen Ordner ein
3. Bestätigen Sie mit Enter

### Dateien/Ordner kopieren und einfügen
1. Wählen Sie die Datei oder den Ordner aus
2. Klicken Sie auf "Bearbeiten" → "Kopieren" (`MenuKopieren_Click`)
3. Navigieren Sie zum Zielverzeichnis
4. Klicken Sie auf "Bearbeiten" → "Einfügen" (`MenuEinfügen_Click`)

### Dateien/Ordner löschen
1. Wählen Sie die Datei oder den Ordner aus
2. Klicken Sie auf "Bearbeiten" → "Löschen" (`MenuLöschen_Click`)
3. Bestätigen Sie den Löschvorgang

### Dateien/Ordner umbenennen
1. Wählen Sie die Datei oder den Ordner aus
2. Klicken Sie auf "Bearbeiten" → "Umbenennen" (`MenuUmbenennen_Click`)
3. Geben Sie den neuen Namen ein
4. Bestätigen Sie mit Enter

### Dateien suchen
1. Geben Sie den Suchbegriff in das Suchfeld ein
2. Klicken Sie auf "Suchen" (`Suchen_Click`)
3. Die Ergebnisse werden in der Dateiliste angezeigt

## Tastenkombinationen

- **Enter**: Öffnet die ausgewählte Datei/Ordner (`Dateien_MouseDoubleClick`)
- **Alt+F4**: Beendet die Anwendung (`MenuBeenden_Click`)
- **Doppelklick**: Öffnet eine Datei oder wechselt in einen Ordner (`Dateien_MouseDoubleClick`)

## Fehlerbehebung

### Häufige Fehlermeldungen

1. "Datei existiert bereits"
   - Implementiert in: Try-Catch Blöcke in allen Dateioperationen
   - Lösung: Wählen Sie einen anderen Namen

2. "Zugriff verweigert"
   - Implementiert in: Try-Catch Blöcke in `File` und `Directory` Operationen
   - Lösung: Überprüfen Sie die Berechtigungen

3. "Datei wird von einem anderen Prozess verwendet"
   - Implementiert in: Try-Catch Blöcke in `File.Delete()` und `File.Move()`
   - Lösung: Schließen Sie alle Programme, die die Datei verwenden

### Allgemeine Tipps

- Nutzen Sie `MenuAktualisieren_Click` für manuelle Aktualisierungen
- Die Suchfunktion verwendet `Directory.GetFileSystemEntries()` mit Filter
- Alle Dateioperationen sind in Try-Catch-Blöcken für Fehlerbehandlung
- Die `DateiInfo`-Klasse bietet formatierte Eigenschaften für die Anzeige

---

Bei weiteren Fragen oder Problemen klicken Sie auf "Hilfe" → "Über" (`MenuÜber_Click`) für Versionsinformationen und Support-Möglichkeiten.