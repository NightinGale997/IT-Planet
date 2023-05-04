using PlanetIT.DAL.Interfaces;
using PlanetIT.DAL.Repositories;
using PlanetIT.Domain.Models.Data;
using PlanetIT.Service.Implementations;
using PlanetIT.Service.Interfaces;

namespace PlanetIT
{
    public static class Initializer
    {
        public static void InitializeRepositories(this IServiceCollection services)
        {
            services.AddScoped<IBaseRepository<Account>, AccountRepository>();
            services.AddScoped<IBaseRepository<Animal>, AnimalRepository>();
            services.AddScoped<IBaseRepository<AnimalType>, AnimalTypeRepository>();
            services.AddScoped<IBaseRepository<AnimalTypeRelation>, AnimalTypeRelationRepository>();
            services.AddScoped<IBaseRepository<AnimalVisitedLocation>, AnimalVisitedLocationRepository>();
            services.AddScoped<IBaseRepository<LocationPoint>, LocationPointRepository>();
            services.AddScoped<IBaseRepository<Area>, AreaRepository>();
            services.AddScoped<IBaseRepository<AreaPoint>, AreaPointRepository>();
        }
        public static void InitializeServices(this IServiceCollection services)
        {
            services.AddScoped<IAnimalService, AnimalService>();
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAnimalTypeService, AnimalTypeService>();
            services.AddScoped<ILocationPointService, LocationPointService>();
            services.AddScoped<IAnimalVisitedLocationPointService, AnimalVisitedLocationService>();
            services.AddScoped<IAreaService, AreaService>();
        }
    }
}
