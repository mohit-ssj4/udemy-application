using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using backend_api.Data;
using backend_api.DTOs;
using backend_api.Entities;
using backend_api.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend_api.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly ITokenService _tokenService;
        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context = context;
        }

        //Method to register a new user
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            //Check if the user already exists or not
            if (await UserExists(registerDto.UserName)) return BadRequest("Username already exists");

            //Used to generate the PasswordHash and PasswordSalt for security
            using var hmac = new HMACSHA512();

            //Creating the object of AppUser
            var user = new AppUser
            {
                UserName = registerDto.UserName.ToLower(),
                PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt = hmac.Key
            };

            //Tracks the new changes to be made to the DB
            _context.Users.Add(user);

            //Saves the changes into the DB
            await _context.SaveChangesAsync();

            return new UserDto {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        //Method to login
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            //Finds the user from the DB
            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == loginDto.UserName);

            //Checks if the user is not present then return 401 error
            if (user == null) return Unauthorized("Invalid Username");

            //Computing the hash using the salt key for security
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i = 0; i < computedHash.Length; i++)
            {
                //If the computed hash and the user's hash are not equal then return 401 error
                if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid Password");
            }

            return new UserDto
            {
                Username = user.UserName,
                Token = _tokenService.CreateToken(user)
            };
        }

        //Method to check if the user already exists or not
        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x => x.UserName == username.ToLower());
        }
    }
}
