using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PartnerOrdersApp
{
    public partial class MainWindow : Window
    {
        PartnerOrders partnerOrders = new PartnerOrders();
        Orders order = new Orders();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var orders = partnerOrders.Orders.ToList();

                // Расчет стоимости заявки для каждого заказа
                foreach (var order in orders)
                {
                    order.CalculateOrderTotal();
                }
                ordersList.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddOrderWindow add = new AddOrderWindow();
            add.Show();
            
        }

        //Обработчик кнопки "Добавить партнера"
        private void btnAddPartner_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddPartnerWindow addPartner = new AddPartnerWindow();
                addPartner.ShowDialog(); 
               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка открытия формы партнера: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnMethod_Click(object sender, RoutedEventArgs e)
        {
            PartnerProductsWindow partnerProducts = new PartnerProductsWindow();
            partnerProducts.Show();
        }
    }

    public partial class Orders
    {
        // Стоимость заявки
        public decimal OrderTotal { get; set; }

        public void CalculateOrderTotal()
        {
            if (OrderItems == null || !OrderItems.Any())
            {
                OrderTotal = 0.00m;
                return;
            }

            decimal total = 0;

            foreach (var orderItem in OrderItems)
            {
                if (orderItem.Quantity > 0 && orderItem.UnitPrice >= 0)
                {
                    total += orderItem.Quantity * orderItem.UnitPrice;
                }
            }

            // Округление до сотых
            OrderTotal = Math.Round(total, 2);

            // Гарантия, что стоимость не отрицательная
            if (OrderTotal < 0)
            {
                OrderTotal = 0.00m;
            }
        }
    }
}