# Dateiverwaltung - Technische Dokumentation

## Inhaltsverzeichnis
1. [Projektübersicht](#projektübersicht)
2. [Hauptfunktionen](#hauptfunktionen)
3. [Klassen und Methoden](#klassen-und-methoden)
4. [Benutzeroberfläche](#benutzeroberfläche)
5. [Technische Details](#technische-details)

## Projektübersicht

Die Dateiverwaltung ist eine WPF-Anwendung (.NET 8.0) zur Verwaltung von Dateien und Ordnern. Sie bietet eine moderne Benutzeroberfläche mit drei Hauptbereichen:
- Verzeichnisbaum (links)
- Dateiliste (mitte)
- Detailansicht (rechts)

## Hauptfunktionen

### Dateioperationen
- Neue Datei erstellen
- Neuen Ordner erstellen
- Dateien/Ordner kopieren und einfügen
- Dateien/Ordner löschen
- Dateien/Ordner umbenennen
- Dateien öffnen

### Navigation
- Verzeichnisbaum mit Standardordnern
- Expandierbare Unterordner
- Doppelklick-Navigation
- Breadcrumb-Navigation

### Suche
- Rekursive Suche in allen Unterordnern
- Echtzeit-Vorschau
- Fehlertolerante Suche

## Klassen und Methoden

### MainWindow Klasse
Hauptfenster der Anwendung mit allen UI-Elementen und Logik.

#### Konstruktor und Initialisierung
```csharp
public MainWindow()
// Initialisiert die Anwendung

private void InitialisiereVerzeichnisbaum()
// Erstellt den Verzeichnisbaum mit Standardordnern

private TreeViewItem ErstelleVerzeichnisItem(string anzeigeName, string pfad, string symbol)
// Erstellt ein einzelnes Verzeichnis-Item für den Baum
```

#### Ereignisbehandlung
```csharp
private void Verzeichnis_Expanded(object sender, RoutedEventArgs e)
// Lädt Unterordner wenn ein Verzeichnis expandiert wird

private void Verzeichnisse_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
// Reagiert auf Auswahl im Verzeichnisbaum

private void Dateien_SelectionChanged(object sender, SelectionChangedEventArgs e)
// Aktualisiert die Vorschau bei Dateiauswahl

private void Dateien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
// Öffnet Dateien bei Doppelklick
```

#### Dateioperationen
```csharp
private void OrdnerLaden()
// Lädt alle Dateien und Ordner des aktuellen Verzeichnisses

private void ÖffneElement(DateiInfo datei)
// Öffnet eine Datei oder wechselt in einen Ordner

private void KopiereOrdner(string quellPfad, string zielPfad)
// Kopiert einen Ordner rekursiv mit allen Unterordnern
```

#### Suchfunktionen
```csharp
private void Suchen_Click(object sender, RoutedEventArgs e)
// Startet die Suche

private void SucheDateienRekursiv(string verzeichnis, string suchText)
// Durchsucht rekursiv alle Unterordner nach Dateien
```

#### Menü-Ereignisbehandlung
```csharp
private void MenuNeueDatai_Click(object sender, RoutedEventArgs e)
// Erstellt eine neue Datei

private void MenuNeuerOrdner_Click(object sender, RoutedEventArgs e)
// Erstellt einen neuen Ordner

private void MenuKopieren_Click(object sender, RoutedEventArgs e)
// Kopiert das ausgewählte Element

private void MenuEinfügen_Click(object sender, RoutedEventArgs e)
// Fügt das kopierte Element ein

private void MenuLöschen_Click(object sender, RoutedEventArgs e)
// Löscht das ausgewählte Element

private void MenuUmbenennen_Click(object sender, RoutedEventArgs e)
// Benennt das ausgewählte Element um

private void MenuAktualisieren_Click(object sender, RoutedEventArgs e)
// Aktualisiert die Ansicht

private void MenuÜber_Click(object sender, RoutedEventArgs e)
// Zeigt Informationen über die Anwendung
```

### DateiInfo Klasse
Repräsentiert eine Datei oder einen Ordner im System.

#### Eigenschaften
```csharp
public string Name { get; set; }
// Name der Datei/des Ordners

public bool IstOrdner { get; set; }
// Gibt an, ob es sich um einen Ordner handelt

public long Größe { get; set; }
// Größe in Bytes

public DateTime Änderungsdatum { get; set; }
// Letztes Änderungsdatum

public string Dateityp { get; set; }
// Dateierweiterung oder "Ordner"

public string Symbol
// Emoji-Symbol (📁 für Ordner, 📄 für Dateien)

public string GrößeAnzeige
// Formatierte Größe (B, KB, MB, GB)

public string ÄnderungsdatumAnzeige
// Formatiertes Datum
```

#### Methoden
```csharp
public static DateiInfo ErzeugeInfo(FileSystemInfo info)
// Fabrikmethode zum Erstellen von DateiInfo-Objekten
```

## Benutzeroberfläche

### XAML-Struktur
- DockPanel als Hauptcontainer
- Grid mit drei Spalten für das Layout
- TreeView für den Verzeichnisbaum
- ListView für die Dateiliste
- GroupBox für die Detailansicht

### Styles
- Definiert in App.xaml
- Einheitliches Design für Buttons, Listen und Menüs
- Emoji-Icons für bessere Visualisierung

## Technische Details

### Projekteinstellungen
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
  </PropertyGroup>
</Project>
```

### Abhängigkeiten
- Microsoft.VisualBasic für Eingabedialoge
- System.IO für Dateisystemoperationen
- WPF-Framework für die Benutzeroberfläche

### Fehlerbehandlung
- Try-Catch-Blöcke für alle Dateisystemoperationen
- Benutzerfreundliche Fehlermeldungen
- Logging von Zugriffsfehlern

### Leistungsoptimierungen
- Lazy Loading von Unterordnern
- Asynchrone Dateioperationen
- Effiziente Suche mit frühem Abbruch

---

## Verwendung im Code

### Beispiel: Datei erstellen
```csharp
private void MenuNeueDatai_Click(object sender, RoutedEventArgs e)
{
    var name = Microsoft.VisualBasic.Interaction.InputBox(
        "Name der neuen Datei:",
        "Neue Datei",
        "Neue Datei.txt");

    if (!string.IsNullOrWhiteSpace(name))
    {
        try
        {
            File.WriteAllText(Path.Combine(aktuellerOrdner, name), "");
            OrdnerLaden();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Fehler: {ex.Message}", "Fehler", 
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
```

### Beispiel: Rekursive Suche
```csharp
private void SucheDateienRekursiv(string verzeichnis, string suchText)
{
    try
    {
        foreach (var dateiPfad in Directory.GetFiles(verzeichnis))
        {
            var datei = new FileInfo(dateiPfad);
            if (datei.Name.ToLower().Contains(suchText))
            {
                dateiListe.Add(DateiInfo.ErzeugeInfo(datei));
            }
        }

        foreach (var ordnerPfad in Directory.GetDirectories(verzeichnis))
        {
            var ordner = new DirectoryInfo(ordnerPfad);
            if (ordner.Name.ToLower().Contains(suchText))
            {
                dateiListe.Add(DateiInfo.ErzeugeInfo(ordner));
            }
            SucheDateienRekursiv(ordnerPfad, suchText);
        }
    }
    catch (UnauthorizedAccessException)
    {
        // Ignoriere unzugängliche Ordner
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine(
            $"Fehler beim Durchsuchen von {verzeichnis}: {ex.Message}");
    }
}