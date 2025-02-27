using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using API.Infrastructure.Interface;
using API.Domain.Hero;
using CloudinaryDotNet.Actions;
using Newtonsoft.Json;
using API.Domain.HeroImages;
using API.Domain.Hero.AddressRequest;
using API.Utilities;
using API.Domain.Hero.AddressResults;
using API.Domain.Hero.Addresses;
namespace API.Infrastructure.Dao;

public class HeroDao : IHeroDao
{
    private readonly string _connectStr;
    private SqlConnection _connection;
    private SqlConnection Connection => _connection ??= new SqlConnection(_connectStr);

    public HeroDao(IConfiguration config)
    {
        _connectStr = config.GetConnectionString("Default");
    }
    public async Task<(List<HeroesResult>, int total)> ListHero(HeroFilter request)
    {

        const string procedure = "SP_LS_HEROES";

        var p = new DynamicParameters();
        p.Add("SEARCH", request.Search);
        p.Add("PAGE", request.PageNumber);
        p.Add("PAGINATION_SIZE", request.PageSize);
        p.Add("TOTAL", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var result = await Connection.QueryAsync<HeroesResult>(procedure, p, commandType: CommandType.StoredProcedure);

        var total = p.Get<int>("TOTAL");
       
        var processedResult = result.Select(item =>
        {
            item.Id = int.Parse(item.Id).EncryptInt();

            if (!string.IsNullOrEmpty(item.heroImage))
            {
             
                item.heroImages = JsonConvert.DeserializeObject<List<HeroImage>>(item.heroImage)
                    .Select(image =>
                    {
                        if (!string.IsNullOrEmpty(image.PublicId))
                        {
                            image.PublicId = image.PublicId.Encrypt();
                        }
                        return image;
                    })
                    .ToList();
            }
            return item;
        }).ToList();

        return (processedResult, total);


    }
    public async Task<HeroResult> GetHeroDetail(string id)
    {
        var hero = await Connection.QueryFirstOrDefaultAsync<HeroResult>("LIST_HERO_BY_ID", new { ID = id.DecryptInt() }, commandType: CommandType.StoredProcedure);

        if (hero == null) return hero;

        hero.Id = int.Parse(hero.Id).EncryptInt();

        if (hero.HeroImage == null) return hero;
        hero.HeroImages = JsonConvert.DeserializeObject<List<HeroImage>>(hero.HeroImage);

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
            hero.DisguiseName,
            hero.Description,

        }, commandType: CommandType.StoredProcedure);

    }

    public async Task SetImage(string heroId, List<ImageUploadResult> image)
    {
        var images = new DataTable("TP_CODE");

        images.Columns.Add("PUBLIC_ID", typeof(string));
        images.Columns.Add("URL", typeof(string));

        image?.ForEach(x => images.Rows.Add(x.PublicId, x.SecureUrl));


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
            hero.DisguiseName,
            hero.Description


        }, commandType: CommandType.StoredProcedure);
    }

    public async Task DeleteHero(string id)
    {
        await Connection.ExecuteAsync("DELETE_HERO", new
        {
            ID  = id.DecryptInt()

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
}

