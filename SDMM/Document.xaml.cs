using SDMMOperations;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using DocumentFormat.OpenXml;
using System.Text;
using NPOI.XWPF.UserModel;
using System.Windows.Media;
using DocumentFormat.OpenXml.Office2016.Excel;
using MySql.Data.MySqlClient;
using System.Timers;

namespace SDMM
{
    /// <summary>
    /// Логика взаимодействия для Document.xaml
    /// </summary>
    public partial class Document : Window
    {
        WordFile wordFile;
        Transaction? trans = null;
        string version_id;

        public Document(WordFile wordFile, string version_id = "")
        {
            InitializeComponent();
            this.wordFile = wordFile;
            this.version_id = version_id;
            LoadDocument(MainDoc, wordFile);
        }

        public async void LoadDocument(RichTextBox rtf, WordFile wordFile, bool isMainDoc = true)
        {
            List<ParagraphEntity> paragraphs = wordFile.ReadText();

            FlowDocument doc = new FlowDocument();

            bool isFirst = true;

            foreach (var item in paragraphs)
            {
                string name = item.name;
                OpenXmlElement element = item.element;
                string id = item.id;

                if (isMainDoc && isFirst)
                {
                    headings.Items.Clear();
                    string version_section_id = await SQLQuery.FindVersionNSectionConnection(version_id, wordFile.sections[id]);
                    ContextMenu mn = GetContextMenu(version_section_id);
                    var listBoxItem = new ListBoxItem() { Tag = id, Content = "Без заголовка", ContextMenu = mn };
                    listBoxItem.MouseDoubleClick += on_Double_Click_Section;
                    headings.Items.Add(listBoxItem);
                    isFirst = false;
                }

                {
                    if (element is DocumentFormat.OpenXml.Wordprocessing.Paragraph para)
                    {

                        var paragraph = (DocumentFormat.OpenXml.Wordprocessing.Paragraph)element;

                        Paragraph wpfParagraph = new Paragraph() { Tag = id };

                        wpfParagraph.Margin = new Thickness(0, 5, 0, 5);

                        if (name.StartsWith("heading"))
                        {
                            wpfParagraph.TextAlignment = System.Windows.TextAlignment.Center;
                            wpfParagraph.FontWeight = FontWeights.Bold;
                        }

                        foreach (var text in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                        {
                            wpfParagraph.Inlines.Add(new Run(text.InnerText));
                        }

                        if (paragraph.ParagraphProperties != null && paragraph.ParagraphProperties.NumberingProperties != null)
                        {
                            List wpfList = new List();

                            System.Windows.Documents.ListItem listItem = new System.Windows.Documents.ListItem(new Paragraph(new Run(paragraph.InnerText)));
                            wpfList.ListItems.Add(listItem);

                            doc.Blocks.Add(wpfList);
                        }
                        else
                        {
                            doc.Blocks.Add(wpfParagraph);
                        }

                    }

                    if (element is DocumentFormat.OpenXml.Wordprocessing.Table tbl)
                    {
                        DocumentFormat.OpenXml.Wordprocessing.Table openXmlTable = (DocumentFormat.OpenXml.Wordprocessing.Table)element;

                        System.Windows.Documents.Table wpfTable = new System.Windows.Documents.Table();


                        TableRowGroup rowGroup = new TableRowGroup();
                        wpfTable.RowGroups.Add(rowGroup);

                        foreach (var row in openXmlTable.Elements<DocumentFormat.OpenXml.Wordprocessing.TableRow>())
                        {
                            TableRow wpfRow = new TableRow();

                            foreach (var cell in row.Elements<DocumentFormat.OpenXml.Wordprocessing.TableCell>())
                            {
                                TableCell wpfCell = new TableCell();
                                wpfCell.BorderBrush = Brushes.Black;
                                wpfCell.BorderThickness = new Thickness(1);

                                wpfCell.Padding = new Thickness(5);

                                foreach (var paragraph in cell.Elements<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
                                {
                                    Paragraph wpfParagraph = new Paragraph();

                                    wpfParagraph.Margin = new Thickness(0, 5, 0, 5);

                                    foreach (var text in paragraph.Elements<DocumentFormat.OpenXml.Wordprocessing.Run>())
                                    {
                                        wpfParagraph.Inlines.Add(new Run(text.InnerText));
                                    }

                                    if (paragraph.ParagraphProperties != null && paragraph.ParagraphProperties.NumberingProperties != null)
                                    {
                                        List wpfList = new List();

                                        System.Windows.Documents.ListItem listItem = new System.Windows.Documents.ListItem(new Paragraph(new Run(paragraph.InnerText)));
                                        wpfList.ListItems.Add(listItem);

                                        wpfCell.Blocks.Add(wpfList);
                                    }
                                    else
                                    {
                                        wpfCell.Blocks.Add(wpfParagraph);
                                    }
                                }

                                wpfRow.Cells.Add(wpfCell);
                            }

                            rowGroup.Rows.Add(wpfRow);
                        }

                        doc.Blocks.Add(wpfTable);
                    }

                    if (isMainDoc && name.StartsWith("heading"))
                    {
                        string version_section_id = await SQLQuery.FindVersionNSectionConnection(version_id, wordFile.sections[id]);
                        ContextMenu mn = GetContextMenu(version_section_id);
                        var listBoxItem = new ListBoxItem() { Tag = id, Content = element.InnerText, ContextMenu = mn };
                        listBoxItem.MouseDoubleClick += on_Double_Click_Section;
                        headings.Items.Add(listBoxItem);
                    }
                }
            }
            rtf.Document = doc;
        }

        public ContextMenu GetContextMenu(string version_section_id)
        {
            ContextMenu mn = new ContextMenu() ;
            MenuItem item;

            item = new MenuItem() { Tag = version_section_id };
            item.Click += AddSection;
            item.Header = "Добавить раздел";
            mn.Items.Add(item);

            item = new MenuItem() { Tag = version_section_id };
            item.Click += RemoveSection;
            item.Header = "Удалить раздел";
            mn.Items.Add(item);

            item = new MenuItem() { Tag = version_section_id };
            item.Click += EditSection;
            item.Header = "Редактировать раздел";
            mn.Items.Add(item);

            item = new MenuItem() { Tag = version_section_id };
            item.Click += CommentSection;
            item.Header = "Комментировать раздел";
            mn.Items.Add(item);

            return mn;
        }

        private async void AddSection(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null && version_id != "")
            {
                string? version_section_id = item.Tag.ToString();
                if (version_section_id != null && version_section_id != "")
                {
                    var window = new InputText("");
                    window.TextBlock1.Text = "Название раздела";
                    window.ComboBox.Visibility = Visibility.Visible;
                    window.TextBlock2.Text = "Уровень заголовка";
                    window.ComboBox.Items.Add("1");
                    window.ComboBox.Items.Add("2");
                    window.ComboBox.Items.Add("3");
                    window.ComboBox.SelectedIndex = 0;
                    window.ShowDialog();
                    if (window.DialogResult.Value)
                    {
                        this.Cursor = Cursors.Wait;
                        var sections = await SQLQuery.ReadSections(version_id);

                        string name = $"<w:p xmlns:w14=\"http://schemas.microsoft.com/office/word/2010/wordml\" xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:pPr><w:pStyle w:val=\"{window.ComboBox.Text}\" /></w:pPr><w:r></w:r><w:r ><w:t>{window.TextBox.Text}</w:t></w:r></w:p>";

                        var section_id = await SQLQuery.AddSection(name, $"<w:pStyle w:val=\"{window.ComboBox.Text}\" />", "");
                        var versions_sections = await SQLQuery.GetVersionNSectionConnection(version_section_id);
                        foreach (var section in sections)
                        {
                            await SQLQuery.DeleteVersionNSectionConnection(version_id, section["id"]);
                            await SQLQuery.ConnectVersionNSection(version_id, section["id"]);
                            if (versions_sections[0]["section_id"] == section["id"])
                            {
                                await SQLQuery.ConnectVersionNSection(version_id, section_id);
                            }
                        }

                        wordFile = new WordFile(version_id: version_id);
                        LoadDocument(MainDoc, wordFile);
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
        }
        
        private async void RemoveSection(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null && version_id != "")
            {
                string? version_section_id = item.Tag.ToString();
                if (version_section_id != null && version_section_id != "")
                {

                    this.Cursor = Cursors.Wait;
                    var con = await SQLQuery.GetVersionNSectionConnection(version_section_id);
                    await SQLQuery.UpdateVersionNSection(version_section_id, con[0]["version_id"], "0");
                    wordFile = new WordFile(version_id: con[0]["version_id"]);
                    LoadDocument(MainDoc, wordFile);
                    this.Cursor = Cursors.Arrow;
                }
            }
        }
        
        private async void EditSection(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null && version_id != "")
            {
                string? version_section_id = item.Tag.ToString();
                if (version_section_id != null && version_section_id != "")
                {
                    var window = new InputText("");
                    window.TextBlock1.Text = "Название раздела";
                    window.ComboBox.Visibility = Visibility.Visible;
                    window.TextBlock2.Text = "Уровень заголовка";
                    window.ComboBox.Items.Add("1");
                    window.ComboBox.Items.Add("2");
                    window.ComboBox.Items.Add("3");
                    window.ComboBox.SelectedIndex = 0;
                    window.ShowDialog();
                    if (window.DialogResult != null && window.DialogResult.Value)
                    {
                        this.Cursor = Cursors.Wait;
                        var sections = await SQLQuery.ReadSections(version_id);
                        var versions_sections = await SQLQuery.GetVersionNSectionConnection(version_section_id);
                        foreach (var section in sections)
                        {
                            await SQLQuery.DeleteVersionNSectionConnection(version_id, section["id"]);
                            if (versions_sections[0]["section_id"] == section["id"])
                            {
                                var old_section = await SQLQuery.GetSection(section["id"]);

                                string name = $"<w:p xmlns:w14=\"http://schemas.microsoft.com/office/word/2010/wordml\" xmlns:w=\"http://schemas.openxmlformats.org/wordprocessingml/2006/main\"><w:pPr><w:pStyle w:val=\"{window.ComboBox.Text}\" /></w:pPr><w:r></w:r><w:r ><w:t>{window.TextBox.Text}</w:t></w:r></w:p>";

                                var section_id = await SQLQuery.AddSection(name, $"<w:pStyle w:val=\"{window.ComboBox.Text}\" />", old_section[0]["text"]);
                                await SQLQuery.ConnectVersionNSection(version_id, section_id);
                            }
                            else
                            {
                                await SQLQuery.ConnectVersionNSection(version_id, section["id"]);
                            }
                        }

                        wordFile = new WordFile(version_id: version_id);
                        LoadDocument(MainDoc, wordFile);
                        this.Cursor = Cursors.Arrow;
                    }
                }
            }
        }
        
        
        private async void CommentSection(object sender, RoutedEventArgs e)
        {
            try
            {
                CancelButton_Click(sender, e);

                MenuItem? item = sender as MenuItem;

                if (item != null && version_id != "")
                {
                    string? version_section_id = item.Tag.ToString();
                    if (version_section_id != null && version_section_id != "")
                    {
                        commentTextBox.Tag = version_section_id;
                        var comment_text = await SQLQuery.ReadComment(version_section_id);
                        if (comment_text.Count == 0)
                            await SQLQuery.AddComment(version_section_id, "", "user");
                        trans = new Transaction(version_section_id, this);
                        OpenComment();
                        commentTextBox.Text = trans.StartTransaction();
                    }
                }

            }
            catch
            {
                CloseComment(false);
            }
        }

        private void on_Double_Click_Section(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = (ListBoxItem)sender;

            var targetSection = MainDoc.Document.Blocks
                .OfType<Paragraph>()
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            string? version_section_id = commentTextBox.Tag as string;

            if (trans != null && version_section_id != null)
            {
                trans.Comment = commentTextBox.Text;
                trans.SaveAndClose();

                CloseComment();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseComment();
            if (trans != null)
            {
                if (trans.Close())
                    trans = null;
            }
        }

        private void CloseComment(bool isFine = true)
        {
            if (!isFine)
            {
                MessageBox.Show("Не удалось подключиться, возможно комментарий открыт другим устройством.");
            }
            commentTextBox.Tag = null;
            commentTextBox.Text = "";
            commentTextBox.IsEnabled = false;
            SaveButton.IsEnabled = false;
            CancelButton.IsEnabled = false;
        }

        private void OpenComment()
        {
            commentTextBox.IsEnabled = true;
            SaveButton.IsEnabled = true;
            CancelButton.IsEnabled = true;
        }

        private async void compareButton_Click(object sender, RoutedEventArgs e)
        {
            if (SecondDoc.Visibility == Visibility.Collapsed)
            {
                string document_id = (await SQLQuery.GetVersion(version_id))[0]["document_id"];
                var versions = await SQLQuery.ReadVersions(document_id);
                var res = new List<DataBaseEntities.Version>();
                foreach(var version in versions)
                {
                    res.Add(new DataBaseEntities.Version(version["id"], version["document_id"], version["version"], version["create_date"]));
                }

                ChooseVersion window = new ChooseVersion(res);
                window.ShowDialog();

                if (window.DialogResult != null && window.DialogResult.Value)
                {
                
                    if (window.chosenVersion != null)
                    {
                        this.Cursor = Cursors.Wait;
                        WordFile wordFile = new WordFile("templates/Empty.docx", window.chosenVersion.id);

                        this.Cursor = Cursors.Arrow;
                        LoadDocument(SecondDoc, wordFile, false);
                        SecondDoc.Visibility = Visibility.Visible;
                        compareButton.Content = "Закрыть сравнение";
                        CompareDocuments(SecondDoc.Document, MainDoc.Document);
                        
                    }
                }

            }
            else
            {
                SecondDoc.Document.Blocks.Clear();
                SecondDoc.Visibility = Visibility.Collapsed;
                compareButton.Content = "Сравнить версии документа";
                ResetFormatting(MainDoc.Document);
            }
        }

        public static void CompareDocuments(FlowDocument doc1, FlowDocument doc2)
        {
            var paragraphs1 = ExtractParagraphs(doc1);
            var paragraphs2 = ExtractParagraphs(doc2);

            foreach (var id in paragraphs1.Keys.Union(paragraphs2.Keys))
            {
                bool inFirst = paragraphs1.ContainsKey(id);
                bool inSecond = paragraphs2.ContainsKey(id);

                if (inFirst && inSecond)
                {
                    string text1 = GetText(paragraphs1[id]);
                    string text2 = GetText(paragraphs2[id]);

                    if (text1 != text2)
                    {
                        HighlightParagraph(paragraphs1[id], Brushes.Yellow);
                    }
                }
                else if (inFirst)
                {
                    HighlightParagraph(paragraphs1[id], Brushes.Red);
                }
                else if (inSecond)
                {
                    HighlightParagraph(paragraphs2[id], Brushes.Green);
                }
            }
        }

        private static string GetText(Paragraph paragraph)
        {
            return new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text.Trim();
        }

        private static Dictionary<string, Paragraph> ExtractParagraphs(FlowDocument doc)
        {
            var paragraphs = new Dictionary<string, Paragraph>();

            foreach (var block in doc.Blocks)
            {
                if (block is Paragraph paragraph && paragraph.Tag is string id)
                {
                    paragraphs[id] = paragraph;
                }
            }

            return paragraphs;
        }

        private static void HighlightParagraph(Paragraph paragraph, Brush color)
        {
            TextRange range = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);
            range.ApplyPropertyValue(TextElement.BackgroundProperty, color);
        }

        private static void ResetFormatting(FlowDocument doc)
        {
            foreach (var block in doc.Blocks.ToList())
            {
                if (block is Paragraph paragraph)
                {
                    var range = new TextRange(paragraph.ContentStart, paragraph.ContentEnd);

                    range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
                }
            }
        }

        class Transaction
        {
            protected string? connectionString;
            private MySqlConnection connection;
            private MySqlTransaction transaction;
            private System.Timers.Timer timer;
            private string version_section_id;
            Document window;
            public string Comment { get; set; }

            public Transaction(string version_section_id, Document window)
            {
                connectionString = SDMMOperations.MySQLConnector.GetConnectionString();
                connection = new MySqlConnection(connectionString);
                this.version_section_id = version_section_id;
                this.window = window;
            }

            public string? StartTransaction(string sql = "")
            {
                timer = new System.Timers.Timer(300000);
                timer.Elapsed += TimerElapsed;
                timer.AutoReset = false;
                timer.Start();
                string? res = "";
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);

                    sql = "SELECT text FROM comment WHERE version_section_id = @version_section_id FOR UPDATE NOWAIT";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@version_section_id", version_section_id);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                res = reader["text"].ToString();
                            }
                        }
                    }
                }
                catch 
                {
                    
                    window.CloseComment(false);
                }
                return res;
            }

            private void TimerElapsed(object sender, ElapsedEventArgs e)
            {
                SaveAndClose();
            }

            public void SaveAndClose()
            {
                try
                {
                    string sql = "UPDATE comment SET text = @text WHERE version_section_id = @version_section_id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, connection, transaction))
                    {                        cmd.Parameters.AddWithValue("@text", Comment);
                        cmd.Parameters.AddWithValue("@version_section_id", version_section_id);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
                finally
                {
                    connection?.Close();
                    timer?.Stop();
                    window.CloseComment();
                    
                }
            }

            public bool Close()
            {
                try
                {
                    transaction?.Rollback();
                    return true;
                }
                catch
                {
                    return false;
                }
                finally
                {
                    connection?.Close();
                }
            }
        }

    }
}
