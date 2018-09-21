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
    /// Interaction logic for LoadingModal.xaml
    /// </summary>
    public partial class LoadingModal : Window
    {
        public LoadingModal(Action action, string message, Window owner)
        {
            Owner = owner;

            InitializeComponent();

            LoadingLabel.Content = message;

            RunAsync(action);
        }

        private async void RunAsync(Action action)
        {
            await Task.Run(action);

            DialogResult = true;
        }
    }
}
