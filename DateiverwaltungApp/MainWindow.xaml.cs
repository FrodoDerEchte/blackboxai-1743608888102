using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

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
            OrdnerLaden();
        }

        // Datei-Menü Events
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
                    MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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

        // Bearbeiten-Menü Events
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
                MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        MessageBox.Show($"Fehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void MenuAktualisieren_Click(object sender, RoutedEventArgs e)
        {
            OrdnerLaden();
        }

        // Sonstige Events
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
                MessageBox.Show($"Fehler bei der Suche: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Dateien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Dateien.SelectedItem is DateiInfo datei)
            {
                ÖffneElement(datei);
            }
        }

        // Hilfsmethoden
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
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MessageBox.Show($"Fehler beim Öffnen: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
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