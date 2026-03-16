using API.Domain.Hero;
using API.Domain.Hero.Addresses;

namespace API.Infrastructure.Interface
{
    public interface IAddressService
    {
        Task<Address> GetAddress(string cep);
    }
}