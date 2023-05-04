using Microsoft.EntityFrameworkCore;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            Database.EnsureCreated();
            if (Accounts.Any())
            {
                return; // Не добавлять аккаунтов, так как они уже есть
            }

            try
            {
                // Добавить аккаунты
                InitiateDefaultAccounts();
                // Синхронизировать инкремент датабазы
                SynchronizeAccountSequence();
            }
            catch
            {
                // Синхронизировать инкремент датабазы и уже потом добавить аккаунты (связано с множественными ошибками)
                SynchronizeAccountSequence();
                InitiateDefaultAccounts();
                return;
            }
        }
        private void InitiateDefaultAccounts()
        {
            var accounts = new Account[]
             {
                new Account {
                    Id = 1,
                    FirstName = "adminFirstName",
                    LastName = "adminLastName",
                    Email = "admin@simbirsoft.com",
                    Password = "qwerty123",
                    Role = Role.ADMIN
                },
                new Account {
                    Id = 2,
                    FirstName = "chipperFirstName",
                    LastName = "chipperLastName",
                    Email = "chipper@simbirsoft.com",
                    Password = "qwerty123",
                    Role = Role.CHIPPER
                },
                new Account {
                    Id = 3,
                    FirstName = "userFirstName",
                    LastName = "userLastName",
                    Email = "user@simbirsoft.com",
                    Password = "qwerty123",
                    Role = Role.USER
                }
            };

            Accounts.AddRange(accounts);
            SaveChanges();
        }
        public void SynchronizeAccountSequence()
        {
            Database.ExecuteSqlRaw("SELECT setval('animal-chipization.public.\"Accounts_Id_seq\"', coalesce((select max(\"Id\")+1 from \"animal-chipization\".public.\"Accounts\"), 1), false);");
        }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Animal> Animals { get; set; }
        public DbSet<AnimalType> AnimalTypes { get; set; }
        public DbSet<AnimalTypeRelation> AnimalTypeRelations { get; set; }
        public DbSet<LocationPoint> LocationPoints { get; set; }
        public DbSet<AreaPoint> AreaPoints { get; set; }
        public DbSet<AnimalVisitedLocation> AnimalVisitedLocations { get; set; }
    }
}