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
        ObservableCollection<Datei> dateien = new();
        string aktOrdner;
        string zwischenablage;

        public MainWindow()
        {
            InitializeComponent();
            aktOrdner = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            Dateien.ItemsSource = dateien;
            
            OrdnerBaumErstellen();
            DateienLaden();

            Dateien.SelectionChanged += DateiAusgewählt;
        }

        void OrdnerBaumErstellen()
        {
            var benutzerOrdner = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            var desktop = OrdnerEintragErstellen("Desktop", Path.Combine(benutzerOrdner, "Desktop"), "🖥️");
            var dokumente = OrdnerEintragErstellen("Dokumente", Path.Combine(benutzerOrdner, "Documents"), "📄");
            var downloads = OrdnerEintragErstellen("Downloads", Path.Combine(benutzerOrdner, "Downloads"), "⭳");
            var bilder = OrdnerEintragErstellen("Bilder", Path.Combine(benutzerOrdner, "Pictures"), "🖼️");
            var musik = OrdnerEintragErstellen("Musik", Path.Combine(benutzerOrdner, "Music"), "🎵");
            var videos = OrdnerEintragErstellen("Videos", Path.Combine(benutzerOrdner, "Videos"), "🎬");

            var laufwerke = new TreeViewItem { Header = "Laufwerke 💽" };
            foreach (var lw in DriveInfo.GetDrives())
            {
                if (lw.IsReady)
                {
                    var name = string.IsNullOrEmpty(lw.VolumeLabel) ? lw.Name : $"{lw.Name} ({lw.VolumeLabel})";
                    laufwerke.Items.Add(OrdnerEintragErstellen(name, lw.Name, "💿"));
                }
            }

            Verzeichnisse.Items.Clear();
            Verzeichnisse.Items.Add(desktop);
            Verzeichnisse.Items.Add(dokumente);
            Verzeichnisse.Items.Add(downloads);
            Verzeichnisse.Items.Add(bilder);
            Verzeichnisse.Items.Add(musik);
            Verzeichnisse.Items.Add(videos);
            Verzeichnisse.Items.Add(laufwerke);

            Verzeichnisse.SelectedItemChanged += OrdnerAusgewählt;
        }

        TreeViewItem OrdnerEintragErstellen(string name, string pfad, string symbol)
        {
            var eintrag = new TreeViewItem
            {
                Header = $"{symbol} {name}",
                Tag = pfad
            };

            try
            {
                if (Directory.Exists(pfad))
                {
                    eintrag.Items.Add("...");
                    eintrag.Expanded += UnterordnerLaden;
                }
            }
            catch { }

            return eintrag;
        }

        void UnterordnerLaden(object sender, RoutedEventArgs e)
        {
            var ordner = sender as TreeViewItem;
            if (ordner?.Tag == null) return;

            if (ordner.Items.Count == 1 && ordner.Items[0] is string)
            {
                ordner.Items.Clear();
                try
                {
                    var pfad = ordner.Tag.ToString();
                    foreach (var unterordner in Directory.GetDirectories(pfad))
                    {
                        try
                        {
                            var info = new DirectoryInfo(unterordner);
                            ordner.Items.Add(OrdnerEintragErstellen(info.Name, info.FullName, "📁"));
                        }
                        catch { }
                    }
                }
                catch { }
            }
        }

        void OrdnerAusgewählt(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var ordner = Verzeichnisse.SelectedItem as TreeViewItem;
            if (ordner?.Tag != null)
            {
                var pfad = ordner.Tag.ToString();
                if (Directory.Exists(pfad))
                {
                    aktOrdner = pfad;
                    DateienLaden();
                }
            }
        }

        void DateiAusgewählt(object sender, SelectionChangedEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                DetailsAnzeigen(datei);
            }
        }

        void DetailsAnzeigen(Datei datei)
        {
            try
            {
                var pfad = Path.Combine(aktOrdner, datei.Name);
                var info = datei.IstOrdner ? 
                    (FileSystemInfo)new DirectoryInfo(pfad) : 
                    new FileInfo(pfad);

                DateiName.Text = info.Name;
                Pfad.Text = info.FullName;
                Größe.Text = datei.GrößeText;
                Erstellt.Text = info.CreationTime.ToString("dd.MM.yyyy HH:mm:ss");
                Geändert.Text = info.LastWriteTime.ToString("dd.MM.yyyy HH:mm:ss");
                Typ.Text = datei.IstOrdner ? "Ordner" : datei.DateiTyp ?? "Datei";
            }
            catch (Exception ex)
            {
                FehlerAnzeigen("Details konnten nicht geladen werden", ex);
            }
        }

        void DateienLaden()
        {
            try
            {
                dateien.Clear();

                foreach (var ordnerPfad in Directory.GetDirectories(aktOrdner))
                {
                    var info = new DirectoryInfo(ordnerPfad);
                    dateien.Add(Datei.OrdnerErstellen(info));
                }

                foreach (var dateiPfad in Directory.GetFiles(aktOrdner))
                {
                    var info = new FileInfo(dateiPfad);
                    dateien.Add(Datei.DateiErstellen(info));
                }
            }
            catch (Exception ex)
            {
                FehlerAnzeigen("Ordner konnte nicht geladen werden", ex);
            }
        }

        void MenuNeueDatai_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Wie soll die Datei heißen?",
                "Neue Datei",
                "Neue Datei.txt");

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    File.WriteAllText(Path.Combine(aktOrdner, name), "");
                    DateienLaden();
                }
                catch (Exception ex)
                {
                    FehlerAnzeigen("Datei konnte nicht erstellt werden", ex);
                }
            }
        }

        void MenuNeuerOrdner_Click(object sender, RoutedEventArgs e)
        {
            var name = Microsoft.VisualBasic.Interaction.InputBox(
                "Wie soll der Ordner heißen?",
                "Neuer Ordner",
                "Neuer Ordner");

            if (!string.IsNullOrWhiteSpace(name))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(aktOrdner, name));
                    DateienLaden();
                }
                catch (Exception ex)
                {
                    FehlerAnzeigen("Ordner konnte nicht erstellt werden", ex);
                }
            }
        }

        void MenuÖffnen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                DateiÖffnen(datei);
            }
        }

        void MenuBeenden_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void MenuKopieren_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                zwischenablage = Path.Combine(aktOrdner, datei.Name);
            }
        }

        void MenuEinfügen_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(zwischenablage)) return;

            try
            {
                var name = Path.GetFileName(zwischenablage);
                var ziel = Path.Combine(aktOrdner, "Kopie von " + name);

                if (File.Exists(zwischenablage))
                    File.Copy(zwischenablage, ziel);
                else if (Directory.Exists(zwischenablage))
                    OrdnerKopieren(zwischenablage, ziel);

                DateienLaden();
            }
            catch (Exception ex)
            {
                FehlerAnzeigen("Einfügen fehlgeschlagen", ex);
            }
        }

        void MenuLöschen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                var antwort = MessageBox.Show(
                    $"Soll '{datei.Name}' wirklich gelöscht werden?",
                    "Löschen",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (antwort == MessageBoxResult.Yes)
                {
                    try
                    {
                        var pfad = Path.Combine(aktOrdner, datei.Name);
                        if (File.Exists(pfad))
                            File.Delete(pfad);
                        else if (Directory.Exists(pfad))
                            Directory.Delete(pfad, true);

                        DateienLaden();
                    }
                    catch (Exception ex)
                    {
                        FehlerAnzeigen("Löschen fehlgeschlagen", ex);
                    }
                }
            }
        }

        void MenuUmbenennen_Click(object sender, RoutedEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                var neuerName = Microsoft.VisualBasic.Interaction.InputBox(
                    "Neuer Name:",
                    "Umbenennen",
                    datei.Name);

                if (!string.IsNullOrWhiteSpace(neuerName) && neuerName != datei.Name)
                {
                    try
                    {
                        var alterPfad = Path.Combine(aktOrdner, datei.Name);
                        var neuerPfad = Path.Combine(aktOrdner, neuerName);

                        if (File.Exists(alterPfad))
                            File.Move(alterPfad, neuerPfad);
                        else if (Directory.Exists(alterPfad))
                            Directory.Move(alterPfad, neuerPfad);

                        DateienLaden();
                    }
                    catch (Exception ex)
                    {
                        FehlerAnzeigen("Umbenennen fehlgeschlagen", ex);
                    }
                }
            }
        }

        void MenuAktualisieren_Click(object sender, RoutedEventArgs e)
        {
            DateienLaden();
        }

        void MenuÜber_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Dateiverwaltung v1.0\n\n" +
                "Ein einfacher Dateimanager zum Verwalten von Dateien und Ordnern.",
                "Über",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        void Suchen_Click(object sender, RoutedEventArgs e)
        {
            var suchtext = Suche.Text.ToLower();
            if (string.IsNullOrWhiteSpace(suchtext))
            {
                DateienLaden();
                return;
            }

            try
            {
                dateien.Clear();
                DateienSuchen(aktOrdner, suchtext);
            }
            catch (Exception ex)
            {
                FehlerAnzeigen("Suche fehlgeschlagen", ex);
            }
        }

        void DateienSuchen(string ordner, string suchtext)
        {
            try
            {
                foreach (var dateiPfad in Directory.GetFiles(ordner))
                {
                    var datei = new FileInfo(dateiPfad);
                    if (datei.Name.ToLower().Contains(suchtext))
                    {
                        dateien.Add(Datei.DateiErstellen(datei));
                    }
                }

                foreach (var ordnerPfad in Directory.GetDirectories(ordner))
                {
                    var unterordner = new DirectoryInfo(ordnerPfad);
                    if (unterordner.Name.ToLower().Contains(suchtext))
                    {
                        dateien.Add(Datei.OrdnerErstellen(unterordner));
                    }
                    DateienSuchen(ordnerPfad, suchtext);
                }
            }
            catch { }
        }

        void Dateien_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Dateien.SelectedItem is Datei datei)
            {
                DateiÖffnen(datei);
            }
        }

        void DateiÖffnen(Datei datei)
        {
            var pfad = Path.Combine(aktOrdner, datei.Name);
            if (datei.IstOrdner)
            {
                aktOrdner = pfad;
                DateienLaden();
            }
            else
            {
                try
                {
                    var start = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = pfad,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(start);
                }
                catch (Exception ex)
                {
                    FehlerAnzeigen("Datei konnte nicht geöffnet werden", ex);
                }
            }
        }

        void OrdnerKopieren(string quelle, string ziel)
        {
            Directory.CreateDirectory(ziel);
            foreach (var datei in Directory.GetFiles(quelle))
            {
                var name = Path.GetFileName(datei);
                File.Copy(datei, Path.Combine(ziel, name));
            }

            foreach (var ordner in Directory.GetDirectories(quelle))
            {
                var name = Path.GetFileName(ordner);
                OrdnerKopieren(ordner, Path.Combine(ziel, name));
            }
        }

        void FehlerAnzeigen(string nachricht, Exception ex)
        {
            MessageBox.Show(
                $"{nachricht}\n\nDetails: {ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    class Datei
    {
        public string Name { get; set; }
        public bool IstOrdner { get; set; }
        public long Größe { get; set; }
        public DateTime Geändert { get; set; }
        public string DateiTyp { get; set; }
        public string Symbol => IstOrdner ? "📁" : "📄";

        public string GrößeText
        {
            get
            {
                if (IstOrdner) return "-";
                if (Größe < 1024) return $"{Größe} B";
                if (Größe < 1024 * 1024) return $"{Größe / 1024:N0} KB";
                if (Größe < 1024 * 1024 * 1024) return $"{Größe / (1024 * 1024):N1} MB";
                return $"{Größe / (1024.0 * 1024 * 1024):N1} GB";
            }
        }

        public static Datei DateiErstellen(FileInfo info)
        {
            return new Datei
            {
                Name = info.Name,
                IstOrdner = false,
                Größe = info.Length,
                Geändert = info.LastWriteTime,
                DateiTyp = info.Extension
            };
        }

        public static Datei OrdnerErstellen(DirectoryInfo info)
        {
            return new Datei
            {
                Name = info.Name,
                IstOrdner = true,
                Größe = 0,
                Geändert = info.LastWriteTime,
                DateiTyp = null
            };
        }
    }
}