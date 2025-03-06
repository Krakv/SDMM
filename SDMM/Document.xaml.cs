using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using SDMMOperations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для Document.xaml
    /// </summary>
    public partial class Document : Window
    {
        string fileName;
        public Document(string fileName)
        {
            InitializeComponent();
            this.fileName = fileName;
            LoadDocument(fileName);
        }

        public void LoadDocument(string fileName)
        {
            WordFile wordFile = new WordFile(fileName);

            //Body parts = wordFile.body;
            //var paragraphs = parts.Elements<Paragraph>().Select(p => p.InnerText);

            List<string[]> paragraphs = wordFile.ReadText();

            FlowDocument doc = new FlowDocument();
            

            System.Windows.Documents.Section newSection = null;
            foreach(string[] item in paragraphs)
            {
                string name = item[0];
                string paragraph = item[1];
                string id = item[2];

                System.Windows.Documents.Paragraph newParagraph = new System.Windows.Documents.Paragraph() { };

                if (name.StartsWith("heading"))
                {
                    if (newSection != null)
                        doc.Blocks.Add(newSection);

                    newSection = new Section() { Tag = id };

                    var listBoxItem = new ListBoxItem() { Tag = id, Content = paragraph };
                    listBoxItem.MouseDoubleClick += on_Double_Click_Section;
                    headings.Items.Add(listBoxItem);

                    newParagraph.FontWeight = FontWeights.Bold;
                    newParagraph.FontSize = 16;
                    newParagraph.TextAlignment = System.Windows.TextAlignment.Center;
                }

                newParagraph.Inlines.Add(new System.Windows.Documents.Run(paragraph));
                if (newSection != null)
                {
                    newSection.Blocks.Add(newParagraph);
                }
                else
                {
                    doc.Blocks.Add(newParagraph);
                }
            }
            if (newSection != null)
                doc.Blocks.Add(newSection);

            MainDoc.Document = doc;
        }

        public void LoadSections()
        {

        }

        private void on_Double_Click_Section(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;

            var targetSection = MainDoc.Document.Blocks
                .OfType<Section>()
                .FirstOrDefault(s => s.Tag == item.Tag);

            if (targetSection != null)
            {
                TextPointer sectionStart = targetSection.ContentStart;

                if (sectionStart != null)
                {
                    MainDoc.CaretPosition = sectionStart;

                    MainDoc.Focus();

                    Rect rect = MainDoc.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
                    MainDoc.ScrollToVerticalOffset(rect.Top + MainDoc.VerticalOffset - 20);
                }
            }
            else
            {
                MessageBox.Show("Секция не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

    }
}
