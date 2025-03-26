using DocumentFormat.OpenXml.Office2010.Word;
using DocumentFormat.OpenXml.Wordprocessing;
using SDMMOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для EditDocument.xaml
    /// </summary>
    public partial class EditDocument : Window
    {
        DataBaseEntities.Document document;

        public EditDocument(DataBaseEntities.Document document)
        {
            InitializeComponent();
            this.document = document;
            FillFields();
            standartComboBox.SelectionChanged += StandartComboBox_SelectionChanged;
        }

        private void FillFields()
        {
            projectNameTextBox.Text = document.project.name;
            authorTextBox.Text = document.author;
            statusComboBox.Text = document.status;
            string tags = GetTags(document.id);
            tagsTextBox.Text = tags;
            tagsTextBox.Tag = tags;
            FillStandart();
            FillDocumentType();

        }

        private void FillStandart()
        {
            var standarts = SQLQuery.ReadStandarts();
            foreach(var standart in standarts)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = new DataBaseEntities.Standart(standart["id"], standart["name"]) };
                standartComboBox.Items.Add(item);
                if (standart["id"] == document.documentType.standartID)
                    standartComboBox.SelectedItem = item;
            }
        }

        private void FillDocumentType()
        {
            var documentTypes = SQLQuery.ReadDocumentTypes(document.documentType.standartID);
            foreach (var documentType in documentTypes)
            {
                ComboBoxItem item = new ComboBoxItem() { Content = new DataBaseEntities.DocumentType(documentType["id"], documentType["name"], documentType["standart_id"], documentType["template_id"], documentType["description"]) };
                documentTypeComboBox.Items.Add(item);
                if (documentType["id"] == document.documentType.id)
                    documentTypeComboBox.SelectedItem = item;
            }

        }

        private static string GetTags(string document_id)
        {
            var tags = SQLQuery.ReadTags(document_id);

            var result = "";

            foreach (var tag in tags)
                result += tag["name"] + " ";

            return result;
        }

        private void StandartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? cb = sender as ComboBox;

            if (cb != null)
            {
                DataBaseEntities.Standart? standart = ((ComboBoxItem)cb.SelectedValue).Content as DataBaseEntities.Standart;
                documentTypeComboBox.IsEnabled = false;
                documentTypeComboBox.Items.Clear();

                if (standart != null)
                {
                    var documentTypes = SQLQuery.ReadDocumentTypes(standart.id);
                    foreach (var documentType in documentTypes)
                        documentTypeComboBox.Items.Add(new DataBaseEntities.DocumentType(documentType["id"], documentType["name"], documentType["standart_id"], documentType["template_id"], documentType["description"]));

                    documentTypeComboBox.IsEnabled = true;
                }
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isValidName = MainWindow.IsValidInput(projectNameTextBox.Text);
            bool isValidStatus = MainWindow.IsValidInput(statusComboBox.Text);
            bool isValidAuthor = MainWindow.IsValidInput(authorTextBox.Text);
            bool isValidStandart = MainWindow.IsValidInput(standartComboBox.Text);
            bool isValidType = MainWindow.IsValidInput(documentTypeComboBox.Text);
            if (isValidName && isValidStatus && isValidAuthor && isValidStandart && isValidType)
            {
                try
                {
                    string project_id = SQLQuery.FindProject(projectNameTextBox.Text);
                    if (project_id == "")
                        project_id = SQLQuery.AddProject(projectNameTextBox.Text);

                    string document_type_id = ((DataBaseEntities.DocumentType)((ComboBoxItem)documentTypeComboBox.SelectedItem).Content).id;

                    SQLQuery.UpdateDocument(document.id, project_id, document_type_id, authorTextBox.Text, statusComboBox.Text, document.size);


                    var tags = tagsTextBox.Text.Trim().Split();
                    string? textboxtag = tagsTextBox.Tag.ToString();
                    textboxtag = textboxtag == null ? "" : textboxtag;
                    var old_tags = textboxtag.Trim().Split();
                    var tags_to_add = tags.Except(old_tags);
                    var tags_to_remove = old_tags.Except(tags);

                    foreach (var tag in tags_to_add)
                    {
                        string tag_id = SQLQuery.FindTag(tag);
                        if (tag_id == "")
                            tag_id = SQLQuery.AddTag(tag);

                    
                        SQLQuery.ConnectDocumentNTag(document.id, tag_id);
                    }
                
                    foreach (var tag in tags_to_remove)
                    {
                        string tag_id = SQLQuery.FindTag(tag);
                        if (tag_id != "")
                            SQLQuery.DeleteDocumentNTagConnection(document.id, tag_id);
                    }

                    this.Close();
                }
                catch
                {
                    MessageBox.Show("Не удалось обновить.");
                }

            }
            else
            {
                MessageBox.Show("Неверный ввод");
            }

        }
    }
}
