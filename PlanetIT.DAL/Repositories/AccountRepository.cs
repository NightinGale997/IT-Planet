using Microsoft.EntityFrameworkCore;
using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AccountRepository : IBaseRepository<Account>
    {
        private readonly ApplicationDbContext _db;

        public AccountRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(Account entity)
        {
            try
            {
                await _db.Accounts.AddAsync(entity);
                await _db.SaveChangesAsync();
            }
            catch
            {
                // Синхронизировать инкремент базы данных
                _db.SynchronizeAccountSequence();
                await _db.Accounts.AddAsync(entity);
                await _db.SaveChangesAsync();
            }
        }

        public async Task Delete(Account entity)
        {
            _db.Accounts.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<Account> GetAll()
        {
            return _db.Accounts;
        }

        public async Task<Account> Update(Account entity)
        {
            _db.Accounts.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
        
    }
}
