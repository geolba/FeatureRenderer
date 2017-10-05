using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace WpfRendererForm
{
    /// <summary>
    /// Interaktionslogik für ProgressDialog.xaml
    /// </summary>
    public partial class ProgressDialog : Window
    {
        

        public ProgressDialog()
        {
            InitializeComponent();
        }

        


        //public event EventHandler Cancel = delegate { };
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            //Cancel(sender, e);
            this.IsEnabled = false;
            this.Close();
        }

         public string ProgressText
         {
             set
             {
                 this.StatusText.Content = value;
             }
         }  

        public int ProgressValue
        {
            set
            {
                this.Progress.Value = value;
            }
        }

        

       


    }
}
