using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using WPFAutoService.Models;
using static WPFAutoService.MainWindow;

namespace WPFAutoService.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientForm.xaml
    /// </summary>
    public partial class ClientForm : Window
    {
        private Client _client; 
        private ClientGRUD _clientGRUD;
        private string newFilePath = "";
        private Tag _addTag;
        private List<Tag> tags = new List<Tag> { };


        private ICollection<Tag> _tags;
        public ClientForm(ClientGRUD clientGRUD, Client client)
        {
            _client = client;
            _clientGRUD = clientGRUD;
            InitializeComponent();
            tags = helper.GetContext().Tag.ToList();
            TagComboBox.ItemsSource = tags;

            if (_client != null)
            {
                IDTextBlock.Visibility = Visibility.Visible;
                IDTextBox.Visibility = Visibility.Visible;
                _tags = _client.Tag;
                LoadClient();
            }
            
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.jpeg;*.png|All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var fileInfo = new FileInfo(openFileDialog.FileName);
                if (fileInfo.Length > 2 * 1024 * 1024) // 2 MB limit
                {
                    MessageBox.Show("Размер файла не должен превышать 2 МБ.");
                    return;
                }
                PhotoPreview.Source = new BitmapImage(new Uri(openFileDialog.FileName));
                 
                string filePath = openFileDialog.FileName;
                string fileName = System.IO.Path.GetFileName(filePath);
                newFilePath = $"Клиенты\\{fileName}";
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                if (_client == null)
                {
                    _client = new Client();
                    _client.RegistrationDate = DateTime.Now; // Устанавливаем дату регистрации для нового клиента
                }

                _client.FirstName = FirstNameTextBox.Text;
                _client.LastName = LastNameTextBox.Text;
                _client.Patronymic = PatronymicTextBox.Text;
                _client.Email = EmailTextBox.Text;
                _client.Phone = PhoneTextBox.Text;
                _client.Birthday = BirthDatePicker.SelectedDate.Value;
                _client.GenderCode = MaleRadioButton.IsChecked == true ? "м" : "ж";

                // Сохранение пути к фотографии, если изображение выбрано
                if (PhotoPreview.Source is BitmapImage image)
                {
                    _client.PhotoPath = SavePhotoToDisk(image);
                }

                try
                {
                    if (_client.ID > 0)
                    {
                        helper.GetContext().Entry(_client).State = EntityState.Modified;
                        helper.GetContext().SaveChanges();
                        MessageBox.Show("Обновление информации об агенте завершено");
                    }
                    else
                    {
                        helper.ent.Client.Add(_client);
                        helper.ent.SaveChanges();
                        MessageBox.Show("Добавление информации об агенте завершено");
                    }
                }
                catch { };

                _clientGRUD.RefreshClientList(); // Обновляем список в главном окне
                DialogResult = true;
                Close();
            }
        }
        private void LoadClient()
        {
            try
            {
                IDTextBox.Text = _client.ID.ToString();
                FirstNameTextBox.Text = _client.FirstName;
                LastNameTextBox.Text = _client.LastName;
                PatronymicTextBox.Text = _client.Patronymic;
                EmailTextBox.Text = _client.Email;
                PhoneTextBox.Text = _client.Phone;
                BirthDatePicker.SelectedDate = _client.Birthday;
                TagsListBox.ItemsSource = _tags.Select(t => t.Title);

                if (_client.GenderCode == "м")
                    MaleRadioButton.IsChecked = true;
                else
                    FemaleRadioButton.IsChecked = true;

                // Загрузка фотографии из папки images
                if (!string.IsNullOrEmpty(_client.PhotoPath))
                {
                    try
                    {
                        string imagePath = System.IO.Path.Combine("..\\Images", _client.PhotoPath);
                        newFilePath = imagePath;
                        PhotoPreview.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                    }
                    catch (UriFormatException ex)
                    {
                        MessageBox.Show("Ошибка в пути к изображению: " + ex.Message);
                    }
                }
            }
            catch (UriFormatException ex)
            {
                MessageBox.Show("Ошибка в пути к изображению: " + ex.Message);
            }

        }
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                string.IsNullOrWhiteSpace(PhoneTextBox.Text) ||
                string.IsNullOrWhiteSpace(BirthDatePicker.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return false;
            }

            // Проверка email
            if (!Regex.IsMatch(EmailTextBox.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Неверный формат email.");
                return false;
            }

            // Проверка телефона
            if (!Regex.IsMatch(PhoneTextBox.Text, @"^[\d\s\+\-\(\)]+$") || !(PhoneTextBox.Text.Length > 10))
            {
                MessageBox.Show("Телефон может содержать только цифры и символы: +, -, (, ), пробел. И не менее 11 символов");
                return false;
            }

            if (!Regex.IsMatch(FirstNameTextBox.Text, @"^[а-яА-Яa-zA-Z\s-]{1,50}$") ||
                !Regex.IsMatch(LastNameTextBox.Text, @"^[а-яА-Яa-zA-Z\s-]{1,50}$") ||
                !Regex.IsMatch(PatronymicTextBox.Text, @"^[а-яА-Яa-zA-Z\s-]{1,50}$"))
            {
                MessageBox.Show("Поле ФИО может содержать только буквы, пробел и дефис, и не может превышать 50 символов.");
                return false;
            }
            if (MaleRadioButton.IsChecked == false && FemaleRadioButton.IsChecked == false)
            {
                MessageBox.Show("Пол не может быть пустым.");
                return false;
            }

            return true;
        }
        private string SavePhotoToDisk(BitmapImage image)
        {
            string directoryPath = System.IO.Path.Combine("..\\Images");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = System.IO.Path.Combine(directoryPath, newFilePath);

          

            return newFilePath; // Возвращаем имя файла для сохранения в базе данных
        }
        private void TagComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _addTag = ((Tag)TagComboBox.SelectedItem);
        }
        private void AddTagButton_Click(object sender, RoutedEventArgs e)
        {
            if(!(_client == null))
            {
                if (TagComboBox.SelectedItem != null)
                {
                    if (_client.Tag.Any(t => t.Title == _addTag.Title))
                        MessageBox.Show("Такой тег у клиента уже есть.");
                    else
                    {
                        try
                        {
                            _client.Tag.Add(_addTag); // Добавляем новый тег
                            UpdateTagsListBox(); // Обновляем отображение тегов
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting tag: {ex.Message}");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Выберите тег из списка.");
                }
            }
            else
                MessageBox.Show("клиент еще не создан, создайте клиента и повторите");
        }
        private void RemoveTagButton_Click(object sender, RoutedEventArgs e)
        {
            if (TagsListBox.SelectedItem != null)
            {
                try
                {
                    var selectedTagTitle = (string)TagsListBox.SelectedItem;
                    if (_client.Tag.Any(t => t.Title == selectedTagTitle))
                    {
                        var tagToRemove = _client.Tag.FirstOrDefault(t => t.Title == selectedTagTitle);
                        // Подтверждение удаления
                        MessageBoxResult result = MessageBox.Show($"Вы уверены, что хотите удалить тег {tagToRemove.Title} у клиента {_client.FirstName} {_client.LastName}?", "Подтверждение", MessageBoxButton.YesNo);
                        if (result == MessageBoxResult.Yes)
                        {
                            _client.Tag.Remove(tagToRemove);
                            helper.GetContext().SaveChanges();
                            UpdateTagsListBox(); // Обновляем отображение тегов
                            MessageBox.Show("Удаление информации об теге завешено!", "Успешно");
                        }
                        else
                            MessageBox.Show("вы передумали");
                        
                        
                    }
                    else
                    {
                        MessageBox.Show("Тег не найден", "Ошибка");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting tag: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Выберите тег для удаления.", "Предупреждение");
            }
        }
        private void UpdateTagsListBox()
        {
            TagsListBox.ItemsSource = null; // Обнуляем источник данных
            TagsListBox.ItemsSource = _client.Tag.Select(t => t.Title).ToList();
        }
    }
}
