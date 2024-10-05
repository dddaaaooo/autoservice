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

using WPFAutoService.Models;
using WPFAutoService.Pages;

namespace WPFAutoService
{
    public partial class MainWindow : Window
    {
        public class helper
        {
            public static AutoServiceEntities ent;
            public static AutoServiceEntities GetContext()
            {
                if (ent == null)
                {
                    ent = new AutoServiceEntities();
                }
                return ent;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            //frame.Content = new ClientGRUD();
            frame.Navigate(new ClientGRUD());

        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            frame.GoBack();
        }
        //private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        //{
        //    try
        //    {
        //        ClientGRUD pg = (ClientGRUD)e.Content;
        //        pg.displayClient();
        //    }
        //    catch { };
        //}
        private void frame_ContentRendered(object sender, EventArgs e)
        {
            if (frame.CanGoBack)
                btnBack.Visibility = Visibility.Visible;
            else
                btnBack.Visibility = Visibility.Hidden;

        }
    }
}