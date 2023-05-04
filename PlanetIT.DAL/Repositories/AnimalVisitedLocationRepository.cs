using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AnimalVisitedLocationRepository : IBaseRepository<AnimalVisitedLocation>
    {
        private readonly ApplicationDbContext _db;

        public AnimalVisitedLocationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(AnimalVisitedLocation entity)
        {
            await _db.AnimalVisitedLocations.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(AnimalVisitedLocation entity)
        {
            _db.AnimalVisitedLocations.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<AnimalVisitedLocation> GetAll()
        {
            return _db.AnimalVisitedLocations;
        }

        public async Task<AnimalVisitedLocation> Update(AnimalVisitedLocation entity)
        {
            _db.AnimalVisitedLocations.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
