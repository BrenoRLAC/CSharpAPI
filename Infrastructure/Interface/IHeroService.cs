using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Domain.Hero.AddressRequest;
using API.Domain.Hero.AddressResults;
using API.Domain.HeroImages;
using API.Domain.Pagination;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;

namespace API.Infrastructure.Interface
{
    public interface IHeroService
    {
        Task<(List<HeroesResult>, int total)> ListHero(HeroFilter request);
        Task<HeroResult> GetHeroDetail(string heroId);
        Task<AddressResult> GetHeroAddress(string heroId);       
        Task SetHero(HeroRequest hero);
        Task SetImage(string heroId, ImageUploadResult image);
        Task SetHeroAddress(string id, Address address);
        Task UpdateHero(string heroId, HeroRequest hero);
        Task DeleteHero(string id);
        Task UpdateHeroAddress(string heroId, AddressRequest address);
        Task DeleteHeroImage(string heroId, string imageId);
        Task SetCompleteHero(CompleteHeroRequest request, ImageUploadResult imageResult);
    }
}
