using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using shop.Models;

namespace shop
{
    public partial class MainWindow : Window
    {
        public static List<CartLine> Cart = new List<CartLine>();

        Brush red = new SolidColorBrush(Color.FromRgb(255, 128, 128));
        List<ProductItem> items = new List<ProductItem>();
        UsersImport currentUser;
        int role = 0;
        bool pickMode;
        bool ready = false;

        public MainWindow(UsersImport currentUser, bool pickMode = false)
        {
            InitializeComponent();

            this.currentUser = currentUser;
            this.pickMode = pickMode;

            btnCart.Visibility = Visibility.Collapsed;
            btnOrders.Visibility = Visibility.Collapsed;

            if (currentUser != null)
            {
                role = Convert.ToInt32(currentUser.RoleId);
            }

            if (role == 0)
            {
                user.Text = "Гость";
                cbSort.Visibility = Visibility.Collapsed;
                cbFilter.Visibility = Visibility.Collapsed;
                searchPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                string roleName = "Клиент";
                if (role == 2)
                {
                    roleName = "Менеджер";
                }
                else if (role == 3)
                {
                    roleName = "Администратор";
                }

                user.Text = currentUser.LastName + " " + currentUser.Name + " " + currentUser.Surname + " (" + roleName + ")";

                if (pickMode)
                {
                    btnDone.Visibility = Visibility.Visible;
                }
                else if (role == 1)
                {
                    btnCart.Visibility = Visibility.Visible;
                    btnOrders.Content = "Мои заказы";
                    btnOrders.Visibility = Visibility.Visible;
                }
                else
                {
                    btnOrders.Content = "Заказы";
                    btnOrders.Visibility = Visibility.Visible;
                }
            }

            LoadData();
            UpdateCartButton();
        }

        void UpdateCartButton()
        {
            btnCart.Content = "Корзина (" + Cart.Count + ")";
        }

        void LoadData()
        {
            AppDbContext db = new AppDbContext();
            items.Clear();

            var allProducts = db.ProductsImports.ToList();
            var allManufacturers = db.Manufacturers.ToList();
            var allCategories = db.ProductCategories.ToList();
            var allStock = db.StockItemsImports.ToList();
            var orders = db.OrdersImports.ToList();
            var orderItems = db.OrderItemsImports.ToList();

            DateTime now = DateTime.Now;
            DateOnly start = new DateOnly(now.Year, now.Month, 1).AddMonths(-1);
            DateOnly end = new DateOnly(now.Year, now.Month, 1);

            for (int i = 0; i < allProducts.Count; i++)
            {
                var p = allProducts[i];
                var item = new ProductItem();

                string manufacturer = "";
                foreach (var m in allManufacturers)
                {
                    if (m.ManufacturerId == p.ManufacturerId)
                    {
                        manufacturer = m.Manufacturer1;
                    }
                }

                string category = "";
                foreach (var c in allCategories)
                {
                    if (c.CategoryId == p.CategoryId)
                    {
                        category = c.CategoryName;
                    }
                }

                double count = 0;
                foreach (var s in allStock)
                {
                    if (s.ProductId == p.ProductId)
                    {
                        count += s.QuantityForOrders;
                    }
                }

                bool hasOrder = false;
                foreach (var oi in orderItems)
                {
                    if (oi.ProductId == p.ProductId)
                    {
                        foreach (var o in orders)
                        {
                            if (o.OrderNumber == oi.OrderNumber && o.OrderDate >= start && o.OrderDate < end)
                            {
                                hasOrder = true;
                            }
                        }
                    }
                }

                decimal price = p.Price;

                if (hasOrder == false)
                {
                    price = price * 0.75m;
                }

                item.ProductId = p.ProductId;
                item.ProductName = p.ProductName;
                item.Manufacturer = manufacturer;
                item.Name = manufacturer + " | " + p.ProductName;
                item.Category = category;
                item.Description = p.Description;
                item.Structure = p.Structure;
                item.PriceValue = price;
                item.Price = price.ToString("F2") + " руб.";
                item.Count = count;
                item.Photo = GetPhoto(p.Picture);

                if (count <= 3)
                {
                    item.BackColor = red;
                }
                else
                {
                    item.BackColor = Brushes.White;
                }

                items.Add(item);
            }

            if (cbFilter.Items.Count == 0)
            {
                cbFilter.Items.Add("Все категории");
                foreach (var c in allCategories)
                {
                    cbFilter.Items.Add(c.CategoryName);
                }
                cbFilter.SelectedIndex = 0;
            }

            ready = true;
            ShowItems();
        }

        void ShowItems()
        {
            List<ProductItem> list = items;

            string text = tbSearch.Text.Trim().ToLower();
            if (text != "")
            {
                list = list.Where(x => x.Name.ToLower().Contains(text)
                                    || (x.Description ?? "").ToLower().Contains(text)).ToList();
            }

            if (cbFilter.SelectedIndex > 0)
            {
                string category = cbFilter.SelectedItem.ToString();
                list = list.Where(x => x.Category == category).ToList();
            }

            if (cbSort.SelectedIndex == 1)
            {
                list = list.OrderBy(x => x.PriceValue).ToList();
            }
            else if (cbSort.SelectedIndex == 2)
            {
                list = list.OrderByDescending(x => x.PriceValue).ToList();
            }

            ProductsList.ItemsSource = list;
        }

        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ready)
            {
                ShowItems();
            }
        }

        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ready)
            {
                ShowItems();
            }
        }

        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ready)
            {
                ShowItems();
            }
        }

        private void ProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProductItem selected = ProductsList.SelectedItem as ProductItem;
            if (selected == null)
            {
                return;
            }

            ProductsList.SelectedItem = null;

            if (role == 0)
            {
                MessageBox.Show("Оформление заказа доступно только авторизованным пользователям");
                return;
            }

            if (role != 1 && pickMode == false)
            {
                MessageBox.Show("Менеджер и администратор оформляют заказы в разделе «Заказы»");
                return;
            }

            ProductWindow window = new ProductWindow(selected);
            window.Owner = this;
            window.ShowDialog();

            UpdateCartButton();
        }

        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            CartWindow window = new CartWindow(currentUser, false);
            window.Owner = this;
            window.ShowDialog();

            LoadData();
            UpdateCartButton();
        }

        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            OrdersWindow window = new OrdersWindow(currentUser);
            window.Owner = this;
            window.ShowDialog();

            LoadData();
        }

        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        BitmapImage GetPhoto(string fileName)
        {
            string path = "pack://application:,,,/Image/picture.png";

            if (!string.IsNullOrEmpty(fileName))
            {
                path = "pack://application:,,,/Image/" + fileName;
            }

            try
            {
                return MakeImage(path);
            }
            catch
            {
                return MakeImage("pack://application:,,,/Image/picture.png");
            }
        }

        BitmapImage MakeImage(string path)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }
    }

    public class ProductItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public BitmapImage Photo { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Structure { get; set; }
        public string Price { get; set; }
        public decimal PriceValue { get; set; }
        public double Count { get; set; }
        public Brush BackColor { get; set; }
    }
}