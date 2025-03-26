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
    /// Логика взаимодействия для ManageDocumentsPack.xaml
    /// </summary>
    public partial class ManageDocumentsPack : Window
    {
        List<DataBaseEntities.Document> docs;

        public ManageDocumentsPack(List<DataBaseEntities.Document> documents)
        {
            InitializeComponent();
            docs = documents;
            LoadDocumentsList();
        }

        private void LoadDocumentsList()
        {
            foreach(var document in docs)
            {
                ListBoxItem item = new ListBoxItem() { Content = document };
                item.MouseDoubleClick += ChangeDocumentInfo;
                documentTypesList.Items.Add(item);
            }
        }

        private void ChangeDocumentInfo(object sender, MouseEventArgs e)
        {
            ListBoxItem? item = sender as ListBoxItem;
            if(item != null)
            {
                DataBaseEntities.Document? doc = item.Content as DataBaseEntities.Document;

                if (doc != null)
                {
                    AddDocument window = new AddDocument(doc);
                    window.ShowDialog();

                    if (window.DialogResult.Value)
                    {
                        doc.author = window.authorTextBox.Text;
                        doc.status = window.statusComboBox.Text;
                        doc.path = window.path.Text;
                        doc.tags = window.tagsTextBox.Text;
                    }
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var document in docs)
            {
                if (document.path == "") document.path = "templates/Empty.docx";
                long length = new System.IO.FileInfo(document.path).Length;
                WordFile wordFile = new WordFile(document.path);
                wordFile.SaveNewDocument(document.project.name, document.documentType.id, document.author, document.status, length.ToString(), [.. document.tags.Split()]);
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
