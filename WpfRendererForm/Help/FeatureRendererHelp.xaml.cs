using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace WpfRendererForm.Help
{
    /// <summary>
    /// Interaktionslogik für FeatureRendererHelp.xaml
    /// </summary>
    public partial class FeatureRendererHelp : NavigationWindow
    {
        private bool _isInitializing = false;
        public delegate void NeuerEventHandler();

        public FeatureRendererHelp()
        {
            this._isInitializing = true;
            InitializeComponent();
            this._isInitializing = false;
            TreeViewItem item = (TreeViewItem)tree.SelectedItem;
            this.frame.Source = new Uri(item.Tag.ToString(), UriKind.RelativeOrAbsolute);
        }

        private void HelponselectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this._isInitializing == false)
            {
                TreeViewItem item = (TreeViewItem)tree.SelectedItem;
                this.frame.Source = new Uri(item.Tag.ToString(), UriKind.RelativeOrAbsolute);
            }
        }
    }
}
