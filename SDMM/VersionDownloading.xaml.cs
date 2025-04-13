using SDMMOperations;
using System.Windows;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для VersionDownloading.xaml
    /// </summary>
    public partial class VersionDownloading : Window
    {
        string version_id;
        string name;

        public VersionDownloading(string version_id, string name)
        {
            InitializeComponent();
            this.version_id = version_id;
            this.name = name;
        }

        private void saveInDocumentCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            chooseDocumentToSaveInGrid.Visibility = Visibility.Collapsed;
        }

        private void saveInDocumentCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            chooseDocumentToSaveInGrid.Visibility = Visibility.Visible;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (saveInDocumentCheckBox.IsChecked.Value)
            {
                try
                {
                    WordFile.LoadVersionInFile(documentPath.Text, path.Text + '/' + name + ".docx", WordFile.ReadBodyStatic(version_id));
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Ошибка доступа: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    WordFile.LoadVersionInFile("templates/Empty.docx", path.Text + '/' + name + ".docx", WordFile.ReadBodyStatic(version_id));
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show($"Ошибка доступа: {ex.Message}");
                }
            }
            this.Close();
        }

        private void openDocumentPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".docx";
            dialog.Filter = "Text documents (.docx)|*.docx";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                documentPath.Text = dialog.FileName;
            }
        }

        private void openPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                path.Text = dialog.FolderName;
            }
        }
    }
}
