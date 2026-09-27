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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

namespace PlayniteAnywhere
{
    public partial class PlayniteAnywhereSettingsView : UserControl
    {
        public PlayniteAnywhereSettingsView()
        {
            InitializeComponent();
        }

        private void LocalUrl_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }

        private void GitHub_RequestNavigate(
            object sender,
            RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}