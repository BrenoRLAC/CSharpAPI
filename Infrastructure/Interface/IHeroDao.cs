using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Domain.Hero.AddressRequest;
using API.Domain.Hero.AddressResults;
using API.Domain.Pagination;
using CloudinaryDotNet.Actions;


namespace API.Infrastructure.Interface
{
    public interface IHeroDao
    {
        Task<(List<HeroesResult>, int total)> ListHero(HeroFilter request);
        Task<HeroResult> GetHeroDetail(string id);
        Task<AddressResult> GetHeroAddress(string heroId);
        Task SetHero(HeroRequest hero);
        Task SetImage(string heroId, ImageUploadResult image);
        Task SetHeroAddress(string id, Address address);
        Task UpdateHero(string heroId, HeroRequest hero);
        Task DeleteHero(string id);
        Task UpdateHeroAddress(string heroId, AddressRequest address);
        Task DeleteHeroImage(string heroId, string imageId);
        Task<int> ListActiveHeroes();
        Task SetCompleteHero(CompleteHeroRequest request, ImageUploadResult imageResult);
    }
}
