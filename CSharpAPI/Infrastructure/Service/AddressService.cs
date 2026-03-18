
using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Infrastructure.Interface;

namespace API.Infrastructure.Service
{
    public class AddressService: IAddressService
    {
    
        private readonly IHttpClientFactory _clientFactory;

        public AddressService(IHttpClientFactory clientFactory)
        {         
            _clientFactory = clientFactory;
        }
 

        public async Task<Address> GetAddress(string cep)
        {

            var client = _clientFactory.CreateClient();
            
            var response = await client.GetAsync($"https://viacep.com.br/ws/{cep}/json/");
            
            if (!response.IsSuccessStatusCode) return null;
            
            var content = await response.Content.ReadFromJsonAsync<Address>();

            if (content.ZipCode == null) return null;

            return content;
        }
       
    }
}

