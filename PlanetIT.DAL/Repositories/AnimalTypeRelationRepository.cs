using PlanetIT.DAL.Interfaces;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.DAL.Repositories
{
    public class AnimalTypeRelationRepository : IBaseRepository<AnimalTypeRelation>
    {
        private readonly ApplicationDbContext _db;

        public AnimalTypeRelationRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task Create(AnimalTypeRelation entity)
        {
            await _db.AnimalTypeRelations.AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task Delete(AnimalTypeRelation entity)
        {
            _db.AnimalTypeRelations.Remove(entity);
            await _db.SaveChangesAsync();
        }

        public IQueryable<AnimalTypeRelation> GetAll()
        {
            return _db.AnimalTypeRelations;
        }

        public async Task<AnimalTypeRelation> Update(AnimalTypeRelation entity)
        {
            _db.AnimalTypeRelations.Update(entity);
            await _db.SaveChangesAsync();

            return entity;
        }
    }
}

