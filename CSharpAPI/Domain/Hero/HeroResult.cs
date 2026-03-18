using API.Domain.Hero.AddressResults;

namespace API.Domain.Hero
{
    public class HeroResult: HeroesResult
    {
        public AddressResult AddressResult { get; set; }

    }
}
