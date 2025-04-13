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
    /// Логика взаимодействия для SaveNewVersion.xaml
    /// </summary>
    public partial class SaveNewVersion : Window
    {
        string document_id;

        public SaveNewVersion(string document_id)
        {
            InitializeComponent();
            this.document_id = document_id;
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


        private async void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            if (versionTextBox.Text.Trim() != "")
            {
                if (duplicateCheckBox.IsChecked != null)
                {
                    try
                    {
                        if (duplicateCheckBox.IsChecked.Value)
                        {
                            var version = await SQLQuery.GetLastVersion(document_id);
                            string version_id = version[0]["id"];

                            var sections = await SQLQuery.ReadSections(version_id);

                            string new_version_id = await SQLQuery.AddVersion(document_id, versionTextBox.Text);

                            foreach(var section in sections)
                            {
                                string new_section_id = await SQLQuery.AddSection(section["name"], section["style"], section["text"]);
                                await SQLQuery.ConnectVersionNSection(new_version_id, new_section_id);
                            }
                        }
                        else
                        {
                            try
                            {
                                WordFile wordFile = new WordFile(path.Text);

                                var sections = wordFile.GetSections();

                                string new_version_id = await SQLQuery.AddVersion(document_id, versionTextBox.Text);

                                foreach (var section in sections)
                                {
                                    string new_section_id = await SQLQuery.AddSection(section.name, section.style, section.text);
                                    await SQLQuery.ConnectVersionNSection(new_version_id, new_section_id);
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Не удалось загрузить файл.");
                            }
                        }
                    }
                    finally
                    {
                        this.Close();
                    }

                }
                
            }
            else
                MessageBox.Show("Версия не должна быть пустая");

        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void duplicateCheckBox_checked(object sender, RoutedEventArgs e)
        {
            path.IsEnabled = false;
            openPathButton.IsEnabled = false;
        }

        private void duplicateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            path.IsEnabled = true;
            openPathButton.IsEnabled = true;
        }
    }
}
