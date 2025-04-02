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
        // Variablen für die Dateiverwaltung
        ObservableCollection<Datei> dateien = new();
        string aktOrdner;
        string zwischenablage;

        public MainWindow()
        {
            InitializeComponent();
            
            // Standardmäßig Desktop anzeigen
            aktOrdner = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            
            // ListView mit Dateien verbinden
            Dateien.ItemsSource = dateien;
            
            // Ordnerbaum und Dateien laden
            OrdnerBaumErstellen();
            DateienLaden();

            // Wenn sich die Auswahl ändert, Details anzeigen
            Dateien.SelectionChanged += DateiAusgewählt;
        }

        void OrdnerBaumErstellen()
        {
            // Benutzerordner holen
            var benutzerOrdner = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            
            // Wichtige Ordner erstellen
            var desktop = OrdnerEintragErstellen("Desktop", Path.Combine(benutzerOrdner, "Desktop"), "🖥️");
            var dokumente = OrdnerEintragErstellen("Dokumente", Path.Combine(benutzerOrdner, "Documents"), "📄");
            var downloads = OrdnerEintragErstellen("Downloads", Path.Combine(benutzerOrdner, "Downloads"), "⭳");
            var bilder = OrdnerEintragErstellen("Bilder", Path.Combine(benutzerOrdner, "Pictures"), "🖼️");
            var musik = OrdnerEintragErstellen("Musik", Path.Combine(benutzerOrdner, "Music"), "🎵");
            var videos = OrdnerEintragErstellen("Videos", Path.Combine(benutzerOrdner, "Videos"), "🎬");

            // Laufwerke hinzufügen
            var laufwerke = new TreeViewItem { Header = "Laufwerke 💽" };
            foreach (var lw in DriveInfo.GetDrives())
            {
                if (lw.IsReady)
                {
                    var name = string.IsNullOrEmpty(lw.VolumeLabel) ? lw.Name : $"{lw.Name} ({lw.VolumeLabel})";
                    laufwerke.Items.Add(OrdnerEintragErstellen(name, lw.Name, "💿"));
                }
            }

            // Alles zum Baum hinzufügen
            Verzeichnisse.Items.Clear();
            Verzeichnisse.Items.Add(desktop);
            Verzeichnisse.Items.Add(dokumente);
            Verzeichnisse.Items.Add(downloads);
            Verzeichnisse.Items.Add(bilder);
            Verzeichnisse.Items.Add(musik);
            Verzeichnisse.Items.Add(videos);
            Verzeichnisse.Items.Add(laufwerke);

            // Wenn ein Ordner ausgewählt wird
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
            catch { } // Fehler ignorieren

            return eintrag;
        }

        void UnterordnerLaden(object sender, RoutedEventArgs e)
        {
            var ordner = sender as TreeViewItem;
            if (ordner?.Tag == null) return;

            // Nur laden wenn noch nicht geladen
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
                        catch { } // Unzugängliche Ordner überspringen
                    }
                }
                catch { } // Fehler ignorieren
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

                // Erst Ordner
                foreach (var ordnerPfad in Directory.GetDirectories(aktOrdner))
                {
                    var info = new DirectoryInfo(ordnerPfad);
                    dateien.Add(Datei.OrdnerErstellen(info));
                }

                // Dann Dateien
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

        // [Rest des Codes folgt dem gleichen Muster...]

        void FehlerAnzeigen(string nachricht, Exception ex)
        {
            MessageBox.Show(
                $"{nachricht}\n\nDetails: {ex.Message}",
                "Fehler",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    // Vereinfachte Datei-Klasse
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