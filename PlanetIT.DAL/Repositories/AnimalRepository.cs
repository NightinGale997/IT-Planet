using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AnimalRepository : IBaseRepository<Animal>
    {
        private readonly ApplicationDbContext _db;

        public AnimalRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(Animal entity)
        {
            await _db.Animals.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Animal entity)
        {
            _db.Animals.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<Animal> GetAll()
        {
            return _db.Animals;
        }

        public async Task<Animal> Update(Animal entity)
        {
            _db.Animals.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
