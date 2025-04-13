using System.Windows;
using System.Windows.Controls;
using SDMMOperations;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для AddDocument.xaml
    /// </summary>
    public partial class AddDocument : Window
    {
        private WordFile wordFile = null;

        public WordFile WordFile { get { return wordFile; } }

        public bool shouldSave = true;

        public AddDocument(DataBaseEntities.Document? doc = null)
        {
            InitializeComponent();
            Fill_Fields();
            if (doc != null)
            {
                shouldSave = false;
                LoadDocumentInfo(doc);
            }
        }

        private void LoadDocumentInfo(DataBaseEntities.Document doc)
        {
            projectNameTextBox.Text = doc.project.name;
            foreach(DataBaseEntities.Standart standart in standartComboBox.Items)
            {
                if (standart.id == doc.documentType.standartID)
                {
                    standartComboBox.SelectedItem = standart;
                    standartComboBox.IsEnabled = false;
                    break;
                }
            }
            foreach (DataBaseEntities.DocumentType type in documentTypeComboBox.Items)
            {
                if (type.id == doc.documentType.id)
                {
                    documentTypeComboBox.SelectedItem = type;
                    documentTypeComboBox.IsEnabled = false;
                    break;
                }
            }
            authorTextBox.Text = doc.author;
            statusComboBox.Text = "Новый";
            templateCheckBox.IsChecked = true;
        }

        private async void LoadProjectNames()
        {
            var projects = await SQLQuery.ReadProjects();

            foreach(var project in projects)
                projectNameTextBox.Items.Add(project["name"]);
        }

        public async void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isValidName = MainWindow.IsValidInput(projectNameTextBox.Text);
            bool isValidStatus = MainWindow.IsValidInput(statusComboBox.Text);
            bool isValidAuthor = MainWindow.IsValidInput(authorTextBox.Text);
            bool isValidStandart = MainWindow.IsValidInput(standartComboBox.Text);
            bool isValidType = MainWindow.IsValidInput(documentTypeComboBox.Text);
            if (isValidName && isValidStatus && isValidAuthor && isValidStandart && isValidType)
            {
                DialogResult = true;
                if (shouldSave)
                {
                    try
                    {
                        DataBaseEntities.DocumentType? type = documentTypeComboBox.SelectedValue as DataBaseEntities.DocumentType;

                        if (type != null)
                        {
                            if (templateCheckBox.IsChecked.Value)
                            {
                                var templates = await SQLQuery.GetTemplate(type.templateID);
                                path.Text = "templates/" + templates[0]["template_ref"];
                            }
                            long length = new System.IO.FileInfo(path.Text).Length;
                            wordFile = new WordFile(path.Text);
                            wordFile.SaveNewDocument(projectNameTextBox.Text, type.id, authorTextBox.Text, statusComboBox.Text, length.ToString(), [.. tagsTextBox.Text.Split()]);
                            this.Close();
                        }
                    
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось загрузить файл.");
                    }
                }

            }
            else
            {
                MessageBox.Show("Неверный ввод");
            }
        }

        private void openPathButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.FileName = "Document";
            dialog.DefaultExt = ".docx";
            dialog.Filter = "Text documents (.docx)|*.docx";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                path.Text = dialog.FileName;
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Fill_Fields()
        {
            LoadProjectNames();
            LoadStandarts();
        }

        private async void LoadStandarts()
        {
            var standarts = await SQLQuery.ReadStandarts();

            foreach (var standart in standarts)
                standartComboBox.Items.Add(new DataBaseEntities.Standart(standart["id"], standart["name"]));
        }

        private async void StandartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? cb = sender as ComboBox;
            
            if (cb != null)
            {
                DataBaseEntities.Standart? standart = cb.SelectedValue as DataBaseEntities.Standart;
                documentTypeComboBox.IsEnabled = false;
                documentTypeComboBox.Items.Clear();

                if (standart != null)
                {
                    var documentTypes = await SQLQuery.ReadDocumentTypes(standart.id);
                    foreach (var documentType in documentTypes)
                        documentTypeComboBox.Items.Add(new DataBaseEntities.DocumentType(documentType["id"], documentType["name"], documentType["standart_id"], documentType["template_id"], documentType["description"]));

                    documentTypeComboBox.IsEnabled = true;
                }
            }
        }
        private async void templateCheckBox_checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (cb.IsChecked != null)
            {
                path.IsEnabled = false;
                openPathButton.IsEnabled = false;
                DataBaseEntities.DocumentType? type = documentTypeComboBox.SelectedValue as DataBaseEntities.DocumentType;
                if (type != null)
                {
                    var templates = await SQLQuery.GetTemplate(type.templateID);
                    path.Text = "templates/" + templates[0]["template_ref"];
                }
                else
                {
                    path.Text = "templates/Empty.exe";
                }
            }
        }


        private void templateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            path.IsEnabled = true;
            openPathButton.IsEnabled = true;
        }
    }
}
