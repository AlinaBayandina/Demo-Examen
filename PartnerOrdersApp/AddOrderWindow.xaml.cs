using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;

namespace PartnerOrdersApp
{
    public partial class AddOrderWindow : Window
    {
        PartnerOrders partnerOrders = new PartnerOrders();
        private List<ProductViewModel> products;

        public AddOrderWindow()
        {
            InitializeComponent();
            LoadPartners();
            LoadProducts();
        }

        // Метод для загрузки партнеров
        private void LoadPartners()
        {
            try
            {
                var partners = partnerOrders.Partners.ToList();
                partnersComboBox.ItemsSource = partners;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки партнеров: {ex.Message}");
            }
        }

        // Метод для загрузки продукции
        private void LoadProducts()
        {
            try
            {
                products = partnerOrders.Products.ToList()
                    .Select(p => new ProductViewModel
                    {
                        ProductID = p.ProductID,
                        ProductName = p.ProductName,
                        Article = p.Article,
                        MinPartnerPrice = p.MinPartnerPrice,
                        Quantity = 0,
                        IsSelected = false
                    }).ToList();

                productsDataGrid.ItemsSource = products;

                // Подписываемся на изменения для пересчета суммы
                foreach (var product in products)
                {
                    product.PropertyChanged += Product_PropertyChanged;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки продукции: {ex.Message}");
            }
        }

        // Обработчик изменений в продукции для пересчета суммы
        private void Product_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductViewModel.IsSelected) ||
                e.PropertyName == nameof(ProductViewModel.Quantity))
            {
                UpdateTotalAmount();
            }
        }

        // Метод для обновления отображения общей суммы
        private void UpdateTotalAmount()
        {
            decimal totalAmount = CalculateTotalAmount();
            totalAmountText.Text = $"Общая стоимость: {totalAmount:N2} р";
        }

        // Метод для расчета общей стоимости заказа
        private decimal CalculateTotalAmount()
        {
            decimal totalAmount = 0;

            foreach (var product in products)
            {
                if (product.IsSelected && product.Quantity > 0)
                {
                    totalAmount += product.Quantity * product.MinPartnerPrice;
                }
            }

            return Math.Round(totalAmount, 2);
        }

        // Сохранение заявки
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (partnersComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите партнера");
                return;
            }

            var selectedProducts = products.Where(p => p.IsSelected && p.Quantity > 0).ToList();
            if (!selectedProducts.Any())
            {
                MessageBox.Show("Выберите хотя бы один продукт с количеством больше 0");
                return;
            }

            Partners selectedPartner = partnersComboBox.SelectedItem as Partners;

            try
            {
                // Создаем новый заказ
                var newOrder = new Orders
                {
                    PartnerID = selectedPartner.PartnerID,
                    OrderDate = DateTime.Now,
                    Status = "Новая",
                    PrepaymentReceived = false
                };

                partnerOrders.Orders.Add(newOrder);
                partnerOrders.SaveChanges();

                // Добавление продукции в заказ
                foreach (var product in selectedProducts)
                {
                    var orderItem = new OrderItems
                    {
                        OrderID = newOrder.OrderID,
                        ProductID = product.ProductID,
                        Quantity = product.Quantity,
                        UnitPrice = product.MinPartnerPrice
                    };

                    partnerOrders.OrderItems.Add(orderItem);
                }

                partnerOrders.SaveChanges();

                MessageBox.Show("Заявка успешно создана");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        // Отмена добавления заявки
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Класс для представления продукта с уведомлениями об изменении
    public class ProductViewModel : INotifyPropertyChanged
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string Article { get; set; }
        public decimal MinPartnerPrice { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}