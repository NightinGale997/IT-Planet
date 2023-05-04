using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanetIT.DAL.Repositories
{
    public class AreaPointRepository : IBaseRepository<AreaPoint>
    {
        private readonly ApplicationDbContext _db;

        public AreaPointRepository(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task Create(AreaPoint entity)
        {
            await _db.AreaPoints.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(AreaPoint entity)
        {
            _db.AreaPoints.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<AreaPoint> GetAll()
        {
            return _db.AreaPoints;
        }

        public async Task<AreaPoint> Update(AreaPoint entity)
        {
            _db.AreaPoints.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}
