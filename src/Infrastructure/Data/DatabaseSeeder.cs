using Application.Interfaces;

namespace Infrastructure.Data
{
    public class DatabaseSeeder
    {
        private readonly ApplicationContext _context;
        private readonly IPasswordHashService _passwordHashService;

        public DatabaseSeeder(ApplicationContext context, IPasswordHashService passwordHashService)
        {
            _context = context;
            _passwordHashService = passwordHashService;
        }

        public bool Seed()
        {
            if (!IsEmpty())
            {
                return false;
            }

            SeedDataBuilder builder = new SeedDataBuilder(_passwordHashService);
            new SeedScenario(builder, DateTime.Now).Build();

            _context.Users.AddRange(builder.Users);
            _context.Companies.AddRange(builder.Companies);
            _context.Branches.AddRange(builder.Branches);
            _context.Products.AddRange(builder.Products);
            _context.BranchStocks.AddRange(builder.Stocks);
            _context.Purchases.AddRange(builder.Purchases);
            _context.Sales.AddRange(builder.Sales);
            _context.SaveChanges();

            return true;
        }

        private bool IsEmpty()
        {
            return !_context.Users.Any()
                && !_context.Companies.Any()
                && !_context.Branches.Any()
                && !_context.Products.Any()
                && !_context.Sales.Any()
                && !_context.Purchases.Any();
        }
    }
}
