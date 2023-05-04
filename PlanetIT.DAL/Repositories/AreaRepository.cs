using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AreaRepository : IBaseRepository<Area>
    {
        private readonly ApplicationDbContext _db;

        public AreaRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(Area entity)
        {
            await _db.Areas.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(Area entity)
        {
            _db.Areas.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<Area> GetAll()
        {
            return _db.Areas;
        }

        public async Task<Area> Update(Area entity)
        {
            _db.Areas.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
