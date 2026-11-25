using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using shop.Models;

namespace shop
{
    public partial class OrdersWindow : Window
    {
        UsersImport currentUser;
        bool isAdmin;
        bool isClient;

        public OrdersWindow(UsersImport currentUser)
        {
            InitializeComponent();

            this.currentUser = currentUser;

            int role = Convert.ToInt32(currentUser.RoleId);
            isAdmin = role == 3;
            isClient = role == 1;

            if (isClient)
            {
                Title = "Мои заказы";
                txtTitle.Text = "Мои заказы";
                btnAddOrder.Visibility = Visibility.Collapsed;
                btnDeleteOrder.Visibility = Visibility.Collapsed;
            }

            LoadOrders();
        }

        void LoadOrders()
        {
            AppDbContext db = new AppDbContext();
            var orders = db.OrdersImports.ToList();
            var users = db.UsersImports.ToList();
            var items = db.OrderItemsImports.ToList();

            int myId = Convert.ToInt32(currentUser.UserId);

            var rows = new List<OrderRow>();

            foreach (var o in orders)
            {
                if (isClient && Convert.ToInt32(o.UserId) != myId)
                {
                    continue;
                }

                string client = "";
                foreach (var u in users)
                {
                    if (u.UserId == o.UserId)
                    {
                        client = u.LastName + " " + u.Name + " " + u.Surname;
                    }
                }

                decimal total = 0;
                foreach (var i in items)
                {
                    if (i.OrderNumber == o.OrderNumber)
                    {
                        total += Convert.ToDecimal(i.Price) * Convert.ToInt32(i.Quantity);
                    }
                }

                OrderRow row = new OrderRow();
                row.OrderNumber = Convert.ToInt32(o.OrderNumber);
                row.DateText = string.Format("{0:dd.MM.yyyy}", o.OrderDate);
                row.Client = client;
                row.TotalText = total.ToString("F2") + " руб.";
                rows.Add(row);
            }

            dgOrders.ItemsSource = rows.OrderByDescending(r => r.OrderNumber).ToList();
        }

        public static void ReturnToStock(AppDbContext db, OrderItemsImport item)
        {
            StockItemsImport st = db.StockItemsImports.FirstOrDefault(s => s.ProductId == item.ProductId && s.SizeId == item.SizeId);
            if (st != null)
            {
                st.QuantityForOrders += Convert.ToInt32(item.Quantity);
            }
        }

        private void btnDetails_Click(object sender, RoutedEventArgs e)
        {
            OrderRow row = dgOrders.SelectedItem as OrderRow;
            if (row == null)
            {
                MessageBox.Show("Выберите заказ в списке");
                return;
            }

            OrderDetailsWindow window = new OrderDetailsWindow(row.OrderNumber, isAdmin);
            window.Owner = this;
            window.ShowDialog();

            LoadOrders();
        }

        private void btnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Cart.Clear();

            CartWindow window = new CartWindow(currentUser, true);
            window.Owner = this;
            window.ShowDialog();

            LoadOrders();
        }

        private void btnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            OrderRow row = dgOrders.SelectedItem as OrderRow;
            if (row == null)
            {
                MessageBox.Show("Выберите заказ в списке");
                return;
            }

            MessageBoxResult answer = MessageBox.Show("Удалить заказ № " + row.OrderNumber + "?", "Удаление", MessageBoxButton.YesNo);
            if (answer != MessageBoxResult.Yes)
            {
                return;
            }

            int number = row.OrderNumber;
            AppDbContext db = new AppDbContext();

            try
            {
                using (var tx = db.Database.BeginTransaction())
                {
                    var orderItems = db.OrderItemsImports.Where(i => i.OrderNumber == number).ToList();
                    foreach (var i in orderItems)
                    {
                        ReturnToStock(db, i);
                    }
                    db.SaveChanges();

                    db.Database.ExecuteSqlRaw("DELETE FROM [Order_Items_import$] WHERE OrderNumber = {0}", number);
                    db.Database.ExecuteSqlRaw("DELETE FROM [Orders_import$] WHERE OrderNumber = {0}", number);

                    tx.Commit();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return;
            }

            LoadOrders();
        }
    }

    public class OrderRow
    {
        public int OrderNumber { get; set; }
        public string DateText { get; set; }
        public string Client { get; set; }
        public string TotalText { get; set; }
    }
}