using API.Domain.Hero;
using API.Domain.Hero.Addresses;
using API.Domain.Hero.AddressRequest;
using API.Domain.Hero.AddressResults;
using API.Domain.HeroImages;
using API.Infrastructure.Interface;
using API.Utilities;
using CloudinaryDotNet.Actions;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;
namespace API.Infrastructure.Dao;

public class HeroDao(IConfiguration config) : IHeroDao
{

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseUpper
    };


    private readonly string _connectStr = config.GetConnectionString("Default");
    private SqlConnection _connection;
    private SqlConnection Connection => _connection ??= new SqlConnection(_connectStr);


    public async Task<(List<HeroesResult>, int total)> ListHero(HeroFilter request)
    {
        const string procedure = "SP_LS_HEROES";

        var p = new DynamicParameters();
        p.Add("SEARCH", request.Search);
        p.Add("PAGE", request.PageNumber);
        p.Add("PAGINATION_SIZE", request.PageSize);
        p.Add("TOTAL", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var result = await Connection.QueryAsync<HeroesResult>(procedure, p, commandType: CommandType.StoredProcedure);

        var list = result.ToList();

        list.ForEach(ProcessedResult);

        return (list, p.Get<int>("TOTAL"));

    }
    public async Task<HeroResult> GetHeroDetail(string id)
    {
        var hero = await Connection.QueryFirstOrDefaultAsync<HeroResult>(
            "LIST_HERO_BY_ID",
            new { ID = id.DecryptInt() },
            commandType: CommandType.StoredProcedure
        );

        ProcessedResult(hero);
        return hero;
    }

    public async Task<AddressResult> GetHeroAddress(string heroId)
    {
        return await Connection.QueryFirstOrDefaultAsync<AddressResult>("SP_LS_HERO_ADDRESS",
        new { HERO_ID = heroId.DecryptInt() }, commandType: CommandType.StoredProcedure);

    }

    public async Task SetHero(HeroRequest hero)
    {

        await Connection.ExecuteAsync("INSERT_HERO", new
        {
            hero.Name,
            hero.Disguise,
            hero.Description,

        }, commandType: CommandType.StoredProcedure);

    }

    public async Task SetImage(string heroId, ImageUploadResult image)
    {
        var images = new DataTable("TP_CODE");

        images.Columns.Add("PUBLIC_ID", typeof(string));
        images.Columns.Add("URL", typeof(string));

        images.Rows.Add(image.PublicId, image.SecureUrl);


        await Connection.QueryFirstOrDefaultAsync<object>("INSERT_HERO_IMAGE", new
        {
            HERO_ID = heroId.DecryptInt(),
            IMAGES = images

        }, commandType: CommandType.StoredProcedure);

    }

    public async Task SetHeroAddress(string id, Address address)
    {

        await Connection.ExecuteAsync("INSERT_HERO_ADDRESS", new
        {
            HERO_ID = id.DecryptInt(),
            address.State,
            address.City,
            address.Neighborhood,
            address.ZipCode,
            address.Street,
            address.Country,
            address.Number,
            address.Complement,
            address.ReferencePoint
        }, commandType: CommandType.StoredProcedure);

    }

    public async Task UpdateHero(string heroId, HeroRequest hero)
    {

        await Connection.ExecuteAsync("UPDATE_HERO", new
        {
            id = heroId.DecryptInt(),
            hero.Name,
            hero.Disguise,
            hero.Description


        }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteHero(string id)
    {
        await Connection.ExecuteAsync("DELETE_HERO", new
        {
            ID = id.DecryptInt()

        }, commandType: CommandType.StoredProcedure);
    }

    public async Task UpdateHeroAddress(string heroId, AddressRequest address)
    {
        await Connection.ExecuteAsync("UPDATE_HERO_ADDRESS", new
        {
            HERO_ID = heroId.DecryptInt(),
            address.State,
            address.City,
            address.Neighborhood,
            address.ZipCode,
            address.Street,
            address.Country,
            address.Number,
            address.Complement,
            address.ReferencePoint
        }, commandType: CommandType.StoredProcedure);

    }

    public async Task DeleteHeroImage(string heroId, string imageId)
    {
        await Connection.ExecuteAsync("DELETE_HERO_IMAGE", new
        {
            HEROID = heroId.DecryptInt(),
            IMAGEID = imageId.Decrypt()

        }, commandType: CommandType.StoredProcedure);
    }


    public async Task<int> ListActiveHeroes()
    {
        return await Connection.QueryFirstOrDefaultAsync<int>("LIST_ACTIVE_HEROES", commandType: CommandType.StoredProcedure);

    }

    public async Task SetCompleteHero(CompleteHeroRequest request, ImageUploadResult imageResult)
    {
        var image = new DataTable("TP_CODE");

        image.Columns.Add("PUBLIC_ID", typeof(string));
        image.Columns.Add("URL", typeof(string));

        image.Rows.Add(imageResult.PublicId, imageResult.SecureUrl);


        await Connection.ExecuteAsync("SP_INSERT_COMPLETE_HERO", new
        {
            request.Name,
            request.Disguise,
            request.Description,
            IMAGES = image,
            request.Address.State,
            request.Address.City,
            request.Address.Neighborhood,
            request.Address.ZipCode,
            request.Address.Street,
            request.Address.Country,
            request.Address.Number,
            request.Address.Complement,
            request.Address.ReferencePoint

        }, commandType: CommandType.StoredProcedure);
    }
    private void ProcessedResult(HeroesResult hero)
    {
        if (hero == null) return;

        if (int.TryParse(hero.Id, out int numericId))
            hero.Id = numericId.EncryptInt();

        if (!string.IsNullOrEmpty(hero.Image))
        {
            var deserialized = JsonSerializer.Deserialize<List<HeroImage>>(hero.Image, JsonOptions);
            if (deserialized != null)
            {
                foreach (var img in deserialized.Where(i => !string.IsNullOrEmpty(i.PublicId)))
                {
                    img.PublicId = img.PublicId.Encrypt();
                }
                hero.Images = deserialized;
            }
        }
    }
}
