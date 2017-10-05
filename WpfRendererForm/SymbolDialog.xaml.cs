using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;

namespace WpfRendererForm
{
    /// <summary>
    /// Interaktionslogik für SymbolDialog.xaml
    /// </summary>
    public partial class SymbolDialog : Window
    {
        FeatureRendererWindow frw;
        bool btnClicked = false;

        //Konstruktor:
        public SymbolDialog(FeatureRendererWindow window1)
        {
            frw = window1;
            InitializeComponent();           
        }

        //properties:
        public bool BtnClicked
        {
            get
            {
                return this.btnClicked;
            }
        }

        private void btnSymbolDirectory_Click(object sender, RoutedEventArgs e)
        {
            //BrowseForFolderDialog dlg = new BrowseForFolderDialog();
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select a directory and click OK!";
            dlg.SelectedPath = Environment.SpecialFolder.Desktop.ToString();
            //dlg.OKButtonText = "OK!";
            if (System.Windows.Forms.DialogResult.OK == dlg.ShowDialog())
            {
                // Do something with the selected folder...
                //MessageBox.Show(dlg.SelectedFolder, "Selected Folder");
                //this.txtSymbolDirectory.Text = dlg.SelectedFolder;
                this.txtSymbolDirectory.Text = dlg.SelectedPath;
            }
        }

        private void btnSymbol_Click(object sender, RoutedEventArgs e)
        {
            frw.SymbolDirectory = this.txtSymbolDirectory.Text;
            this.btnClicked = true;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtSymbolDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (this.txtSymbolDirectory.Text != String.Empty && Directory.Exists(this.txtSymbolDirectory.Text) == true)
            {
                this.btnSymbol.IsEnabled = true;
            }
            else
            {
                this.btnSymbol.IsEnabled = false;
            }
        }
    }
}
