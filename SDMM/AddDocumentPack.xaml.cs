using SDMMOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для AddDocumentPack.xaml
    /// </summary>
    public partial class AddDocumentPack : Window
    {
        public AddDocumentPack()
        {
            InitializeComponent();
            documentTypesCheckBoxList.IsEnabled = false;
            LoadStandarts();
        }

        private async void LoadStandarts()
        {
            var standarts = await SQLQuery.ReadStandarts();

            foreach (var standart in standarts)
                standartComboBox.Items.Add(new DataBaseEntities.Standart(standart["id"], standart["name"]));

            standartComboBox.SelectedIndex = 0;
        }

        private async void LoadDocumentTypes(string standart_id)
        {
            var document_types = await SQLQuery.ReadDocumentTypes(standart_id);

            foreach(var document_type in document_types)
            {
                var document_type_entity = new DataBaseEntities.DocumentType(document_type["id"], document_type["name"], document_type["standart_id"], document_type["template_id"], document_type["description"]);
                var cb = new CheckBox() { Tag = document_type["id"], Content = document_type_entity, Margin = new Thickness(10), MinWidth=300 };

                documentTypesCheckBoxList.Children.Add(cb);
            }
        }

        private void StandartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? cb = sender as ComboBox;

            if (cb != null)
            {
                DataBaseEntities.Standart? standart = cb.SelectedValue as DataBaseEntities.Standart;
                documentTypesCheckBoxList.IsEnabled = false;
                documentTypesCheckBoxList.Children.Clear();

                if (standart != null)
                {
                    LoadDocumentTypes(standart.id);

                    documentTypesCheckBoxList.IsEnabled = true;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool isValidName = MainWindow.IsValidInput(projectNameTextBox.Text);
            bool isValidAuthor = MainWindow.IsValidInput(authorTextBox.Text);
            bool isValidStandart = MainWindow.IsValidInput(standartComboBox.Text);
            if (isValidName && isValidAuthor && isValidStandart)
            {
                ManageDocumentsPack window = new ManageDocumentsPack(LoadDocuments());
                window.ShowDialog();
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный ввод");
            }

        }

        private List<DataBaseEntities.Document> LoadDocuments()
        {
            List<DataBaseEntities.Document> docs = new List<DataBaseEntities.Document>();

            var templates = Task.Run(async () =>
            {
                return await SQLQuery.ReadTemplates();
            }).Result; 

            var templatesDic = new Dictionary<string, string>();
            foreach (var template in templates)
                templatesDic.Add(template["id"], template["template_ref"]);

            foreach (CheckBox item in documentTypesCheckBoxList.Children)
            {
                if (item.IsChecked.Value)
                {
                    DataBaseEntities.DocumentType? type = item.Content as DataBaseEntities.DocumentType;

                    if (type != null)
                    {
                        DataBaseEntities.Document doc = new DataBaseEntities.Document("-1", new DataBaseEntities.Project("-1", projectNameTextBox.Text), type, authorTextBox.Text, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Новый", "0", "templates/" + templatesDic[type.templateID]);
                        docs.Add(doc);
                    }
                }
            }

            return docs;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
