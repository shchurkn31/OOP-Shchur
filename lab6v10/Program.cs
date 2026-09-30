using System;

namespace lab6v10
{
    public class Building
    {
        private string _address;
        private int _yearBuilt;

        public string Address
        {
            get { return _address; }
            set { _address = value; }
        }

        public int YearBuilt
        {
            get { return _yearBuilt; }
            set { _yearBuilt = value; }
        }

        public Building(string address, int yearBuilt)
        {
            _address = address;
            _yearBuilt = yearBuilt;
        }

        public virtual void GetInfo()
        {
            Console.WriteLine($"Будівля за адресою: {Address}, рік побудови: {YearBuilt}");
        }

        public void ShowType()
        {
            Console.WriteLine("Тип: Звичайна будівля");
        }
    }

    public class House : Building
    {
        private int _numFloors;

        public int NumFloors
        {
            get { return _numFloors; }
            set { _numFloors = value; }
        }

        public House(string address, int yearBuilt, int numFloors) : base(address, yearBuilt)
        {
            _numFloors = numFloors;
        }

        public override void GetInfo()
        {
            Console.WriteLine($"Житловий будинок: {Address}, рік: {YearBuilt}, поверхів: {NumFloors}");
        }

        public void OpenDoor()
        {
            Console.WriteLine("Двері будинку відчинено.");
        }

        public new void ShowType()
        {
            Console.WriteLine("Тип: Житловий будинок");
        }
    }

    public class Skyscraper : Building
    {
        private int _numElevators;

        public int NumElevators
        {
            get { return _numElevators; }
            set { _numElevators = value; }
        }

        public Skyscraper(string address, int yearBuilt, int numElevators) : base(address, yearBuilt)
        {
            _numElevators = numElevators;
        }

        public override void GetInfo()
        {
            Console.WriteLine($"Хмарочос: {Address}, рік: {YearBuilt}, ліфтів: {NumElevators}");
        }

        public void GoToRoof()
        {
            Console.WriteLine("Підйом на дах хмарочоса.");
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Building building = new Building("вул. Соборна, 1", 1995);
            House house = new House("вул. Зелена, 12", 2010, 3);
            Skyscraper skyscraper = new Skyscraper("пр. Незалежності, 50", 2020, 10);

            Console.WriteLine("--- Унікальні методи ---");
            house.OpenDoor();
            skyscraper.GoToRoof();

            Console.WriteLine("\n--- Демонстрація поліморфізму (GetInfo) ---");
            Building[] buildings = new Building[] { building, house, skyscraper };
            foreach (Building b in buildings)
            {
                b.GetInfo();
            }

            Console.WriteLine("\n--- Демонстрація new vs override ---");
            Building houseAsBuilding = house;

            Console.WriteLine("Виклик override GetInfo():");
            houseAsBuilding.GetInfo();

            Console.WriteLine("Виклик new ShowType():");
            house.ShowType();
            houseAsBuilding.ShowType();
        }
    }
}