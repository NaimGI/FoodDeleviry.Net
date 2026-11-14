using FoodOrdersApi.Entities;
using FoodOrdersApi.Entities.Objects;
using FoodOrdersApi.Models.Address;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FoodOrdersApi.Controllers
{
    [Route("api/address")]
    [ApiController]
    [Authorize]
    public class AddressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AddressController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("mine")]
        public ActionResult<IEnumerable<AddressDto>> GetMine()
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var addresses = _context.Addresses
                .Where(a => a.AccountId == accountId)
                .OrderByDescending(a => a.IsDefault)
                .ThenBy(a => a.Id)
                .Select(a => ToDto(a))
                .ToList();
            return Ok(addresses);
        }

        [HttpPost]
        public ActionResult<AddressDto> Create([FromBody] SaveAddressDto dto)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var hasAddress = _context.Addresses.Any(a => a.AccountId == accountId);
            var address = new Address
            {
                AccountId = accountId,
                Label = dto.Label.Trim(),
                Country = dto.Country.Trim(),
                City = dto.City.Trim(),
                Street = dto.Street.Trim(),
                Building = dto.Building.Trim(),
                Premises = dto.Premises?.Trim(),
                IsDefault = dto.IsDefault || !hasAddress
            };

            if (address.IsDefault) ClearDefault(accountId);
            _context.Addresses.Add(address);
            _context.SaveChanges();
            return CreatedAtAction(
                nameof(GetMine), routeValues: null, value: ToDto(address));
        }

        [HttpPut("{id:int}")]
        public ActionResult<AddressDto> Update(int id, [FromBody] SaveAddressDto dto)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var address = _context.Addresses.FirstOrDefault(
                a => a.Id == id && a.AccountId == accountId);
            if (address == null) return NotFound();

            address.Label = dto.Label.Trim();
            address.Country = dto.Country.Trim();
            address.City = dto.City.Trim();
            address.Street = dto.Street.Trim();
            address.Building = dto.Building.Trim();
            address.Premises = dto.Premises?.Trim();
            if (dto.IsDefault) ClearDefault(accountId);
            address.IsDefault = dto.IsDefault;
            _context.SaveChanges();
            return Ok(ToDto(address));
        }

        [HttpPut("{id:int}/default")]
        public ActionResult SetDefault(int id)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var address = _context.Addresses.FirstOrDefault(
                a => a.Id == id && a.AccountId == accountId);
            if (address == null) return NotFound();

            ClearDefault(accountId);
            address.IsDefault = true;
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public ActionResult Delete(int id)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();

            var address = _context.Addresses.FirstOrDefault(
                a => a.Id == id && a.AccountId == accountId);
            if (address == null) return NotFound();

            var wasDefault = address.IsDefault;
            _context.Addresses.Remove(address);
            if (wasDefault)
            {
                var nextDefault = _context.Addresses
                    .Where(a => a.AccountId == accountId && a.Id != id)
                    .OrderBy(a => a.Id)
                    .FirstOrDefault();
                if (nextDefault != null) nextDefault.IsDefault = true;
            }
            _context.SaveChanges();
            return NoContent();
        }

        private void ClearDefault(int accountId)
        {
            var defaults = _context.Addresses
                .Where(a => a.AccountId == accountId && a.IsDefault);
            foreach (var address in defaults) address.IsDefault = false;
        }

        private static AddressDto ToDto(Address address) => new()
        {
            Id = address.Id,
            Label = address.Label,
            Country = address.Country,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            Premises = address.Premises,
            IsDefault = address.IsDefault
        };

        private bool TryGetAccountId(out int accountId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out accountId);
    }
}
