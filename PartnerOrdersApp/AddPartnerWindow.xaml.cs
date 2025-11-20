using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Data.Entity.Validation;
using System.Text;

namespace PartnerOrdersApp
{
    public partial class AddPartnerWindow : Window
    {
        PartnerOrders partnerOrders = new PartnerOrders();

        public AddPartnerWindow()
        {
            InitializeComponent();
            LoadPartnerTypes();
        }

        // Загрузка типов партнеров в выпадающий список
        private void LoadPartnerTypes()
        {
            try
            {
                var partnerTypes = partnerOrders.PartnerTypes.ToList();
                typeComboBox.ItemsSource = partnerTypes;

                if (partnerTypes.Any())
                {
                    typeComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов партнеров: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Валидация ввода для рейтинга (только цифры)
        private void RatingTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Валидация ввода для ИНН (только цифры)
        private void InnTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        // Сохранение партнера
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация обязательных полей
                if (string.IsNullOrWhiteSpace(nameTextBox.Text))
                {
                    ShowValidationError("Наименование компании является обязательным полем", nameTextBox);
                    return;
                }

                if (typeComboBox.SelectedItem == null)
                {
                    ShowValidationError("Необходимо выбрать тип партнера", typeComboBox);
                    return;
                }

                if (string.IsNullOrWhiteSpace(innTextBox.Text) || innTextBox.Text.Length != 10)
                {
                    ShowValidationError("ИНН должен содержать ровно 10 цифр", innTextBox);
                    return;
                }

                if (string.IsNullOrWhiteSpace(ratingTextBox.Text))
                {
                    ShowValidationError("Рейтинг является обязательным полем", ratingTextBox);
                    return;
                }

                if (!int.TryParse(ratingTextBox.Text, out int rating) || rating < 0 || rating > 10)
                {
                    ShowValidationError("Рейтинг должен быть целым числом от 0 до 10", ratingTextBox);
                    return;
                }

                // Создание нового партнера
                var newPartner = new Partners
                {
                    PartnerTypeID = ((PartnerTypes)typeComboBox.SelectedItem).PartnerTypeID,
                    PartnerName = nameTextBox.Text.Trim(),
                    Director = string.IsNullOrWhiteSpace(directorTextBox.Text) ? null : directorTextBox.Text.Trim(),
                    Email = string.IsNullOrWhiteSpace(emailTextBox.Text) ? null : emailTextBox.Text.Trim(),
                    Phone = string.IsNullOrWhiteSpace(phoneTextBox.Text) ? null : phoneTextBox.Text.Trim(),
                    LegalAddress = string.IsNullOrWhiteSpace(addressTextBox.Text) ? null : addressTextBox.Text.Trim(),
                    INN = innTextBox.Text.Trim(),
                    Rating = rating
                };

                // Сохранение в базу данных
                partnerOrders.Partners.Add(newPartner);
                partnerOrders.SaveChanges();

                MessageBox.Show("Партнер успешно добавлен в систему!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                this.Close();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = new StringBuilder();
                errorMessages.AppendLine("Обнаружены ошибки валидации:");

                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        errorMessages.AppendLine($"• {validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }

                MessageBox.Show(errorMessages.ToString(),
                    "Ошибка валидации данных", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при сохранении:\n{ex.Message}",
                    "Ошибка сохранения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Вспомогательный метод для показа ошибок валидации
        private void ShowValidationError(string message, FrameworkElement element)
        {
            MessageBox.Show(message, "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            element.Focus();
        }

        // Отмена добавления партнера
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Все несохраненные данные будут потеряны. Продолжить?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                this.Close();
            }
        }
    }
}