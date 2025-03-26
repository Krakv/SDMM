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
    /// Логика взаимодействия для InputText.xaml
    /// </summary>
    public partial class InputText : Window
    {
        public InputText(string text)
        {
            InitializeComponent();
            TextBox.Text = text;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void Save_Button_Click(object sender, RoutedEventArgs e)
        {
            bool isValidText = MainWindow.IsValidInput(TextBox.Text);
            bool isValidCombo = MainWindow.IsValidInput(ComboBox.Text);
            if (isValidText && isValidCombo)
            {
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Неверный ввод");
            }

        }
    }
}
