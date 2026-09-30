using System;

namespace lab7v10
{
    public class Product
    {
        private string _name;
        private decimal _basePrice;

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public decimal BasePrice
        {
            get { return _basePrice; }
            set { _basePrice = value; }
        }

        public Product(string name, decimal basePrice)
        {
            _name = name;
            _basePrice = basePrice;
        }

        public virtual decimal GetPrice()
        {
            return _basePrice;
        }
    }

    public class Electronics : Product
    {
        private decimal _warrantyFee;

        public decimal WarrantyFee
        {
            get { return _warrantyFee; }
            set { _warrantyFee = value; }
        }

        public Electronics(string name, decimal basePrice, decimal warrantyFee) 
            : base(name, basePrice)
        {
            _warrantyFee = warrantyFee;
        }

        public override decimal GetPrice()
        {
            return BasePrice + _warrantyFee;
        }
    }

    public class Clothing : Product
    {
        private decimal _discount;

        public decimal Discount
        {
            get { return _discount; }
            set { _discount = value; }
        }

        public Clothing(string name, decimal basePrice, decimal discount) 
            : base(name, basePrice)
        {
            _discount = discount;
        }

        public new decimal GetPrice()
        {
            return BasePrice - _discount;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Product baseProduct = new Product("Товар", 1000m);
            Electronics laptop = new Electronics("Ноутбук", 25000m, 1500m);
            Clothing jacket = new Clothing("Куртка", 3000m, 500m);

            Product product1 = laptop;
            Product product2 = jacket;

            Console.WriteLine($"Base: {baseProduct.GetPrice()}");
            Console.WriteLine($"Electronics (as Product): {product1.GetPrice()}");
            Console.WriteLine($"Clothing (as Product): {product2.GetPrice()}");

            Console.WriteLine($"Electronics (cast): {((Electronics)product1).GetPrice()}");
            Console.WriteLine($"Clothing (cast): {((Clothing)product2).GetPrice()}");
        }
    }
}