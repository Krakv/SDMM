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
    /// Логика взаимодействия для EditVersion.xaml
    /// </summary>
    public partial class EditVersion : Window
    {
        string version_id;

        public EditVersion(string version_id)
        {
            InitializeComponent();
            this.version_id = version_id;
            FillField();
        }

        private void FillField()
        {
            var version = SQLQuery.GetVersion(version_id);
            versionTextBox.Text = version[0]["version"];
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {

            if (versionTextBox.Text.Trim() != "")
            {
                SQLQuery.UpdateVersion(version_id, versionTextBox.Text);
                this.Close();
            }
            else
                MessageBox.Show("Версия не должна быть пустая");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
