using DocumentFormat.OpenXml.Office2010.Word;
using MySql.Data.MySqlClient;
using SDMMOperations;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SDMM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        string chosen_document_id = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadDocuments(List<string[]> docs)
        {
            DocumentsList.Items.Clear();
            foreach (string[] doc in docs)
            {
                StackPanel panel = new StackPanel() { MinWidth = 350, Margin = new Thickness(10, 5, 10, 5) };
                panel.Children.Add(new Separator() { Margin = new Thickness(0, 0, 0, 10) });

                StackPanel docName = new StackPanel() { Orientation = Orientation.Horizontal };
                docName.Children.Add(new TextBlock() { Text = "*", FontSize = 16, FontWeight = FontWeight.FromOpenTypeWeight(900), Margin = new Thickness(5, 5, 5, 5) });
                docName.Children.Add(new TextBlock() { Text = $"{doc[1]} - {doc[2]}", FontSize = 16, FontWeight = FontWeight.FromOpenTypeWeight(900), Margin = new Thickness(5, 5, 5, 5), TextWrapping = TextWrapping.Wrap, MaxWidth=300 });

                panel.Children.Add(docName);
                panel.Children.Add(new TextBlock() { Text = "Статус: " + doc[3], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = "Версия: " + doc[4], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = "Автор: " + doc[5], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = "Размер: " + doc[6], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = "Дата: " + doc[7], Style = (Style)this.Resources["Style1"] });
                panel.Children.Add(new TextBlock() { Text = "Теги: " + doc[8], Style = (Style)this.Resources["Style1"] });

                panel.Children.Add(new Separator() { Margin = new Thickness(0, 10, 0, 0) });

                ListBoxItem item = new ListBoxItem() { Content = panel, Tag = doc[0] };
                item.MouseDoubleClick += Document_Click;
                item.ContextMenu = GetDocumentContextMenu(doc[0]);
                
                DocumentsList.Items.Add(item);
            }
        }

        private void LoadVersions(List<Dictionary<string, string>> vers)
        {
            VersionsList.Items.Clear();
            foreach (var ver in vers)
            {
                StackPanel panel = new StackPanel() { Orientation = Orientation.Horizontal };
                panel.Children.Add(new TextBlock() { Text = "Версия: " + ver["version"] });
                panel.Children.Add(new TextBlock() { Text = " | " });
                panel.Children.Add(new TextBlock() { Text = "Дата создания: " + ver["create_date"] });

                ListBoxItem item = new ListBoxItem() { Content = panel, Tag = ver["id"] };
                item.MouseDoubleClick += OpenVersion_Click;
                item.ContextMenu = GetVersionContextMenu(ver["id"]);
                VersionsList.Items.Add(item);
            }
            SaveNewVersionButton.Visibility = Visibility.Visible;
        }

        private void LoadDocumentInfo(string[] doc)
        {
            chosen_document_id = doc[0];
            DocumentInfo_DocProjectName.Text = "Название проекта: " + doc[1];
            DocumentInfo_DocTypeName.Text = "Название типа документа: " + doc[2];
            DocumentInfo_DocStatus.Text = "Статус: " + doc[3];
            DocumentInfo_DocVer.Text = "Последняя версия документа: " + doc[4];
            DocumentInfo_DocAuthor.Text = "Автор: " + doc[5];
            DocumentInfo_DocSize.Text = "Размер документа: " + doc[6];
            DocumentInfo_DocDate.Text = "Дата изменения информации: " + doc[7];
            DocumentInfo_DocTags.Text = "Теги: " + doc[8];
        }

        private void Document_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem? item = sender as ListBoxItem;
            if (item != null)
            {
                string? document_id = item.Tag.ToString();
                if (document_id != null)
                {
                    LoadDocumentInfo(document_id);
                }
            }
        }

        private async void LoadDocumentInfo(string document_id)
        {
            using(new WaitCursorScope())
            {
                this.IsEnabled = false;
                var getDocumentTask = SQLQuery.GetDocument(document_id);
                await getDocumentTask;
                var document = getDocumentTask.Result[0];

                var readVersionsTask = SQLQuery.ReadVersions(document["id"]);
                var getDocumentTypeInfoTask = SQLQuery.GetDocumentTypeInfo(document["type_id"]);
                var getProjectTask = SQLQuery.GetProject(document["project_id"]);
                var readTagsTask = SQLQuery.ReadTags(document["id"]);

                await Task.WhenAll(readVersionsTask, getDocumentTypeInfoTask, getProjectTask, readTagsTask);

                var versions = readVersionsTask.Result;
                string document_type = getDocumentTypeInfoTask.Result[0]["name"];
                string project = getProjectTask.Result[0]["name"];
                string tags = string.Join(" ", readTagsTask.Result.Select(tag => tag["name"]));

                string[] info = [document["id"], project, document_type, document["status"], versions[0]["version"], document["author"], document["size"] + " байт", document["date"], tags];

                LoadDocumentInfo(info);
                LoadVersions(versions);
                this.IsEnabled = true;
            }
        }

        private void OpenVersion_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem? item = sender as ListBoxItem;
            if (item != null)
            {
                string? version_id = item.Tag as string;

                if (version_id != null)
                {
                    SDMM.Document document; 
                    using(new WaitCursorScope())
                    {
                        WordFile wordFile = new WordFile("templates/Empty.docx", version_id);
                        document = new SDMM.Document(wordFile, version_id);
                    }
                    document.ShowDialog();
                }
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Это программа на WPF!");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            using (new WaitCursorScope())
            {

                var documents = await SQLQuery.FindDocuments(SearchTextBox.Text);
                Dictionary<string, bool> d = new Dictionary<string, bool>();
                HashSet<string> set = new HashSet<string>();
                List<string[]> result = new List<string[]>();

                foreach (var document in documents)
                {
                    if (!set.Contains(document["id"]))
                    {
                        var getLastVersionTask = SQLQuery.GetLastVersion(document["id"]);
                        var readTagsTask = SQLQuery.ReadTags(document["id"]);
                        await getLastVersionTask;
                        await readTagsTask;

                        string version = getLastVersionTask.Result[0]["version"];
                        string tags = "";
                        foreach (var tag in readTagsTask.Result)
                            tags += tag["name"] + " ";

                        string[] info = [document["id"], document["project_name"], document["document_type_name"], document["status"], version, document["author"], document["size"] + " байт", document["date"], tags];
                        result.Add(info);
                        set.Add(document["id"]);
                    }
                }

                LoadDocuments(result);
            }
        }

        private async void createProject_Click(object sender, RoutedEventArgs e)
        {
            AddDocumentPack window = new AddDocumentPack();
            window.ShowDialog();
            await LoadDocumentsList();
        }

        private async void createDocument_Click(object sender, RoutedEventArgs e)
        {
            AddDocument window = new AddDocument();
            window.ShowDialog();
            await LoadDocumentsList();
        }

        private async void MainWindows_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadDocumentsList();
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Не удалось подключиться к базе данных");
                this.Close();
            }
        }

        private async Task LoadDocumentsList()
        {
            using (new WaitCursorScope())
            {
                this.IsEnabled = false;
                List<string[]> result = new List<string[]>();

                List<Dictionary<string, string>> projects = await SQLQuery.ReadProjects();

                foreach (var project in projects)
                {
                    List<Dictionary<string, string>> documents = await SQLQuery.ReadDocuments(project["id"]);

                    foreach (var document in documents)
                    {
                        var getDocumenTypeInfoTask = SQLQuery.GetDocumentTypeInfo(document["type_id"]);
                        var getLastVersionTask = SQLQuery.GetLastVersion(document["id"]);
                        var readTagsTask = SQLQuery.ReadTags(document["id"]);
                        await getDocumenTypeInfoTask;
                        await getLastVersionTask;
                        await readTagsTask;

                        string document_type = getDocumenTypeInfoTask.Result[0]["name"];
                        string version = getLastVersionTask.Result[0]["version"];
                        string tags = "";
                        foreach (var tag in readTagsTask.Result)
                            tags += tag["name"] + " ";

                        string[] info = [document["id"], project["name"], document_type, document["status"], version, document["author"], document["size"] + " Bytes", document["date"], tags];
                        result.Add(info);
                    }
                    this.IsEnabled = true;
                }


                LoadDocuments(result);
            }
        }

        private void SaveNewVersionButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosen_document_id != null)
            {
                SaveNewVersion window = new SaveNewVersion(chosen_document_id);
                window.ShowDialog();

                LoadDocumentInfo(chosen_document_id);
            }
        }

        private ContextMenu GetVersionContextMenu(string id)
        {
            ContextMenu menu = new ContextMenu() ;
            MenuItem item;

            item = new MenuItem() { Tag = id };
            item.Click += DownloadVersion;
            item.Header = "Скачать";
            menu.Items.Add(item);

            item = new MenuItem() { Tag = id };
            item.Click += EditVersion;
            item.Header = "Редактировать";
            menu.Items.Add(item);

            item = new MenuItem() { Tag = id };
            item.Click += DeleteVersion;
            item.Header = "Удалить";
            menu.Items.Add(item);

            return menu;
        }
        
        private ContextMenu GetDocumentContextMenu(string id)
        {
            ContextMenu menu = new ContextMenu() ;
            MenuItem item;

            item = new MenuItem() { Tag = id };
            item.Click += DownloadDocument;
            item.Header = "Скачать";
            menu.Items.Add(item);

            item = new MenuItem() { Tag = id };
            item.Click += EditDocument;
            item.Header = "Редактировать";
            menu.Items.Add(item);

            item = new MenuItem() { Tag = id };
            item.Click += DeleteDocument;
            item.Header = "Удалить";
            menu.Items.Add(item);

            return menu;
        }

        private async void DownloadDocument(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null)
            {
                string? document_id = item.Tag.ToString();

                if (document_id != null)
                {
                    VersionDownloading window;
                    using (new WaitCursorScope())
                    {
                        var getDocumentNameTask = SQLQuery.GetDocumentName(document_id);
                        var getLastVersionTask = SQLQuery.GetLastVersion(document_id);
                        await getDocumentNameTask;
                        await getLastVersionTask;


                        var names = getDocumentNameTask.Result;
                        var versions = getLastVersionTask.Result;
                        window = new VersionDownloading(versions[0]["id"], names[0]["project_name"] + names[0]["document_type_name"]);
                    }
                    window.ShowDialog();
                }
            }
        }

        private async void EditDocument(object sender, RoutedEventArgs e)
        {

            MenuItem? item = sender as MenuItem;

            if (item != null)
            {
                string? document_id = item.Tag.ToString();

                if (document_id != null)
                {
                    EditDocument window;
                    using (new WaitCursorScope()) {
                        var getDocumentTask = SQLQuery.GetDocument(document_id);
                        await getDocumentTask;
                        var documents = getDocumentTask.Result;

                        var getDocumentTypeInfoTask = SQLQuery.GetDocumentTypeInfo(documents[0]["type_id"]);
                        var getProjectTask = SQLQuery.GetProject(documents[0]["project_id"]);
                        await getDocumentTypeInfoTask;
                        await getProjectTask;


                        var document_types = getDocumentTypeInfoTask.Result;
                        var projects = getProjectTask.Result;

                        DataBaseEntities.Document doc = new DataBaseEntities.Document(document_id,
                        new DataBaseEntities.Project(projects[0]["id"], projects[0]["name"]),
                        new DataBaseEntities.DocumentType(document_types[0]["id"], document_types[0]["name"], document_types[0]["standart_id"], document_types[0]["template_id"], document_types[0]["description"]),
                        documents[0]["author"], documents[0]["date"], documents[0]["status"], documents[0]["size"]);

                        window = new EditDocument(doc);
                    }

                    window.ShowDialog();
                    await LoadDocumentsList();
                }
            }
        }

        private async void DeleteDocument(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Вы точно хотите удалить документ, со всем содержимым?", "Удаление", MessageBoxButton.YesNo);

            if (res == MessageBoxResult.Yes)
            {
                MenuItem? item = sender as MenuItem;

                if(item != null)
                {
                    string? document_id = item.Tag.ToString();

                    if (document_id != null)
                    {
                        using(new WaitCursorScope())
                        {
                            await SQLQuery.DeleteDocument(document_id);
                        }
                        await LoadDocumentsList();
                    }
                }
            }
        }

        private async void DownloadVersion(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null)
            {
                string? version_id = item.Tag.ToString();

                if (version_id != null)
                {
                    VersionDownloading window;

                    using (new WaitCursorScope())
                    {
                        var getLastVersionTask = SQLQuery.GetVersion(version_id);
                        await getLastVersionTask;
                        var versions = getLastVersionTask.Result;


                        var getDocumentNameTask = SQLQuery.GetDocumentName(versions[0]["document_id"]);
                        await getDocumentNameTask;
                        var names = getDocumentNameTask.Result;

                        window = new VersionDownloading(versions[0]["id"], names[0]["project_name"] + names[0]["document_type_name"]);
                    }
                    window.ShowDialog();
                }
            }
        }

        private void EditVersion(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;

            if (item != null)
            {
                string? version_id = item.Tag.ToString();

                if (version_id != null)
                {
                    EditVersion window;
                    using (new WaitCursorScope()) { window = new EditVersion(version_id); }
                    window.ShowDialog();
                    LoadDocumentInfo(chosen_document_id);
                }
            }
        }

        private async void DeleteVersion(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show("Вы точно хотите удалить версию документа, со всем содержимым?", "Удаление", MessageBoxButton.YesNo);

            if (res == MessageBoxResult.Yes)
            {
                MenuItem? item = sender as MenuItem;

                if(item != null)
                {
                    string? version_id = item.Tag.ToString();

                    if (version_id != null)
                    {
                        using (new WaitCursorScope())
                        {
                            await SQLQuery.DeleteVersion(version_id);
                        }
                        LoadDocumentInfo(chosen_document_id);
                    }
                }
            }

        }

        static public bool IsValidInput(string input)
        {
            return Regex.IsMatch(input, @"^[a-zA-ZА-Яа-я0-9 ]+$");
        }
    }
}