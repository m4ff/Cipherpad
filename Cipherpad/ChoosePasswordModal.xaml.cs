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

namespace Cipherpad
{
    /// <summary>
    /// Interaction logic for ChoosePasswordModal.xaml
    /// </summary>
    public partial class ChoosePasswordModal : Window
    {
        public ChoosePasswordModal()
        {
            InitializeComponent();
        }

        public string Passphrase { get; private set; }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if(!string.IsNullOrEmpty(PasswordFirst.Password) && PasswordFirst.Password == PasswordSecond.Password)
            {
                Passphrase = PasswordFirst.Password;
                DialogResult = true;
            }
            else
            {
                ErrorMessage.Visibility = Visibility.Visible;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
