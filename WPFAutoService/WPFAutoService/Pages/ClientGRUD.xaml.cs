using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Globalization;
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
    /// Логика взаимодействия для ClientGRUD.xaml
    /// </summary>
    public partial class ClientGRUD : Page
    {
        List<Client> clientList = new List<Client>(); // список клиентов
        private int recordsPerPage = 10; // количество записей на странице
        private int startPage = 0; // текущая страница
        private int clientFullCount = 0; // всего клиентов
        private int clientCount = 0; // всего клиентов отображено
        private string clientGenderFilter = "Все";
        private string clientSortBy = "Без сортировки";
        private string clientSearch = "";
        private bool clientBirthday = false;

        public ClientGRUD()
        {
            InitializeComponent();
            LoadClient();
            displayClient();
        }

        public void LoadClient()
        {
            try
            {
                clientList.Clear();
                // получение всех агентов
                var allclients = helper.GetContext().Client.ToList();
                clientFullCount = helper.GetContext().Client.Count();
                // получение количества визитов и дата последнего визита клиента
                foreach (var client in allclients)
                {
                    var clientServices = helper.GetContext().ClientService
                        .Where(cs => cs.ClientID == client.ID)
                        .OrderByDescending(cs => cs.StartTime)
                        .FirstOrDefault();
                    var clientTags = client.Tag;

                    if (clientServices != null)
                    {
                        client.LastVisitDate = clientServices.StartTime;
                        client.VisitCount = helper.GetContext().ClientService
                            .Count(cs => cs.ClientID == client.ID);
                    }

                    if (clientTags != null)
                    {
                        var tagColor = string.Join(", ", clientTags.Select(t => $"#{t.Color}"));
                        var tagTitle = string.Join(", ", clientTags.Select(t => t.Title));
                        client.TagString = tagTitle;
                        client.TagColor = tagColor;
                    }
                    clientList.Add(client);
                }
            }
            catch
            {
                return;
            }
        }
        public void displayClient()
        {
            var clientFiltrList = clientList
                .Where(client =>
                    (clientGenderFilter == "Все") || (clientGenderFilter == "") ||
                    (clientGenderFilter == "Мужской" && client.Gender.Code == "м") ||
                    (clientGenderFilter == "Женский" && client.Gender.Code == "ж"))
                .Where(client =>
                    (client.FirstName.ToLower().Contains(clientSearch) ||
                     client.LastName.ToLower().Contains(clientSearch) ||
                     client.Patronymic.ToLower().Contains(clientSearch) ||
                     client.Email.ToLower().Contains(clientSearch) ||
                     client.Phone.Contains(clientSearch)))
                .ToList();
            if (!string.IsNullOrEmpty(clientSortBy))
            {
                clientFiltrList = SortClients(clientFiltrList, clientSortBy);
            }

            if (clientBirthday)
            {
                var currentMonth = DateTime.Now.Month;
                clientFiltrList = clientFiltrList
                    .Where(c => c.Birthday?.Month == currentMonth)
                    .ToList();
            }

            clientCount = clientFiltrList.Count();
            clientGrid.ItemsSource = clientFiltrList.Skip(startPage * recordsPerPage).Take(recordsPerPage).ToList();
            RecordCountTextBlock.Text = $"{clientCount} из {clientFullCount}";


            // логика панигации
            int ost = clientCount % recordsPerPage;
            int pag = (clientCount - ost) / recordsPerPage;
            if (ost > 0) pag++;
            pagin.Children.Clear();
            for (int i = 0; i < pag; i++)
            {
                Button myButton = new Button
                {
                    Height = 25,
                    Content = i + 1,
                    Width = 20,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Tag = i
                };
                myButton.Click += new RoutedEventHandler(paginButton_Click); ;
                pagin.Children.Add(myButton);
            }

            turnButton();
            changeButtonColor();

        }
        private List<Client> SortClients(List<Client> clients, string sortBy)
        {
            switch (sortBy)
            {
                case "Фамилия":
                    return clients.OrderBy(client => client.LastName).ToList();
                case "Последний визит":
                    return clients
                        .OrderByDescending(client => client.LastVisitDate.HasValue)
                        .ThenByDescending(client => client.LastVisitDate.GetValueOrDefault())
                        .ToList();
                case "Количество визитов":
                    return clients.OrderByDescending(c => c.VisitCount).ToList();
                default:
                    return clients;
            }
        }
        private void BirthdayThisMonthCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            clientBirthday = (bool)BirthdayThisMonthCheckBox.IsChecked;
            startPage = 0;
            displayClient();
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            clientSearch = SearchTextBox.Text.ToLower();
            startPage = 0;
            displayClient();
        }
        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            var selectedSort = selectedItem.Content.ToString();
            clientSortBy = selectedSort;
            startPage = 0;
            displayClient();
        }
        private void GenderFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;
            var selectedGender = selectedItem.Content.ToString();

            clientGenderFilter = selectedGender;
            startPage = 0;
            displayClient();
        }
        private void RecordsPerPageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            ComboBoxItem selectedItem = (ComboBoxItem)comboBox.SelectedItem;

            if (selectedItem.Content.ToString() == "Все")
            {
                recordsPerPage = clientCount;
                startPage = 0;
                displayClient();
            }
            else
            {
                recordsPerPage = Convert.ToInt32(selectedItem.Content.ToString());
                startPage = 0;
                displayClient();
            }

        }
        //Нажатие на кнопку переключения страниц
        private void paginButton_Click(object sender, RoutedEventArgs e)
        {
            startPage = Convert.ToInt32(((Button)sender).Tag.ToString());
            displayClient();
        }
        //Кнопка назад
        private void backButton_Click(object sender, RoutedEventArgs e)
        {
            startPage--;
            displayClient();
        }
        //Внопка вперед
        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            startPage++;
            displayClient();
        }
        // блокировка кнопок назад и вперед на первой и последних страницах
        private void turnButton()
        {
            if (startPage == 0) { back.IsEnabled = false; }
            else { back.IsEnabled = true; };
            if ((startPage + 1) * recordsPerPage >= clientCount) { forward.IsEnabled = false; }
            else { forward.IsEnabled = true; };
        }

        //Изменение цвета кнопки активной страницы
        private void changeButtonColor()
        {
            foreach (Button but in pagin.Children)
            {
                if (startPage == Convert.ToInt32(but.Tag.ToString()))
                {
                    but.Background = new SolidColorBrush(Color.FromArgb(200, 255, 156, 26)); ;
                }
            }
        }

        private void changeClientBtn_Click(object sender, RoutedEventArgs e)
        {
            Client selectedClient = clientGrid.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для изменения");
                return;
            }
            ClientForm dlg = new ClientForm(this, selectedClient);
            dlg.ShowDialog();
        }

        private void addClientBtn_Click(object sender, RoutedEventArgs e)
        {
            ClientForm dlg = new ClientForm(this, null);
            dlg.ShowDialog();
        }
        private void deleteClientBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, выбран ли клиент
            Client selectedClient = clientGrid.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для удаления");
                return;
            }

            // Проверяем, есть ли у клиента посещения
            if (selectedClient.VisitCount > 0)
            {
                MessageBox.Show("Невозможно удалить клиента с посещениями");
                return;
            }

            // Подтверждение удаления
            MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить {selectedClient.FirstName} {selectedClient.LastName}?", "Подтверждение", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                // Удаляем клиента из базы данных
                DeleteClientFromDatabase(selectedClient);

                // Обновляем список клиентов
                LoadClient();
                displayClient();
            }
        }
        public void RefreshClientList()
        {
            LoadClient();
            displayClient();
        }
        private void DeleteClientFromDatabase(Client client)
        {
            try
            {
                foreach (var tag in client.Tag.ToList())
                {
                    client.Tag.Remove(tag);
                }
                helper.GetContext().Client.Remove(client);
                helper.GetContext().SaveChanges();
                MessageBox.Show("Удаление информации об клиенте завешено!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting client: {ex.Message}");
            }
        }

        private void clientGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Проверяем, выбран ли клиент
            Client selectedClient = clientGrid.SelectedItem as Client;
            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для просмотра посещений");
                return;
            }
            if (selectedClient.VisitCount == 0)
            {
                MessageBox.Show("Выбраный клиент не  имеет посещений", ":(");
                return;
            }
            NavigationService.Navigate(new VisitClient(selectedClient));

        }
    }
}
