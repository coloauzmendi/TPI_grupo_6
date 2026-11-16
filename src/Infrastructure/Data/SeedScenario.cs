using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data
{
    public class SeedScenario
    {
        public const string OwnerPassword = "dueno123";
        public const string EmployeePassword = "empleado123";

        private const int HistoryDays = 30;
        private const int RestockDay = 14;

        private readonly SeedDataBuilder _builder;
        private readonly DateTime _now;
        private readonly DateTime _today;
        private readonly Dictionary<Branch, List<User>> _sellers = new();
        private readonly Dictionary<Branch, int> _maxQuantityPerLine = new();

        public SeedScenario(SeedDataBuilder builder, DateTime now)
        {
            _builder = builder;
            _now = now;
            _today = now.Date;
        }

        public void Build()
        {
            User martin = _builder.AddOwner("20-30111222-3", "Martín", "Gómez", OwnerPassword);
            User laura = _builder.AddOwner("27-28444555-6", "Laura", "Fernández", OwnerPassword);

            Company kiosk = _builder.AddCompany("Kiosco El Sol", martin);
            Company grocery = _builder.AddCompany("Almacén Don José", martin);
            Company hardware = _builder.AddCompany("Ferretería La Tuerca", laura);

            Branch kioskCenter = AddBranch(kiosk, "Rosario", "Córdoba 1234", Industry.Kiosk, martin, 3);
            Branch kioskNorth = AddBranch(kiosk, "Rosario", "Alberdi 850", Industry.Kiosk, martin, 3);
            Branch groceryFunes = AddBranch(grocery, "Funes", "Hipólito Yrigoyen 2100", Industry.Grocery, martin, 3);
            Branch groceryRosario = AddBranch(grocery, "Rosario", "Pellegrini 1500", Industry.Grocery, martin, 2);
            Branch hardwareRosario = AddBranch(hardware, "Rosario", "San Martín 3200", Industry.Hardware, laura, 2);
            Branch hardwareVgg = AddBranch(hardware, "Villa Gobernador Gálvez", "Av. Filippini 450", Industry.Hardware, laura, 1);
            Branch hardwarePerez = AddBranch(hardware, "Pérez", "Av. San Martín 900", Industry.Hardware, laura, 1);

            User sofia = AddEmployee("27-35111222-4", "Sofía", "Ramírez", kioskCenter);
            AddEmployee("20-36222333-5", "Lucas", "Díaz", kioskNorth);
            AddEmployee("27-37333444-6", "Valentina", "López", groceryFunes);
            AddEmployee("20-38444555-7", "Joaquín", "Torres", hardwareRosario);
            AddEmployee("27-39555666-8", "Camila", "Sosa", hardwarePerez);

            Product coke = _builder.AddProduct(kiosk, "Gaseosa cola 500 ml", "Coca-Cola", "Bebidas", 1500m, Industry.Kiosk);
            Product water = _builder.AddProduct(kiosk, "Agua mineral 500 ml", "Villavicencio", "Bebidas", 900m, Industry.Kiosk);
            Product alfajor = _builder.AddProduct(kiosk, "Alfajor triple", "Havanna", "Golosinas", 1800m, Industry.Kiosk);
            Product gum = _builder.AddProduct(kiosk, "Chicles menta", "Beldent", "Golosinas", 600m, Industry.Kiosk);
            Product chips = _builder.AddProduct(kiosk, "Papas fritas 90 g", "Lay's", "Snacks", 2200m, Industry.Kiosk);
            Product nougat = _builder.AddProduct(kiosk, "Turrón de maní", "Arcor", "Golosinas", 500m, Industry.Kiosk);
            _builder.AddProduct(kiosk, "Encendedor", "Bic", "Varios", 1200m, Industry.Kiosk);

            Product yerba = _builder.AddProduct(grocery, "Yerba mate 1 kg", "Playadito", "Almacén", 4500m, Industry.Grocery);
            Product pasta = _builder.AddProduct(grocery, "Fideos spaghetti 500 g", "Lucchetti", "Almacén", 1300m, Industry.Grocery);
            Product oil = _builder.AddProduct(grocery, "Aceite de girasol 1,5 L", "Natura", "Almacén", 3800m, Industry.Grocery);
            Product milk = _builder.AddProduct(grocery, "Leche entera 1 L", "La Serenísima", "Lácteos", 1400m, Industry.Grocery);
            Product rice = _builder.AddProduct(grocery, "Arroz largo fino 1 kg", "Gallo", "Almacén", 1900m, Industry.Grocery);
            Product crackers = _builder.AddProduct(grocery, "Galletitas de agua", "Criollitas", "Almacén", 1100m, Industry.Grocery);

            Product hammer = _builder.AddProduct(hardware, "Martillo carpintero", "Stanley", "Herramientas", 18500m, Industry.Hardware);
            Product screwdriver = _builder.AddProduct(hardware, "Destornillador plano", "Bahco", "Herramientas", 6500m, Industry.Hardware);
            Product screws = _builder.AddProduct(hardware, "Tornillos 6x40 x100", "Fischer", "Fijaciones", 4200m, Industry.Hardware);
            Product tape = _builder.AddProduct(hardware, "Cinta aisladora", "3M", "Electricidad", 1500m, Industry.Hardware);
            Product bulb = _builder.AddProduct(hardware, "Lámpara LED 9 W", "Philips", "Electricidad", 2800m, Industry.Hardware);
            Product drill = _builder.AddProduct(hardware, "Taladro percutor", "Black+Decker", "Herramientas eléctricas", 95000m, Industry.Hardware);

            DateTime loadDate = Day(HistoryDays).AddHours(8);

            Load(kioskCenter, martin, loadDate, (coke, 80, 15, 950m), (water, 60, 10, 520m), (alfajor, 50, 10, 1100m),
                (gum, 70, 10, 330m), (chips, 30, 15, 1400m), (nougat, 40, 5, 280m));
            Load(kioskNorth, martin, loadDate, (coke, 60, 10, 950m), (water, 4, 5, 520m), (alfajor, 40, 8, 1100m),
                (gum, 50, 8, 330m), (chips, 25, 20, 1400m));
            Load(groceryFunes, martin, loadDate, (yerba, 40, 8, 3100m), (pasta, 60, 10, 800m), (oil, 30, 6, 2600m),
                (milk, 50, 25, 950m), (rice, 45, 8, 1250m));
            Load(groceryRosario, martin, loadDate, (yerba, 30, 6, 3100m), (pasta, 40, 8, 800m), (oil, 6, 8, 2600m),
                (milk, 35, 10, 950m));
            Load(hardwareRosario, laura, loadDate, (hammer, 12, 3, 12000m), (screwdriver, 20, 4, 4100m), (screws, 30, 5, 2500m),
                (tape, 40, 8, 850m), (bulb, 35, 6, 1700m), (drill, 3, 1, 68000m));
            Load(hardwareVgg, laura, loadDate, (hammer, 8, 2, 12000m), (screws, 20, 5, 2500m), (tape, 25, 5, 850m),
                (bulb, 8, 10, 1700m));
            Load(hardwarePerez, laura, loadDate, (screwdriver, 15, 3, 4100m), (screws, 25, 5, 2500m), (tape, 30, 6, 850m),
                (bulb, 20, 5, 1700m), (drill, 2, 1, 68000m));

            _builder.AddSale(kioskCenter, sofia, Day(HistoryDays - 1).AddHours(10), (nougat, 5), (coke, 2));
            _builder.AddSale(kioskNorth, martin, Day(HistoryDays - 1).AddHours(11), (water, 4));
            _builder.DeactivateProduct(nougat);
            _builder.DeactivateProduct(crackers);

            GenerateSales(HistoryDays - 2, RestockDay + 1);

            DateTime restockDate = Day(RestockDay).AddHours(8);
            _builder.AddPurchase(kioskCenter, sofia, "Distribuidora Rosario", restockDate, (coke, 24, 980m), (chips, 12, 1450m));
            _builder.AddPurchase(groceryFunes, martin, "Mayorista Makro", restockDate, (yerba, 20, 3200m), (milk, 30, 980m));
            _builder.AddPurchase(hardwareRosario, laura, "Ferremax", restockDate, (screws, 15, 2550m), (tape, 20, 870m));

            GenerateSales(RestockDay, 0);

            _builder.CancelSale(_builder.Sales.Last(s => s.UserCuit == sofia.Cuit));
            _builder.CancelSale(_builder.Sales.First(s => s.UserCuit == laura.Cuit));
        }

        private Branch AddBranch(Company company, string locality, string address, Industry industry, User owner, int maxQuantityPerLine)
        {
            Branch branch = _builder.AddBranch(company, locality, address, industry);
            _sellers[branch] = new List<User> { owner };
            _maxQuantityPerLine[branch] = maxQuantityPerLine;

            return branch;
        }

        private User AddEmployee(string cuit, string firstName, string lastName, Branch branch)
        {
            User employee = _builder.AddEmployee(cuit, firstName, lastName, branch, EmployeePassword);
            _sellers[branch].Add(employee);

            return employee;
        }

        private void Load(Branch branch, User owner, DateTime date, params (Product Product, int Quantity, int MinimumStock, decimal UnitCost)[] items)
        {
            foreach (var item in items)
            {
                _builder.AddInitialLoad(branch, owner, item.Product, item.Quantity, item.MinimumStock, item.UnitCost, date);
            }
        }

        private void GenerateSales(int fromDaysAgo, int toDaysAgo)
        {
            for (int daysAgo = fromDaysAgo; daysAgo >= toDaysAgo; daysAgo--)
            {
                List<(Branch Branch, int Seed, DateTime Date)> planned = new();
                int branchIndex = 0;
                foreach (Branch branch in _sellers.Keys)
                {
                    int salesOfDay = (daysAgo + branchIndex) % 3;
                    for (int saleIndex = 0; saleIndex < salesOfDay; saleIndex++)
                    {
                        DateTime date = Day(daysAgo).AddHours(9 + saleIndex * 4).AddMinutes(branchIndex * 7);
                        planned.Add((branch, daysAgo + branchIndex + saleIndex, date));
                    }

                    branchIndex++;
                }

                foreach (var sale in planned.Where(p => p.Date <= _now).OrderBy(p => p.Date))
                {
                    TryAddSale(sale.Branch, sale.Seed, sale.Date);
                }
            }
        }

        private void TryAddSale(Branch branch, int seed, DateTime date)
        {
            List<User> sellers = _sellers[branch];
            List<Product> available = _builder.Products
                .Where(p => p.IsActive && _builder.FindStock(branch, p) is { IsActive: true })
                .ToList();
            if (available.Count == 0)
            {
                return;
            }

            int quantity = 1 + seed % _maxQuantityPerLine[branch];
            List<(Product Product, int Quantity)> items = new[] { available[seed % available.Count], available[(seed + 2) % available.Count] }
                .Distinct()
                .Where(p => _builder.FindStock(branch, p)!.Quantity >= quantity)
                .Select(p => (p, quantity))
                .ToList();

            if (items.Count > 0)
            {
                _builder.AddSale(branch, sellers[seed % sellers.Count], date, items.ToArray());
            }
        }

        private DateTime Day(int daysAgo)
        {
            return _today.AddDays(-daysAgo);
        }
    }
}
