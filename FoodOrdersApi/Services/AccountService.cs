using FoodOrdersApi.Entities;
using FoodOrdersApi.Entities.Objects;
using FoodOrdersApi.Models.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FoodOrdersApi.Exceptions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace FoodOrdersApi.Services
{
    public interface IAccountService
    {
        void Register(RegisterDto dto);
        string GenerateToken(LoginDto dto, HttpContext httpContext);
        AccountProfileDto? GetProfile(int accountId);
        AccountProfileDto? UpdateProfile(int accountId, UpdateProfileDto dto);
        bool ChangePassword(int accountId, ChangePasswordDto dto);
    }

    public class AccountService : IAccountService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<Account> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;

        public AccountService(AppDbContext context, IPasswordHasher<Account> passwordHasher, AuthenticationSettings authenticationSettings)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
        }



        public void Register(RegisterDto dto)
        {
            var newAccount = new Account()
            {
                Login = dto.Login
            };

            var hashedPassword = _passwordHasher.HashPassword(newAccount, dto.Password);

            newAccount.HashedPassword = hashedPassword;

            _context.Accounts.Add(newAccount);
            _context.SaveChanges();
        }

        public string GenerateToken(LoginDto dto, HttpContext httpContext)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Login == dto.Login);

            if (account is null)
            {
                throw new BadRequestException("Login or password is incorrect");
            }

            var hashedPassword = _passwordHasher.VerifyHashedPassword(account, account.HashedPassword, dto.Password);
            if(hashedPassword == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Login or password is incorrect");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{account.Login}")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(
                _authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenString = tokenHandler.WriteToken(token);

            httpContext.Response.Cookies.Append("jwtToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });

            return tokenString;
        }

        public AccountProfileDto? GetProfile(int accountId)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);
            return account == null
                ? null
                : new AccountProfileDto
                {
                    Id = account.Id,
                    Login = account.Login,
                    TotalSpent = GetTotalSpent(accountId)
                };
        }

        public AccountProfileDto? UpdateProfile(int accountId, UpdateProfileDto dto)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return null;

            var login = dto.Login.Trim();
            if (login.Length < 6)
            {
                throw new BadRequestException("Username must be at least 6 characters");
            }

            if (string.Equals(account.Login, "administrator", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(login, "administrator", StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("The administrator username cannot be changed or reused");
            }

            if (_context.Accounts.Any(a =>
                    a.Id != accountId && a.Login.ToLower() == login.ToLower()))
            {
                throw new BadRequestException("That username is already in use");
            }

            account.Login = login;
            _context.SaveChanges();
            return new AccountProfileDto
            {
                Id = account.Id,
                Login = account.Login,
                TotalSpent = GetTotalSpent(accountId)
            };
        }

        private double GetTotalSpent(int accountId) =>
            _context.Orders
                .Where(order => order.AccountId == accountId)
                .Sum(order => (double?)order.TotalPrice) ?? 0;

        public bool ChangePassword(int accountId, ChangePasswordDto dto)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Id == accountId);
            if (account == null) return false;

            var verification = _passwordHasher.VerifyHashedPassword(
                account, account.HashedPassword, dto.CurrentPassword);
            if (verification == PasswordVerificationResult.Failed)
            {
                throw new BadRequestException("Current password is incorrect");
            }

            account.HashedPassword = _passwordHasher.HashPassword(account, dto.NewPassword);
            _context.SaveChanges();
            return true;
        }
    }
}
