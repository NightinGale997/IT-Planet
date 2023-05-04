using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AnimalTypeRepository : IBaseRepository<AnimalType>
    {
        private readonly ApplicationDbContext _db;

        public AnimalTypeRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(AnimalType entity)
        {
            await _db.AnimalTypes.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(AnimalType entity)
        {
            _db.AnimalTypes.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<AnimalType> GetAll()
        {
            return _db.AnimalTypes;
        }

        public async Task<AnimalType> Update(AnimalType entity)
        {
            _db.AnimalTypes.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
