using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace DateiverwaltungApp
{
    public partial class MainWindow : Window
    {
        private ObservableCollection<DateiInfo> dateiListe = new ObservableCollection<DateiInfo>();
        private string aktuellerOrdner;
        private string kopierterDateiPfad;

        public MainWindow()
        {
            InitializeComponent();
            aktuellerOrdner = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Dateien.ItemsSource = dateiListe;
            
            InitialisiereVerzeichnisbaum();
            OrdnerLaden();

            // Event-Handler für Dateiauswahl
            Dateien.SelectionChanged += Dateien_SelectionChanged;
        }

        private void InitialisiereVerzeichnisbaum()
        {
            // Benutzerverzeichnisse
            var userPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var desktop = Path.Combine(userPath, "Desktop");
            var documents = Path.Combine(userPath, "Documents");
            var downloads = Path.Combine(userPath, "Downloads");
            var pictures = Path.Combine(userPath, "Pictures");
            var music = Path.Combine(userPath, "Music");
            var videos = Path.Combine(userPath, "Videos");

            // TreeViewItems erstellen
            var desktopItem = ErstelleVerzeichnisItem("Desktop", desktop, "🖥️");
            var documentsItem = ErstelleVerzeichnisItem("Dokumente", documents, "📄");
            var downloadsItem = ErstelleVerzeichnisItem("Downloads", downloads, "⭳");
            var picturesItem = ErstelleVerzeichnisItem("Bilder", pictures, "🖼️");
            var musicItem = ErstelleVerzeichnisItem("Musik", music, "🎵");
            var videosItem = ErstelleVerzeichnisItem("Videos", videos, "🎬");

            // Laufwerke hinzufügen
            var laufwerkeItem = new TreeViewItem { Header = "Laufwerke 💽" };
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    var driveItem = ErstelleVerzeichnisItem(
                        $"{drive.Name} ({drive.VolumeLabel})",
                        drive.Name,
                        "💿");
                    laufwerkeItem.Items.Add(driveItem);
                }
            }

            // Zum TreeView hinzufügen
            Verzeichnisse.Items.Clear();
            Verzeichnisse.Items.Add(desktopItem);
            Verzeichnisse.Items.Add(documentsItem);
            Verzeichnisse.Items.Add(downloadsItem);
            Verzeichnisse.Items.Add(picturesItem);
            Verzeichnisse.Items.Add(musicItem);
            Verzeichnisse.Items.Add(videosItem);
            Verzeichnisse.Items.Add(laufwerkeItem);

            // Event-Handler für TreeView
            Verzeichnisse.SelectedItemChanged += Verzeichnisse_SelectedItemChanged;
        }

        private TreeViewItem ErstelleVerzeichnisItem(string anzeigeName, string pfad, string symbol)
        {
            var item = new TreeViewItem
            {
                Header = $"{symbol} {anzeigeName}",
                Tag = pfad
            };

            try
            {
                if (Directory.Exists(pfad))
                {
                    item.Items.Add("Lädt...");
                    item.Expanded += Verzeichnis_Expanded;
                }
            }
            catch { /* Ignoriere Zugriffsfehler */ }

            return item;
        }

        private void Verzeichnis_Expanded(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            if (item?.Tag == null) return;

            if (item.Items.Count == 1 && item.Items[0] is string)
            {
                item.Items.Clear();
                try
                {
                    var pfad = item.Tag.ToString();
                    foreach (var dir in Directory.GetDirectories(pfad))
                    {
                        try
                        {
                            var dirInfo = new DirectoryInfo(dir);
                            var unterItem = ErstelleVerzeichnisItem(
                                dirInfo.Name,
                                dirInfo.FullName,
                                "📁");
                            item.Items.Add(unterItem);
                        }
                        catch { /* Ignoriere unzugängliche Ordner */ }
                    }
                }
                catch { /* Ignoriere Zugriffsfehler */ }
            }
        }

        private void Verzeichnisse_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var item = Verzeichnisse.SelectedItem as TreeViewItem;
            if (item?.Tag != null)
            {
                var pfad = item.Tag.ToString();
                if (Directory.Exists(pfad))
                {
                    aktuellerOrdner = pfad;
                    OrdnerLaden();
                }
            }
        }

        private void Dateien_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                AktualisiereVorschau(datei);
            }
        }

        private void AktualisiereVorschau(DateiInfo datei)
        {
            try
            {
                var pfad = Path.Combine(aktuellerOrdner, datei.Name);
                var info = datei.IstOrdner ? 
                    (FileSystemInfo)new DirectoryInfo(pfad) : 
                    new FileInfo(pfad);

                // Aktualisiere die Detailansicht
                DateiName.Text = info.Name;
                Pfad.Text = info.FullName;
                Größe.Text = datei.GrößeAnzeige;
                Erstellt.Text = info.CreationTime.ToString("dd.MM.yyyy HH:mm:ss");
                Geändert.Text = info.LastWriteTime.ToString("dd.MM.yyyy HH:mm:ss");
                Typ.Text = datei.IstOrdner ? "Ordner" : 
                    (string.IsNullOrEmpty(datei.Dateityp) ? "Datei" : datei.Dateityp);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden der Vorschau: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrdnerLaden()
        {
            try
            {
                dateiListe.Clear();

                // Ordner laden
                foreach (var ordner in Directory.GetDirectories(aktuellerOrdner))
                {
                    var info = new DirectoryInfo(ordner);
                    dateiListe.Add(DateiInfo.ErzeugeInfo(info));
                }

                // Dateien laden
                foreach (var datei in Directory.GetFiles(aktuellerOrdner))
                {
                    var info = new FileInfo(datei);
                    dateiListe.Add(DateiInfo.ErzeugeInfo(info));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", 
                    "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Menü-Event-Handler
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

        private void MenuNeuerOrdner_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Name des neuen Ordners:",
                "Neuer Ordner",
                "Neuer Ordner");

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(aktuellerOrdner, name));
                    OrdnerLaden();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler: {ex.Message}", "Fehler", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuÖffnen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                ÖffneElement(datei);
            }
        }

        private void MenuBeenden_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuKopieren_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                kopierterDateiPfad = Path.Combine(aktuellerOrdner, datei.Name);
            }
        }

        private void MenuEinfügen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(kopierterDateiPfad)) return;

            try
            {
                var name = Path.GetFileName(kopierterDateiPfad);
                var zielPfad = Path.Combine(aktuellerOrdner, "Kopie von " + name);

                if (File.Exists(kopierterDateiPfad))
                    File.Copy(kopierterDateiPfad, zielPfad);
                else if (Directory.Exists(kopierterDateiPfad))
                    KopiereOrdner(kopierterDateiPfad, zielPfad);

                OrdnerLaden();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                var result = MessageBox.Show(
                    $"Möchten Sie '{datei.Name}' wirklich löschen?",
                    "Löschen bestätigen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        var pfad = Path.Combine(aktuellerOrdner, datei.Name);
                        if (File.Exists(pfad))
                            File.Delete(pfad);
                        else if (Directory.Exists(pfad))
                            Directory.Delete(pfad, true);

                        OrdnerLaden();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuUmbenennen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                var neuerName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Neuer Name:",
                    "Umbenennen",
                    datei.Name);

                if (!string.IsNullOrWhiteSpace(neuerName) && neuerName != datei.Name)
                {
                    try
                    {
                        var alterPfad = Path.Combine(aktuellerOrdner, datei.Name);
                        var neuerPfad = Path.Combine(aktuellerOrdner, neuerName);

                        if (File.Exists(alterPfad))
                            File.Move(alterPfad, neuerPfad);
                        else if (Directory.Exists(alterPfad))
                            Directory.Move(alterPfad, neuerPfad);

                        OrdnerLaden();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuAktualisieren_Click(object sender, RoutedEventArgs e)
        {
            OrdnerLaden();
        }

        private void MenuÜber_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Dateiverwaltung\nVersion 1.0\n\n" +
                "Eine einfache Anwendung zur Verwaltung von Dateien und Ordnern.",
                "Über",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Suchen_Click(object sender, RoutedEventArgs e)
        {
            var suchText = Suche.Text.ToLower();
            if (string.IsNullOrWhiteSpace(suchText))
            {
                OrdnerLaden();
                return;
            }

            try
            {
                dateiListe.Clear();
                foreach (var element in Directory.GetFileSystemEntries(aktuellerOrdner, "*", SearchOption.TopDirectoryOnly))
                {
                    var info = new FileInfo(element);
                    if (info.Name.ToLower().Contains(suchText))
                    {
                        dateiListe.Add(DateiInfo.ErzeugeInfo(info));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler bei der Suche: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Dateien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                ÖffneElement(datei);
            }
        }

        private void ÖffneElement(DateiInfo datei)
        {
            var pfad = Path.Combine(aktuellerOrdner, datei.Name);
            if (datei.IstOrdner)
            {
                aktuellerOrdner = pfad;
                OrdnerLaden();
            }
            else
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pfad,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Fehler beim Öffnen: {ex.Message}", "Fehler", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void KopiereOrdner(string quellPfad, string zielPfad)
        {
            Directory.CreateDirectory(zielPfad);
            foreach (string dateiPfad in Directory.GetFiles(quellPfad))
            {
                string zielDateiPfad = Path.Combine(zielPfad, Path.GetFileName(dateiPfad));
                File.Copy(dateiPfad, zielDateiPfad);
            }

            foreach (string ordnerPfad in Directory.GetDirectories(quellPfad))
            {
                string zielOrdnerPfad = Path.Combine(zielPfad, Path.GetFileName(ordnerPfad));
                KopiereOrdner(ordnerPfad, zielOrdnerPfad);
            }
        }
    }

    public class DateiInfo
    {
        public string Name { get; set; }
        public bool IstOrdner { get; set; }
        public long Größe { get; set; }
        public DateTime Änderungsdatum { get; set; }
        public string Dateityp { get; set; }
        public string Symbol => IstOrdner ? "📁" : "📄";

        public string GrößeAnzeige
        {
            get
            {
                if (IstOrdner) return "<Ordner>";
                if (Größe < 1024) return $"{Größe} B";
                if (Größe < 1024 * 1024) return $"{Größe / 1024:N1} KB";
                if (Größe < 1024 * 1024 * 1024) return $"{Größe / (1024 * 1024):N1} MB";
                return $"{Größe / (1024 * 1024 * 1024):N1} GB";
            }
        }

        public string ÄnderungsdatumAnzeige => Änderungsdatum.ToString("dd.MM.yyyy HH:mm");

        public static DateiInfo ErzeugeInfo(FileSystemInfo info)
        {
            if (info is FileInfo datei)
            {
                return new DateiInfo
                {
                    Name = datei.Name,
                    IstOrdner = false,
                    Größe = datei.Length,
                    Änderungsdatum = datei.LastWriteTime,
                    Dateityp = datei.Extension
                };
            }
            else
            {
                return new DateiInfo
                {
                    Name = info.Name,
                    IstOrdner = true,
                    Größe = 0,
                    Änderungsdatum = info.LastWriteTime,
                    Dateityp = "Ordner"
                };
            }
        }
    }
}