using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewGenie.Application.Contracts;
using ReviewGenie.Domain.Entities;
using ReviewGenie.Domain.ValueObjects;
using System.Security.Claims;

namespace ReviewGenie.WebApi.Controllers;

[ApiController]
[Route("api/businesses")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IBusinessRepository _businessRepo;

    public BusinessController(IBusinessRepository businessRepo)
    {
        _businessRepo = businessRepo;
    }

    [HttpGet]
    public async Task<ActionResult<List<BusinessResponseDto>>> GetUserBusinesses()
    {
        var userId = GetCurrentUserId();
        
        // Get all businesses owned by the current user
        var userBusinesses = await _businessRepo.GetByOwnerIdAsync(userId);
        
        var businessDtos = userBusinesses.Select(business => new BusinessResponseDto
        {
            Id = business.Id,
            Name = business.Name,
            Type = business.Type,
            Address = new AddressDto
            {
                Street = business.Address.Street,
                City = business.Address.City,
                State = business.Address.State,
                Zip = business.Address.Zip
            },
            Phone = business.Phone,
            Website = business.Website
        }).ToList();

        return Ok(businessDtos);
    }

    [HttpPost]
    public async Task<ActionResult<BusinessResponseDto>> CreateBusiness([FromBody] CreateBusinessDto request)
    {
        var userId = GetCurrentUserId();
        
        var address = new Address(request.Address.Street, request.Address.City, request.Address.State, request.Address.Zip);
        
        var business = Business.Create(
            userId,
            request.Name,
            request.Type,
            address,
            request.Phone,
            request.Website,
            request.Description ?? ""
        );

        await _businessRepo.AddAsync(business);
        await _businessRepo.SaveChangesAsync();

        var businessDto = new BusinessResponseDto
        {
            Id = business.Id,
            Name = business.Name,
            Type = business.Type,
            Address = new AddressDto
            {
                Street = business.Address.Street,
                City = business.Address.City,
                State = business.Address.State,
                Zip = business.Address.Zip
            },
            Phone = business.Phone,
            Website = business.Website
        };

        return CreatedAtAction(nameof(GetUserBusinesses), businessDto);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(userIdClaim!);
    }
}

public class BusinessResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string? Website { get; set; }
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
}

public class CreateBusinessDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
    public string Phone { get; set; } = string.Empty;
    public string? Website { get; set; }
    public string? Description { get; set; }
}
