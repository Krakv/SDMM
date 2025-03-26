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
    /// Логика взаимодействия для ChooseVersion.xaml
    /// </summary>
    public partial class ChooseVersion : Window
    {
        List<DataBaseEntities.Version> versions;
        public DataBaseEntities.Version? chosenVersion = null;

        public ChooseVersion(List<DataBaseEntities.Version> versions)
        {
            InitializeComponent();
            this.versions = versions;
            LoadVersionsList();
        }

        private void LoadVersionsList()
        {
            foreach (var version in versions)
            {
                ListBoxItem item = new ListBoxItem() { Content = version };
                item.MouseDoubleClick += ChooseVersion_Click;
                versionsList.Items.Add(item);
            }
        }

        private void ChooseVersion_Click(object sender, EventArgs ar)
        {
            ListBoxItem? item = sender as ListBoxItem;
            if (item != null)
            {
                chosenVersion = item.Content as DataBaseEntities.Version;
                if (chosenVersion != null)
                {
                    DialogResult = true;
                    this.Close();
                }
            }
        }
    }
}
