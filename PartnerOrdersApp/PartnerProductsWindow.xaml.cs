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
using System.Windows.Shapes;

namespace PartnerOrdersApp
{
    /// <summary>
    /// Логика взаимодействия для PartnerProductsWindow.xaml
    /// </summary>
    public partial class PartnerProductsWindow : Window
    {
        PartnerOrders partnerOrders = new PartnerOrders();
      
        public PartnerProductsWindow()
        {
            InitializeComponent();
         
            LoadData();
        }
        private void LoadData()
        {
            try
            {
                // Загрузка типов продукции
                var productTypes = partnerOrders.ProductTypes.ToList();
                comboProductType.ItemsSource = productTypes;

                // Загрузка типов материалов
                var materialTypes = partnerOrders.MaterialTypes.ToList();
                comboMaterialType.ItemsSource = materialTypes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Расчет материалов
        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация введенных данных
                if (!ValidateInput())
                    return;

                // Получение параметров
                int productTypeId = ((ProductTypes)comboProductType.SelectedItem).ProductTypeID;
                int materialTypeId = ((MaterialTypes)comboMaterialType.SelectedItem).MaterialTypeID;
                double parameter1 = double.Parse(txtParameter1.Text);
                double parameter2 = double.Parse(txtParameter2.Text);
                int requiredQuantity = int.Parse(txtRequiredQuantity.Text);
                int stockQuantity = int.Parse(txtStockQuantity.Text);

                // Вызов метода расчета
                int result = CalculateRequiredMaterial(
                    productTypeId, materialTypeId, requiredQuantity, stockQuantity, parameter1, parameter2);

                // Отображение результата
                if (result == -1)
                {
                    txtResult.Text = " Ошибка в расчетах. Проверьте правильность введенных данных:\n" +
                                   "• Убедитесь, что все параметры положительные\n" +
                                   "• Проверьте существование выбранных типов\n" +
                                   "• Количество на складе не может быть отрицательным";
                    txtResult.Foreground = System.Windows.Media.Brushes.Red;
                }
                else
                {
                    txtResult.Text = $" Расчет выполнен успешно!\n\n" +
                                   $"Необходимое количество материала: {result} единиц\n\n";
                    txtResult.Foreground = System.Windows.Media.Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при расчете: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Валидация введенных данных
        private bool ValidateInput()
        {
            if (comboProductType.SelectedItem == null)
            {
                ShowValidationError("Выберите тип продукции", comboProductType);
                return false;
            }

            if (comboMaterialType.SelectedItem == null)
            {
                ShowValidationError("Выберите тип материала", comboMaterialType);
                return false;
            }

            if (!double.TryParse(txtParameter1.Text, out double param1) || param1 <= 0)
            {
                ShowValidationError("Параметр 1 должен быть положительным числом", txtParameter1);
                return false;
            }

            if (!double.TryParse(txtParameter2.Text, out double param2) || param2 <= 0)
            {
                ShowValidationError("Параметр 2 должен быть положительным числом", txtParameter2);
                return false;
            }

            if (!int.TryParse(txtRequiredQuantity.Text, out int reqQty) || reqQty <= 0)
            {
                ShowValidationError("Требуемое количество должно быть целым положительным числом", txtRequiredQuantity);
                return false;
            }

            if (!int.TryParse(txtStockQuantity.Text, out int stockQty) || stockQty < 0)
            {
                ShowValidationError("Количество на складе должно быть целым неотрицательным числом", txtStockQuantity);
                return false;
            }

            return true;
        }

        // Метод расчета необходимого количества материала
        public int CalculateRequiredMaterial(int productTypeId, int materialTypeId, int requiredQuantity,
                                           int stockQuantity, double parameter1, double parameter2)
        {
            try
            {
                // Проверка существования типов продукции и материалов
                var productType = partnerOrders.ProductTypes.FirstOrDefault(pt => pt.ProductTypeID == productTypeId);
                var materialType = partnerOrders.MaterialTypes.FirstOrDefault(mt => mt.MaterialTypeID == materialTypeId);

                if (productType == null || materialType == null ||
                    parameter1 <= 0 || parameter2 <= 0 ||
                    requiredQuantity <= 0 || stockQuantity < 0)
                {
                    return -1;
                }

                // Расчет количества продукции, которое нужно произвести
                int productionQuantity = Math.Max(0, requiredQuantity - stockQuantity);

                if (productionQuantity == 0)
                {
                    return 0; 
                }

                // Количество материала на одну единицу продукции
                double materialPerUnit = parameter1 * parameter2 * (double)productType.TypeCoefficient;

                // Учет брака материала
                double materialWithDefect = materialPerUnit * (1 + (double)materialType.DefectPercentage / 100);

                // Общее необходимое количество материала
                double totalMaterialNeeded = materialWithDefect * productionQuantity;

                // Округление до целого числа в большую сторону
                int finalMaterialQuantity = (int)Math.Ceiling(totalMaterialNeeded);

                return finalMaterialQuantity;
            }
            catch
            {
                return -1;
            }
        }

        private void ShowValidationError(string message, Control control)
        {
            MessageBox.Show(message, "Проверка данных", MessageBoxButton.OK, MessageBoxImage.Warning);
            control.Focus();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

