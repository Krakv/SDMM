using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Xml;

namespace SDMMOperations
{
    public class WordFile
    {
        Body? body;
        Dictionary<string, string> headings = new Dictionary<string, string>();
        public readonly Dictionary<string, string> sections = new Dictionary<string, string>();

        public WordFile(string path = "templates/Empty.docx", string version_id = "") {
            Body body;
            if (version_id == "")
            {
                IEnumerable<Style> stylesElements;
                using (WordprocessingDocument myDocument = WordprocessingDocument.Open(path, true))
                {
                    body = myDocument.MainDocumentPart.Document.Body;
                    stylesElements = myDocument.MainDocumentPart.StyleDefinitionsPart.Styles.Elements<Style>();
                }

                ReadHeadings(stylesElements);
            }
            else
            {
                body = ReadBody(version_id);

                IEnumerable<Style> stylesElements;
                using (WordprocessingDocument myDocument = WordprocessingDocument.Open(path, true))
                {
                    stylesElements = myDocument.MainDocumentPart.StyleDefinitionsPart.Styles.Elements<Style>();
                }

                ReadHeadings(stylesElements);
            }

            this.body = body;
        }


        public List<ParagraphEntity> ReadText()
        {
            List<ParagraphEntity> result = new List<ParagraphEntity>();
            if (body != null)
            {
                ParagraphEntity par;

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        if (IsHeading(paragraph, out int level))
                        {
                            par = new ParagraphEntity("heading " + level, paragraph, paragraph.ParagraphId);
                            result.Add(par);
                        }
                        else if (!paragraph.Elements<Drawing>().Any())
                        {
                            par = new ParagraphEntity("p", paragraph, paragraph.ParagraphId);
                            result.Add(par);
                        }
                    }
                    else if (element is Table table)
                    {
                        par = new ParagraphEntity("tbl", element, null);
                        result.Add(par);
                    }

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

        private bool IsHeading(Paragraph paragraph, out int level)
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

        public void SaveNewDocument(string name, string type_id, string author, string status, string size, List<string> tags)
        {
            string project_id = SQLQuery.FindProject(name);

            if (project_id == "")
                project_id = SQLQuery.AddProject(name);

            string document_id = SQLQuery.AddDocument(project_id, type_id, author, status, size);
            string version_id = SQLQuery.AddVersion(document_id, "1.0");

            foreach(var tag in tags)
            {
                string tag_id = SQLQuery.FindTag(tag);
                if (tag_id == "")
                    tag_id = SQLQuery.AddTag(tag);

                SQLQuery.ConnectDocumentNTag(document_id, tag_id);
            }

            SaveSectionsBD(version_id);
        }

        public void SaveNewFile(string path)
        {
            if (body != null)
            {
                using (WordprocessingDocument wordDocument =
                    WordprocessingDocument.Create(path, WordprocessingDocumentType.Document))
                {
                    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                    mainPart.Document = new Document();
                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }
            }
        }
        
        public static void LoadVersionInFile(string source, string destination, Body body)
        {
            try
            {
                File.Copy(source, destination, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            if (body != null)
            {
                using (WordprocessingDocument wordDocument =
                    WordprocessingDocument.Open(destination, true))
                {
                    MainDocumentPart? mainPart = wordDocument.MainDocumentPart;
                    mainPart.Document = new Document();
                    mainPart.Document.Append(body);
                    mainPart.Document.Save();
                }
            }
        }

        public void SaveSectionsBD(string version_id)
        {
            string text = "";
            string header = "";
            string headerLevel = "a";

            foreach (var element in body.Elements())
            {
                bool isHeading = false;
                if (element is Paragraph paragraph)
                {
                    isHeading = IsHeading(paragraph, out int level);
                    if (isHeading)
                    {
                        string section_id = SQLQuery.AddSection(header, $"<w:pStyle w:val=\"{headerLevel}\"/>", text);
                        SQLQuery.ConnectVersionNSection(version_id, section_id);

                        header = paragraph.OuterXml;
                        headerLevel = level.ToString();
                    }
                }
                if (!isHeading)
                    text += element.OuterXml;
                else
                    text = "";
            }

            {
                string section_id = SQLQuery.AddSection(header, $"<w:pStyle w:val=\"{headerLevel}\"/>", text);
                SQLQuery.ConnectVersionNSection(version_id, section_id);
            }
        }

        public Body ReadBody(string version_id)
        {
            Body newBody = new Body();
            var sections = SQLQuery.ReadSections(version_id);

            XmlDocument doc;

            bool isFirst = false;
            foreach (var section in sections)
            {
                if (section["name"] != "")
                {
                    newBody.InnerXml += section["name"];
                    Paragraph par = newBody.LastChild as Paragraph;
                    if (par == null)
                    {
                        par = new Paragraph();
                    }
                    if (par.ParagraphId == null)
                    {
                        par.ParagraphId = GenerateRsid();
                    }
                    this.sections.Add(par.ParagraphId.ToString(), section["id"]);
                }
                else
                {
                    isFirst = true;
                }

                doc = new XmlDocument();
                doc.LoadXml("<root>" + section["text"] + "</root>");


                foreach(XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    newBody.InnerXml += node.OuterXml;

                    if (isFirst)
                    {
                        Paragraph par = newBody.LastChild as Paragraph;
                        if (par.ParagraphId == null)
                        {
                            par.ParagraphId = GenerateRsid();

                        }
                        this.sections.Add(par.ParagraphId.ToString(), section["id"]);
                        isFirst = false;
                    }
                }
            }

            return newBody;
        }

        static string GenerateRsid()
        {
            Random rnd = new Random();
            return rnd.Next(0x1000000, 0xFFFFFFF).ToString("X8");
        }

        public static Body ReadBodyStatic(string version_id)
        {
            Body newBody = new Body();
            var sections = SQLQuery.ReadSections(version_id);

            XmlDocument doc;

            foreach (var section in sections)
            {
                if (section["name"] != "")
                {
                    newBody.InnerXml += section["name"];
                    Paragraph par = newBody.LastChild as Paragraph;
                }


                doc = new XmlDocument();
                doc.LoadXml("<root>" + section["text"] + "</root>");

                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    newBody.InnerXml += node.OuterXml;
                }
            }

            return newBody;
        }

        public List<DataBaseEntities.Section> GetSections()
        {
            List<DataBaseEntities.Section> sections = new List<DataBaseEntities.Section>();
            string text = "";
            string header = "";
            string headerLevel = "a";

            foreach (var element in body.Elements())
            {
                bool isHeading = false;
                if (element is Paragraph paragraph)
                {
                    isHeading = IsHeading(paragraph, out int level);
                    if (isHeading)
                    {
                        sections.Add(new DataBaseEntities.Section(null, header, $"<w:pStyle w:val=\"{headerLevel}\"/>", text));
                        header = paragraph.OuterXml;
                        headerLevel = level.ToString();
                    }
                }
                if (!isHeading)
                    text += element.OuterXml;
                else
                    text = "";
            }

            {
                sections.Add(new DataBaseEntities.Section(null, header, $"<w:pStyle w:val=\"{headerLevel}\"/>", text));
            }

            return sections;
        }

    }
}
