using System.Windows;

namespace HeedNotify
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
            ShowInTaskbar = false;
        }
    }
}
