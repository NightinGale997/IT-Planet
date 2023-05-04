using PlanetIT.Domain.Enums;
using PlanetIT.Domain.Models.Data;

namespace PlanetIT.Domain.Models.Response
{
    public class AnimalResponse
    {
        public long Id { get; set; }
        public List<long> AnimalTypes { get; set; }
        public float Weight { get; set; }
        public float Length { get; set; }
        public float Height { get; set; }
        public Gender Gender { get; set; }
        public LifeStatus LifeStatus { get; set; }
        public string ChippingDateTime { get; set; }
        public int ChipperId { get; set; }
        public long ChippingLocationId { get; set; }
        public List<long> VisitedLocations { get; set; }
        public string? DeathDateTime { get; set; }

        public AnimalResponse(Animal animal, AnimalTypeRelation[] animalTypeRelations, AnimalVisitedLocation[] animalVisitedLocations)
        {
            Id = animal.Id;
            Weight = animal.Weight;
            Length = animal.Length;
            Height = animal.Height;
            Gender = animal.Gender;
            LifeStatus = animal.LifeStatus;
            ChippingDateTime = animal.ChippingDateTime.ToString("yyyy-MM-ddTHH:mm:sszzz");
            ChipperId = animal.ChipperId;
            ChippingLocationId = animal.ChippingLocationId;
            DeathDateTime = animal.DeathDateTime?.ToString("yyyy-MM-ddTHH:mm:sszzz");

            AnimalTypes = new List<long>();

            foreach (var relation in animalTypeRelations)
            {
                AnimalTypes.Add(relation.AnimalTypeId);
            }

            VisitedLocations = new List<long>();

            foreach (var location in animalVisitedLocations)
            {
                VisitedLocations.Add(location.Id);
            }
        }
    }
}
