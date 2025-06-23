using API.Domain;
using API.Domain.Hero;
using API.Domain.Hero.AddressRequest;
using API.Domain.Pagination;
using API.Infrastructure.Interface;
using API.Utilities;
using CloudinaryServiceInterface.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("[controller]")]
public class HeroController(IHeroService heroService, IAddressService addressService, ICloudinaryService cloudinaryService, ILogger<HeroController> logger) : ControllerBase
{
    private readonly IHeroService _heroService = heroService;
    private readonly IAddressService _addressService = addressService;
    private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
    private readonly ILogger<HeroController> _logger = logger;


    [HttpGet, AllowAnonymous, Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApiPaged<List<HeroResult>>))]
    public async Task<IActionResult> GetHeroes([FromQuery] HeroFilter request)
    {
        var codUser = (User?.Identity?.IsAuthenticated ?? false)
        ? User.Identity.GetCodUser()
        : (int?)null;

        _logger.LogInformation("GET /Hero by user {codUser}", codUser?.ToString() ?? "anonymous");

        Pagination.ConfigPagination(request);

        var (result, total) = await _heroService.ListHero(request);

        var pg = Pagination.DefinePaginationResult(request,
            HttpContext,
            total,
            Url.Action("Get"));

        return Ok(new ReturnApiPaged<List<HeroesResult>>(200, result, pg));
    }


    [HttpGet("{id}"), AllowAnonymous, Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<HeroResult>))]
    public async Task<IActionResult> GetHeroById([FromRoute] string id)
    {
        var codUser = (User?.Identity?.IsAuthenticated ?? false)
       ? User.Identity.GetCodUser()
       : (int?)null;

        _logger.LogInformation("GET /Hero by user {codUser}", codUser?.ToString() ?? "anonymous");

        var hero = await _heroService.GetHeroDetail(id);

        if (hero is null)
            return BadRequest(new ReturnApi<HeroResult>(400, "This Hero does not exist"));

        var address = await _heroService.GetHeroAddress(id);

        hero.AddressResult = address;

        return Ok(new ReturnApi<HeroResult>(200, hero));

    }


    [HttpPost, Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHero([FromBody] HeroRequest request)
    {
            var codUser = User.Identity.GetCodUser();
            _logger.LogInformation("Request POST /Hero {@Request} {@Obj}", request.ToJson(), codUser);

            await _heroService.SetHero(request);

            return Ok(new ReturnApi<object>(201, "Hero added successfully"));
        }

    [HttpPost("{id}/image"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHeroImage([FromRoute] string id, IFormFile images)
    {
            var codUser = User.Identity.GetCodUser();

            _logger.LogInformation(
                "Request POST /Hero/{Id}/image by User {CodUser} with file {FileName} ({FileSize} bytes)", id, codUser, images?.FileName, images?.Length);

            if (images == null) return BadRequest(new ReturnApi<object>(400, "Add at least one image"));

            var image = _cloudinaryService.UploadImages(images);

            if (images == null)
                return BadRequest(new ReturnApi<HeroResult>(400, "There is a problem while uploading the image of the hero"));


            await _heroService.SetImage(id, image);

            return Ok(new ReturnApi<object>(201, "Hero's image added successfully"));
        }

    [HttpPost, Route("{id}/address"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> SetHeroAddress([FromRoute] string id, [FromBody] AddressRequest address)
    {
            var codUser = User.Identity.GetCodUser();

            _logger.LogInformation("Request POST /Hero/{Id}/address by User {CodUser} with payload {@Address}", id, codUser, address);

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


    [HttpPut, Route("{id}/address"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> UpdateHeroAddress([FromRoute] string id, [FromBody] AddressRequest address)
    {
            var codUser = User.Identity.GetCodUser();

            _logger.LogInformation("Request PUT /Hero/{Id}/address by User {CodUser} with payload {@Address}", id, codUser, address);

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


    [HttpPut, Route("{id}"), Consumes("application/json"), Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> UpdateHero([FromRoute] string id, [FromBody] HeroRequest hero)
    {
            var codUser = User.Identity.GetCodUser();

            _logger.LogInformation("Request PUT /Hero/{Id} by User {CodUser} with payload {@Hero}", id, codUser, hero);

            await _heroService.UpdateHero(id, hero);

            return Ok(new ReturnApi<object>(200, "Hero updated successfully"));

    }

    [HttpDelete("{id}")]
    [Consumes("application/json")]
    [Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> DeleteHero([FromRoute] string id)
    {
            var codUser = User.Identity.GetCodUser();
            _logger.LogInformation("Request DELETE /Hero/{Id} by User {CodUser}", id, codUser);

            await _heroService.DeleteHero(id);

            return Ok(new ReturnApi<object>(200, "Hero deleted successfully"));
    }

    [HttpDelete("{id}/image/{imageId}")]
    [Consumes("application/json")]
    [Produces("application/json", Type = typeof(ReturnApi<object>))]
    public async Task<IActionResult> DeleteHeroImage([FromRoute] string id, [FromRoute] string imageId)
    {
            var codUser = User.Identity.GetCodUser();

            _logger.LogInformation("Request DELETE /Hero/{Id}/image/{ImageId} by User {CodUser}", id, imageId, codUser);

            await _heroService.DeleteHeroImage(id, imageId);

            var image = await _cloudinaryService.DeleteImage(imageId);

            if (image.Result == "not found")
                return BadRequest(new ReturnApi<HeroResult>(400, "This image does not exist"));

            return Ok(new ReturnApi<object>(201, "Hero's image deleted successfully"));
        }
}

