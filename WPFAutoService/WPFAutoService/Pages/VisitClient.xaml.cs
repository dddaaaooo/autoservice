using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
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
using static WPFAutoService.MainWindow;

namespace WPFAutoService.Pages
{
    /// <summary>
    /// Логика взаимодействия для VisitClient.xaml
    /// </summary>
    public partial class VisitClient : Page
    {
        List<ClientService> clientServiceList = new List<ClientService>(); // список клиентов
        private Client _client;
        public VisitClient(Client client)
        {
            _client = client;
            InitializeComponent();
            LoadService();
        }
        private void LoadService()
        {
            var clientServices = helper.GetContext().ClientService
                .Where(cs => cs.ClientID == _client.ID)
                .ToList();

            foreach (var clientservice in clientServices)
            {
                clientservice.TitleService = clientservice.Service.Title;
                clientservice.TimeService = clientservice.StartTime.ToString("yyyy.MM.dd HH:mm");

                clientServiceList.Add(clientservice);
            }
            serviceGrid.ItemsSource = clientServiceList.ToList();

        }
    }
}
