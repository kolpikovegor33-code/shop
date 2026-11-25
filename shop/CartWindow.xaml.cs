using shop.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace shop
{
    public partial class CartWindow : Window
    {
        UsersImport user;
        bool staffMode;

        public CartWindow(UsersImport user, bool staffMode)
        {
            InitializeComponent();

            this.user = user;
            this.staffMode = staffMode;

            if (staffMode)
            {
                AppDbContext db = new AppDbContext();
                var clients = db.UsersImports.Where(u => u.RoleId == 1).OrderBy(u => u.LastName).ToList();

                foreach (var u in clients)
                {
                    ClientOption option = new ClientOption();
                    option.UserId = Convert.ToInt32(u.UserId);
                    option.FullName = u.LastName + " " + u.Name + " " + u.Surname;
                    cbClient.Items.Add(option);
                }

                if (cbClient.Items.Count > 0)
                {
                    cbClient.SelectedIndex = 0;
                }

                dpDate.SelectedDate = DateTime.Now;
            }
            else
            {
                staffPanel.Visibility = Visibility.Collapsed;
                btnAddProduct.Visibility = Visibility.Collapsed;
            }

            ShowCart();
        }

        void ShowCart()
        {
            lbCart.Items.Clear();
            decimal total = 0;

            foreach (var l in MainWindow.Cart)
            {
                lbCart.Items.Add(l.Text);
                total += l.Price * l.Quantity;
            }

            txtTotal.Text = "Итого: " + total.ToString("F2") + " руб.";
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            MainWindow catalog = new MainWindow(user, true);
            catalog.Owner = this;
            catalog.ShowDialog();

            ShowCart();
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lbCart.SelectedIndex < 0)
            {
                MessageBox.Show("Выберите позицию в заказе");
                return;
            }

            MainWindow.Cart.RemoveAt(lbCart.SelectedIndex);
            ShowCart();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Cart.Count == 0)
            {
                MessageBox.Show("В заказе нет товаров");
                return;
            }

            int userId;
            DateOnly date;

            if (staffMode)
            {
                ClientOption client = cbClient.SelectedItem as ClientOption;
                if (client == null)
                {
                    MessageBox.Show("Выберите клиента");
                    return;
                }

                if (dpDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату заказа");
                    return;
                }

                userId = client.UserId;
                date = DateOnly.FromDateTime(dpDate.SelectedDate.Value);
            }
            else
            {
                userId = Convert.ToInt32(user.UserId);
                date = DateOnly.FromDateTime(DateTime.Now);
            }

            AppDbContext db = new AppDbContext();

            foreach (var l in MainWindow.Cart)
            {
                StockItemsImport st = db.StockItemsImports.FirstOrDefault(s => s.ProductId == l.ProductId && s.SizeId == l.SizeId);
                if (st == null || st.QuantityForOrders < l.Quantity)
                {
                    MessageBox.Show("Товара «" + l.ProductName + "», размер " + l.SizeName + ", недостаточно на складе");
                    return;
                }
            }

            int orderNumber = (db.OrdersImports.Max(o => (int?)o.OrderNumber) ?? 0) + 1;
            int itemId = (db.OrderItemsImports.Max(o => (int?)o.OrderItemId) ?? 0) + 1;

            OrdersImport order = new OrdersImport();
            order.OrderNumber = orderNumber;
            order.OrderDate = date;
            order.UserId = userId;
            db.OrdersImports.Add(order);

            foreach (var l in MainWindow.Cart)
            {
                OrderItemsImport oi = new OrderItemsImport();
                oi.OrderItemId = itemId;
                itemId++;
                oi.OrderNumber = orderNumber;
                oi.ProductId = l.ProductId;
                oi.SizeId = l.SizeId;
                oi.Quantity = l.Quantity;
                oi.Price = l.Price;
                db.OrderItemsImports.Add(oi);

                StockItemsImport st = db.StockItemsImports.First(s => s.ProductId == l.ProductId && s.SizeId == l.SizeId);
                st.QuantityForOrders -= l.Quantity;
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
                return;
            }

            MainWindow.Cart.Clear();
            MessageBox.Show("Заказ оформлен. Номер заказа: " + orderNumber);
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Cart.Clear();
            this.Close();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (staffMode)
            {
                MainWindow.Cart.Clear();
            }
            base.OnClosed(e);
        }
    }

    public class ClientOption
    {
        public int UserId { get; set; }
        public string FullName { get; set; }

        public override string ToString()
        {
            return FullName;
        }
    }
}