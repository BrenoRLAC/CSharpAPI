using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Domain.Hero.AddressRequest;
using System.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;

namespace CSharpAPI.Tests.integrationTests.helpers
{
    public static class DataGenerator
    {
        public static string GetRandomEmail() => $"test{Guid.NewGuid():N}@integration.com";
        public static string GetRandomUserName() => Guid.NewGuid().ToString("N").Substring(0, 8);

        public static string GrantType = "password";
        public static string GetRandomPassword()
        {
            var random = new Random();
            string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                   lower = "abcdefghijklmnopqrstuvwxyz",
                   nums = "0123456789",
                   spec = "!@#$%&";

            var chars = new List<char> {
                upper[random.Next(upper.Length)],
                lower[random.Next(lower.Length)],
                nums[random.Next(nums.Length)],
                spec[random.Next(spec.Length)]
            };

            string all = upper + lower + nums + spec;
            while (chars.Count < 12) chars.Add(all[random.Next(all.Length)]);

            return new string(chars.OrderBy(x => random.Next()).ToArray());
        }

        public static HeroRequest GetValidHeroRequest() => new HeroRequest
        {
            Name = $"Hero_{Guid.NewGuid():N}",
            Disguise = $"Secret_{Guid.NewGuid():N}",
            Description = "A generic description for test hero"
        };

        public static AddressRequest GetValidAddressRequest() => new AddressRequest
        {
            Street = "Main Street",
            Number = "123",
            Complement = "Apt 4",
            ZipCode = "76890000",
            ReferencePoint = "Near the park",
            City = "Metropolis",
            Neighborhood = "Downtown",
            State = "NY",           
        };

        public static AddressRequest GetInvalidAddressRequest() => new AddressRequest
        {
            Street = "Main Street",
            Number = "123",
            Complement = "Apt 4",
            ZipCode = "1111111111111111",
            ReferencePoint = "Near the park",
            City = "Metropolis",
            Neighborhood = "Downtown",
            State = "NY",
        };

        public static Address BuildCompleteHeroAddress() => new()
        {
            Street = "Main Street",
            Number = "123",
            Complement = "Apt 4",
            ZipCode = "76890000",
            ReferencePoint = "Near the park",
            City = "Metropolis",
            Neighborhood = "Downtown",
            State = "NY",
            Country = "USA"
        };

        public static MultipartFormDataContent BuildCompleteHeroFormData()
        {
            string uniqueId = Guid.NewGuid().ToString("N").Substring(0, 6);
            Address address = BuildCompleteHeroAddress();

            var formData = new MultipartFormDataContent();

            formData.Add(new StringContent($"Test_Name_{uniqueId}"), "Name");
            formData.Add(new StringContent($"Test_Disguise{uniqueId}"), "Disguise");
            formData.Add(new StringContent("Integration test hero with valid length"), "Description");

            formData.Add(new StringContent(address.Street), "Address.Street");
            formData.Add(new StringContent(address.Number), "Address.Number");
            formData.Add(new StringContent(address.Complement ?? ""), "Address.Complement");
            formData.Add(new StringContent(address.ZipCode), "Address.ZipCode");
            formData.Add(new StringContent(address.ReferencePoint ?? ""), "Address.ReferencePoint");
            formData.Add(new StringContent(address.City), "Address.City");
            formData.Add(new StringContent(address.Neighborhood), "Address.Neighborhood");
            formData.Add(new StringContent(address.State), "Address.State");
            formData.Add(new StringContent(address.Country), "Address.Country");

            ByteArrayContent imageContent = ImageContent();

            formData.Add(imageContent, "Image", "image.png");

            return formData;
        }

        public static ByteArrayContent ImageContent()
        {
            var imagePathCandidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "image.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "image.png"),
                Path.Combine(Directory.GetCurrentDirectory(), "CSharpAPI.Tests", "image.png")
            };

            var imagePath = imagePathCandidates.FirstOrDefault(File.Exists)
                ?? throw new FileNotFoundException(
                    "Could not find image.png. Add image.png to the test project root or output directory.");

            var imageBytes = File.ReadAllBytes(imagePath);
            var imageContent = new ByteArrayContent(imageBytes);


            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return imageContent;
        }
    }
}