using FoodOrdersApi.Models.Account;
using FoodOrdersApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace FoodOrdersApi.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }


        [HttpPost("register")]
        public ActionResult Register([FromBody] RegisterDto dto)
        {
            _accountService.Register(dto);
            return Ok();
        }

        [HttpPost("login")]
        public ActionResult Login([FromBody] LoginDto dto)
        {
            string token = _accountService.GenerateToken(dto, HttpContext);

            return Ok(new { token = token });
        }

        [Authorize]
        [HttpGet("profile")]
        public ActionResult<AccountProfileDto> GetProfile()
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();
            var profile = _accountService.GetProfile(accountId);
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpPut("profile")]
        public ActionResult<AccountProfileDto> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();
            var profile = _accountService.UpdateProfile(accountId, dto);
            return profile == null ? NotFound() : Ok(profile);
        }

        [Authorize]
        [HttpPut("password")]
        public ActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (!TryGetAccountId(out var accountId)) return Unauthorized();
            return _accountService.ChangePassword(accountId, dto) ? NoContent() : NotFound();
        }

        private bool TryGetAccountId(out int accountId) =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out accountId);
    }
}
