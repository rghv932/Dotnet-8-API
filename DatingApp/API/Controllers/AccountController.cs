using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await UserExists(registerDto.Username)) return BadRequest("Username is taken");
        return Ok();
        //var user = _mapper.Map<AppUser>(registerDto);
        //using var hmac = new HMACSHA512();

        //var user = new AppUser
        //{
        //    UserName = registerDto.Username.ToLower(),
        //    PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
        //    PasswordSalt = hmac.Key
        //};

        //context.Users.Add(user);
        //await context.SaveChangesAsync();

        ////return user;


        ////user.UserName = registerDto.Username.ToLower();
        ////user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password));
        ////user.PasswordSalt = hmac.Key;


        ////_context.Users.Add(user);
        ////await _context.SaveChangesAsync();

        //return new UserDto
        //{
        //    Username = user.UserName,
        //    Token = tokenService.CreateToken(user)
        //};
        //KnownAs = user.KnownAs,
        //Gender = user.Gender
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await context.Users
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(x=> x.UserName == loginDto.Username.ToLower());
        //var user = await _context.Users
        //    .Include(p => p.Photos)
        //    .SingleOrDefaultAsync(x => x.UserName == loginDto.Username);

        if (user == null) return Unauthorized("Invalid username");

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");
        }

        //return user;

        return new UserDto
        {
            Username = user.UserName,
            Token = tokenService.CreateToken(user),
            PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url
        };
        //
        //    KnownAs = user.KnownAs,
        //    Gender = user.Gender
    }


    private async Task<bool> UserExists(string username)
    {
        return await context.Users.AnyAsync(x => x.UserName.ToLower() == username.ToLower());
    }
}
