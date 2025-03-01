using System;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
namespace SDMMOperations
{
    public class WordFile
    {
        Body body;
        Dictionary<string, string> headings = new Dictionary<string, string>();

        public WordFile(string path) {

            IEnumerable<Style> stylesElements;
            using (WordprocessingDocument myDocument = WordprocessingDocument.Open(path, true))
            {
                body = myDocument.MainDocumentPart.Document.Body;
                stylesElements = myDocument.MainDocumentPart.StyleDefinitionsPart.Styles.Elements<Style>();
            }

            ReadHeadings(stylesElements);
        }

        public Body Read()
        {
            return body;
        }

        public List<string[]> ReadText()
        {
            List<string[]> result = new List<string[]>();

            foreach (var element in body.Elements())
            {
                if (element is Paragraph paragraph)
                {
                    if (IsHeading(paragraph, out int level))
                    {
                        result.Add(["heading " + level, $"\nЗАГОЛОВОК {level}: {paragraph.InnerText.ToUpper()}\n"]);
                    }
                    else if (!paragraph.Elements<Drawing>().Any())
                    {
                        result.Add(["p", paragraph.InnerText]);
                    }
                }
                else if (element is Table table)
                {
                    result.Add(["tbl", "\n[Таблица]\n" + ReadTable(table)]);
                }
            }

            return result;
        }

        public void ReadHeadings(IEnumerable<Style> stylesElements)
        {
            foreach (var style in stylesElements)
            {
                string styleId = style.StyleId;
                string styleName = style.StyleName?.Val;

                if (styleName.StartsWith("heading"))
                    headings.Add(styleId, styleName);
            }
        }

        static string ReadTable(Table table)
        {
            string tableText = "";
            foreach (var row in table.Elements<TableRow>())
            {
                foreach (var cell in row.Elements<TableCell>())
                {
                    tableText += cell.InnerText + "\t"; 
                }
                tableText += "\n"; 
            }
            return tableText;
        }

        bool IsHeading(Paragraph paragraph, out int level)
        {
            level = 0;
            var pPr = paragraph.Elements<ParagraphProperties>().FirstOrDefault();
            var style = pPr?.Elements<ParagraphStyleId>().FirstOrDefault();

            if (style != null && headings.Keys.Contains(style.Val.Value))
            {
                level = int.Parse(headings[style.Val.Value].Replace("heading", ""));
                return true;
            }
            return false;
        }
    }
}
