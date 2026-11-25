using System.Linq;
using System.Windows;
using shop.Models;

namespace shop
{
    public partial class ProductWindow : Window
    {
        AppDbContext db = new AppDbContext();
        ProductItem product;

        public ProductWindow(ProductItem product)
        {
            InitializeComponent();

            this.product = product;

            imgPhoto.Source = product.Photo;
            txtName.Text = product.ProductName;
            txtManufacturer.Text = "Производство: " + product.Manufacturer;
            txtCategory.Text = "Категория: " + product.Category;
            txtPrice.Text = "Цена со скидкой: " + product.Price;
            txtStructure.Text = "Состав: " + product.Structure;
            txtDescription.Text = "Описание: " + product.Description;

            LoadSizes();
        }

        void LoadSizes()
        {
            var sizes = db.SizesImports.ToList();
            var rows = db.StockItemsImports.Where(s => s.ProductId == product.ProductId).ToList();

            cbSize.Items.Clear();
            string series = "";

            foreach (var r in rows)
            {
                string sizeName = "";
                foreach (var z in sizes)
                {
                    if (z.SizeId == r.SizeId)
                    {
                        sizeName = z.SizeName.ToString();
                    }
                }

                if (series != "")
                {
                    series += ", ";
                }
                series += sizeName;

                int inCart = 0;
                foreach (var l in MainWindow.Cart)
                {
                    if (l.ProductId == product.ProductId && l.SizeId == r.SizeId)
                    {
                        inCart += l.Quantity;
                    }
                }

                int available = r.QuantityForOrders - inCart;
                if (available > 0)
                {
                    SizeOption option = new SizeOption();
                    option.SizeId = r.SizeId;
                    option.SizeName = sizeName;
                    option.Available = available;
                    cbSize.Items.Add(option);
                }
            }

            txtSizes.Text = "Размерный ряд: " + series;

            if (cbSize.Items.Count > 0)
            {
                cbSize.SelectedIndex = 0;
            }
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            SizeOption size = cbSize.SelectedItem as SizeOption;
            if (size == null)
            {
                MessageBox.Show("Выберите размер");
                return;
            }

            int quantity;
            if (!int.TryParse(tbQuantity.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Введите количество больше нуля");
                return;
            }

            if (quantity > size.Available)
            {
                MessageBox.Show("Доступно только " + size.Available + " шт.");
                return;
            }

            CartLine line = MainWindow.Cart.FirstOrDefault(l => l.ProductId == product.ProductId && l.SizeId == size.SizeId);
            if (line != null)
            {
                line.Quantity += quantity;
            }
            else
            {
                line = new CartLine();
                line.ProductId = product.ProductId;
                line.ProductName = product.ProductName;
                line.Manufacturer = product.Manufacturer;
                line.SizeId = size.SizeId;
                line.SizeName = size.SizeName;
                line.Quantity = quantity;
                line.Price = product.PriceValue;
                MainWindow.Cart.Add(line);
            }

            MessageBox.Show("Товар добавлен в заказ");
            tbQuantity.Text = "1";
            LoadSizes();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    public class SizeOption
    {
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public int Available { get; set; }

        public override string ToString()
        {
            return SizeName + " (в наличии: " + Available + ")";
        }
    }

    public class CartLine
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string Manufacturer { get; set; }
        public int SizeId { get; set; }
        public string SizeName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public string Text
        {
            get
            {
                return Manufacturer + " | " + ProductName + ", размер " + SizeName + ", " + Quantity + " шт. x " + Price.ToString("F2") + " руб.";
            }
        }
    }
}