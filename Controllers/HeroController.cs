using API.Domain;
using API.Domain.Hero;
using API.Domain.Hero.AddressRequest;
using API.Domain.Pagination;
using API.Infrastructure.Interface;
using CloudinaryServiceInterface.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class Hero : ControllerBase
{

    public Hero(IHeroService heroService, IAddressService addressService, ICloudinaryService cloudinaryService)
    {
        _heroService = heroService;
        _addressService = addressService;
        _cloudinaryService = cloudinaryService;
    }

    private readonly IHeroService _heroService;
    private readonly IAddressService _addressService;
    private readonly ICloudinaryService _cloudinaryService;


    [HttpGet, Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApiPaged<List<HeroResult>>))]
    public async Task<IActionResult> GetHeroes([FromQuery] HeroFilter request)
    {
        Pagination.ConfigPagination(request);


        var (result, total) = await _heroService.ListHero(request);

        var pg = Pagination.DefinePaginationResult(request,
            HttpContext,
            total,
            Url.Action("Get"));

        return Ok(new ReturnApiPaged<List<HeroesResult>>(200, result, pg));
    }


    [HttpGet("{id}"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<HeroResult>))]
    public async Task<IActionResult> GetHero([FromRoute] string id)
    {
        var hero = await _heroService.GetHeroDetail(id);

        if (hero is null)
            return BadRequest(new ReturnApi<HeroResult>(400, "This Hero does not exist"));


        var address = await _heroService.GetHeroAddress(id);

        hero.AddressResult = address;

        return Ok(new ReturnApi<HeroResult>(200, hero));

    }


    [HttpPost, Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHero(HeroRequest hero)
    {
        try
        {
            await _heroService.SetHero(hero);

            return Ok(new ReturnApi<object>(201, "Hero added successfully"));

        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(400, ex.Message));
        }


    }

    [HttpPost("HeroImage/{id}"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHeroImage([FromRoute] string id, IFormFile images)
    {

        try
        {
            if (images == null) return BadRequest(new ReturnApi<object>(400, "Add at least one image"));


            var image = _cloudinaryService.UploadImages(images);

            if (images == null)
                return BadRequest(new ReturnApi<HeroResult>(400, "There is a problem while uploading the image of the hero"));

         
            await _heroService.SetImage(id, image);

            return Ok(new ReturnApi<object>(201, "Hero's image added successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(400, ex.Message));
        }
    }

    [HttpPost, Route("HeroAddress/{id}"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHeroAddress([FromRoute]string id, [FromBody] AddressRequest address)
    {
        try
        {
            var addressValidation = await _addressService.GetAddress(address.ZipCode);

            var errors = new List<string>();

            if (addressValidation == null)
                errors.Add("Invalid Address");

            if (errors.Count > 0)
            {
                return BadRequest(string.Join("\n", errors));
            }

            addressValidation.ZipCode = address.ZipCode;
            addressValidation.Number = address.Number;
            addressValidation.Complement = address.Complement;
            addressValidation.ReferencePoint = address.ReferencePoint;

            await _heroService.SetHeroAddress(id, addressValidation);

            return Ok(new ReturnApi<object>(200, "Hero added successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(200, ex.Message));
        }
    }


    [HttpPut, Route("UpdateAddress/{id}"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> UpdateHeroAddress([FromRoute] string id, [FromBody] AddressRequest address)
    {
        try
        {
            var addressValidation = await _addressService.GetAddress(address.ZipCode);

            var errors = new List<string>();

            if (addressValidation == null)
                errors.Add("Invalid Address");

            if (errors.Count > 0)
            {
                return BadRequest(string.Join("\n", errors));
            }
          
            await _heroService.UpdateHeroAddress(id, address);

            return Ok(new ReturnApi<object>(200, "Address updated successfully"));
        }

        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(200, ex.Message));
        }
    }

    
    [HttpPut, Route("UpdateHero/{id}"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> UpdateHero([FromRoute]string id, [FromBody]HeroRequest hero)
    {
        try
        {
            await _heroService.UpdateHero(id, hero);

            return Ok(new ReturnApi<object>(200, "Hero updated successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(200, ex.Message));
        }


    }

    [HttpDelete("{id}")]
    [Consumes("application/json")]
    [Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> DeleteHero([FromRoute] string id)
    {
        try
        {
            await _heroService.DeleteHero(id);

            return Ok(new ReturnApi<object>(200, "Hero deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(500, ex.Message));
        }

    }

    [HttpDelete("{id}/image/{imageId}")]
    [Consumes("application/json")]
    [Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> DeleteHeroImage([FromRoute] string id, [FromRoute] string imageId)
    {
        try
        {
            await _heroService.DeleteHeroImage(id, imageId);

            var image = await _cloudinaryService.DeleteImage(imageId);

            if (image.Result == "not found")
                return BadRequest(new ReturnApi<HeroResult>(400, "This image does not exist"));

            return Ok(new ReturnApi<object>(201, "Hero's image deleted successfully"));
        }
        catch (Exception ex)
        {
            return BadRequest(new ReturnApi<object>(400, ex.Message));
        }

    }




}

