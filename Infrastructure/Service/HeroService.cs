using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Domain.Hero.AddressRequest;
using API.Domain.Hero.AddressResults;
using API.Infrastructure.Interface;
using CloudinaryDotNet.Actions;

namespace API.Infrastructure.Service
{
    public class HeroService : IHeroService
    {
        private readonly IHeroDao _dao;

        public HeroService(IHeroDao dao)
        {
            _dao = dao;
        }
        public async Task<(List<HeroesResult>, int total)> ListHero(HeroFilter request)
        {
            var (result, total) = await _dao.ListHero(request);

            return (result, total);
        }
        public Task<HeroResult> GetHeroDetail(string id)
        {
            return _dao.GetHeroDetail(id);
        }

        public async Task SetHero(HeroRequest hero)
        {
            await _dao.SetHero(hero);
        }

        public async Task SetImage(string heroId, ImageUploadResult image)
        {
            await _dao.SetImage(heroId, image);
        }

        public async Task SetHeroAddress(string id, Address address)
        {
            await _dao.SetHeroAddress(id, address);
        }

        public async Task UpdateHero(string heroId, HeroRequest hero)
        {
            await _dao.UpdateHero(heroId, hero);
        }
        public async Task DeleteHero(string id)
        {
            await _dao.DeleteHero(id);

        }
        public Task<AddressResult> GetHeroAddress(string heroId)
        {
            return _dao.GetHeroAddress(heroId);
        }

        public Task UpdateHeroAddress(string heroId, AddressRequest address)
        {
            return _dao.UpdateHeroAddress(heroId, address);
        }

        public Task DeleteHeroImage(string heroId, string imageId)
        {
            return _dao.DeleteHeroImage(heroId, imageId);
        }
    }
}
