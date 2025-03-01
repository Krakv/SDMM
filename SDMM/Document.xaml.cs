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

            //Body parts = wordFile.Read();

            //var paragraphs = parts.Elements<Paragraph>().Select(p => p.InnerText);

            List<string[]> paragraphs = wordFile.ReadText();

            FlowDocument doc = new FlowDocument();
            

            foreach(string[] item in paragraphs)
            {
                string id = item[0];
                string paragraph = item[1];

                System.Windows.Documents.Paragraph newParagraph = new System.Windows.Documents.Paragraph() { };

                if (id.StartsWith("heading"))
                {
                    newParagraph.FontWeight = FontWeights.Bold;
                    newParagraph.FontSize = 16;
                    newParagraph.TextAlignment = System.Windows.TextAlignment.Center;
                }

                newParagraph.Inlines.Add(new System.Windows.Documents.Run(paragraph));
                doc.Blocks.Add(newParagraph);
            }

            MainDoc.Document = doc;
        }

    }
}
