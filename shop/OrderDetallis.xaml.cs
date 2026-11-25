using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using shop.Models;

namespace shop
{
    public partial class OrderDetailsWindow : Window
    {
        int orderNumber;

        public OrderDetailsWindow(int orderNumber, bool isAdmin)
        {
            InitializeComponent();

            this.orderNumber = orderNumber;

            if (isAdmin == false)
            {
                adminPanel.Visibility = Visibility.Collapsed;
            }

            LoadItems();
        }

        void LoadItems()
        {
            AppDbContext db = new AppDbContext();
            var products = db.ProductsImports.ToList();
            var manufacturers = db.Manufacturers.ToList();
            var sizes = db.SizesImports.ToList();
            var users = db.UsersImports.ToList();
            var orderItems = db.OrderItemsImports.Where(i => i.OrderNumber == orderNumber).ToList();
            OrdersImport order = db.OrdersImports.FirstOrDefault(o => o.OrderNumber == orderNumber);

            string client = "";
            string dateText = "";
            if (order != null)
            {
                foreach (var u in users)
                {
                    if (u.UserId == order.UserId)
                    {
                        client = u.LastName + " " + u.Name + " " + u.Surname;
                    }
                }
                dateText = string.Format("{0:dd.MM.yyyy}", order.OrderDate);
            }

            txtHeader.Text = "Заказ № " + orderNumber + ", " + client + ", " + dateText;

            DateTime parsed;
            if (DateTime.TryParseExact(dateText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                dpDate.SelectedDate = parsed;
            }

            var rows = new List<OrderItemRow>();
            decimal total = 0;

            foreach (var i in orderItems)
            {
                string productName = "";
                string manufacturer = "";

                foreach (var p in products)
                {
                    if (p.ProductId == i.ProductId)
                    {
                        productName = p.ProductName;

                        foreach (var m in manufacturers)
                        {
                            if (m.ManufacturerId == p.ManufacturerId)
                            {
                                manufacturer = m.Manufacturer1;
                            }
                        }
                    }
                }

                string sizeName = "";
                foreach (var z in sizes)
                {
                    if (z.SizeId == i.SizeId)
                    {
                        sizeName = z.SizeName.ToString();
                    }
                }

                int quantity = Convert.ToInt32(i.Quantity);
                decimal price = Convert.ToDecimal(i.Price);
                decimal sum = price * quantity;
                total += sum;

                OrderItemRow row = new OrderItemRow();
                row.ItemId = Convert.ToInt32(i.OrderItemId);
                row.ProductText = manufacturer + " | " + productName;
                row.SizeName = sizeName;
                row.Quantity = quantity;
                row.PriceText = price.ToString("F2") + " руб.";
                row.SumText = sum.ToString("F2") + " руб.";
                rows.Add(row);
            }

            dgItems.ItemsSource = rows;
            txtTotal.Text = "Итого по заказу: " + total.ToString("F2") + " руб.";
        }

        private void btnSaveDate_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату");
                return;
            }

            AppDbContext db = new AppDbContext();
            OrdersImport order = db.OrdersImports.FirstOrDefault(o => o.OrderNumber == orderNumber);
            if (order == null)
            {
                return;
            }

            order.OrderDate = DateOnly.FromDateTime(dpDate.SelectedDate.Value);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return;
            }

            LoadItems();
        }

        private void btnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            OrderItemRow row = dgItems.SelectedItem as OrderItemRow;
            if (row == null)
            {
                MessageBox.Show("Выберите позицию в составе заказа");
                return;
            }

            MessageBoxResult answer = MessageBox.Show("Удалить позицию из заказа?", "Удаление", MessageBoxButton.YesNo);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            AppDbContext db = new AppDbContext();
            OrderItemsImport item = db.OrderItemsImports.FirstOrDefault(i => i.OrderItemId == row.ItemId);
            if (item == null)
            {
                return;
            }

            OrdersWindow.ReturnToStock(db, item);
            db.OrderItemsImports.Remove(item);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return;
            }

            LoadItems();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class OrderItemRow
    {
        public int ItemId { get; set; }
        public string ProductText { get; set; }
        public string SizeName { get; set; }
        public int Quantity { get; set; }
        public string PriceText { get; set; }
        public string SumText { get; set; }
    }
}