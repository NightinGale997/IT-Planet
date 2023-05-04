using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class LocationPointRepository : IBaseRepository<LocationPoint>
    {
        private readonly ApplicationDbContext _db;

        public LocationPointRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(LocationPoint entity)
        {
            await _db.LocationPoints.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(LocationPoint entity)
        {
            _db.LocationPoints.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<LocationPoint> GetAll()
        {
            return _db.LocationPoints;
        }

        public async Task<LocationPoint> Update(LocationPoint entity)
        {
            _db.LocationPoints.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
