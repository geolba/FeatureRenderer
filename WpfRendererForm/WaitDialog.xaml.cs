using System.Windows;
using System.Windows.Input;

namespace WpfRendererForm
{
    /// <summary>
    /// Interaktionslogik für WaitDialog.xaml
    /// </summary>
    public partial class WaitDialog : Window
    {
        public WaitDialog()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
