using Microsoft.Win32;
using SDMMOperations;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SDMM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            LoadDocuments(new List<string[]> {  new string[7] { "1", "Doc", "ТЗ", "1.0", "192kb", "26.02.2025", "#tag" }, 
                                                new string[7] { "2", "Doc1", "ТЗ", "1.1", "193kb", "26.02.2025", "#tag1 #tag2" },
                                                new string[7] { "2", "Doc1", "ТЗ", "1.1", "193kb", "26.02.2025", "#tag1 #tag2" },
                                                new string[7] { "2", "Doc1", "ТЗ", "1.1", "193kb", "26.02.2025", "#tag1 #tag2" },
                                                new string[7] { "2", "Doc1", "ТЗ", "1.1", "193kb", "26.02.2025", "#tag1 #tag2" } });
        }

        private void LoadDocuments(List<string[]> docs)
        {
            DocumentsList.Items.Clear();
            foreach (string[] doc in docs)
            {
                StackPanel panel = new StackPanel() { Width = 350, Margin = new Thickness(10, 5, 10, 5) };
                panel.Children.Add(new Separator() { Margin = new Thickness(0, 0, 0, 10) });

                StackPanel docName = new StackPanel() { Orientation = Orientation.Horizontal };
                docName.Children.Add(new TextBlock() { Text = "*", FontSize = 16, FontWeight = FontWeight.FromOpenTypeWeight(900), Margin = new Thickness(5, 5, 5, 5) });
                docName.Children.Add(new TextBlock() { Text = doc[1], FontSize = 16, FontWeight = FontWeight.FromOpenTypeWeight(900), Margin = new Thickness(5, 5, 5, 5) });

                panel.Children.Add(docName);
                panel.Children.Add(new TextBlock() { Text = doc[2], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = doc[3], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = doc[4], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = doc[5], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = doc[6], Style = (Style)this.Resources["Style1"] });

                panel.Children.Add(new Separator() { Margin = new Thickness(0, 10, 0, 0) });

                ListBoxItem item = new ListBoxItem() { Content = panel, Tag = doc[0] };
                item.MouseDoubleClick += Document_Click;
                
                DocumentsList.Items.Add(item);
            }
        }

        private void LoadVersions(List<string[]> vers)
        {
            VersionsList.Items.Clear();
            foreach (string[] ver in vers)
            {
                StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
                panel.Children.Add(new TextBlock() { Text = ver[0] });
                panel.Children.Add(new TextBlock() { Text = " | " });
                panel.Children.Add(new TextBlock() { Text = ver[1] });

                ListBoxItem item = new ListBoxItem() { Content = panel };
                VersionsList.Items.Add(item);
            }
        }

        private void LoadDocumentInfo(string[] doc)
        {
            DocumentInfo_DocName.Text = doc[1];
            DocumentInfo_DocType.Text = doc[2];
            DocumentInfo_DocVer.Text = doc[3];
            DocumentInfo_DocVal.Text = doc[4];
            DocumentInfo_DocDate.Text = doc[5];
            DocumentInfo_DocTags.Text = doc[6];
        }

        private void Document_Click(object sender, RoutedEventArgs e)
        {
            LoadDocumentInfo(["1", "Doc", "ТЗ", "1.0", "192kb", "26.02.2025", "#tag"]);
            LoadVersions(new List<string[]> {  new string[7] { "1", "2", "3", "4", "6", "7", "8" },
                                                new string[7] { "2", "2", "3", "5", "5", "7", "8" },});

        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".docx"; 
            dialog.Filter = "Text documents (.docx)|*.docx";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string filename = dialog.FileName;
            }


            SDMM.Document document = new SDMM.Document(dialog.FileName);

            document.Show();

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Сохранение файла...");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Это программа на WPF!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Поиск");
        }
    }
}