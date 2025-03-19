using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(DataContext context, ITokenService tokenService ) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDTO>> Register(RegisterDTO registerDTO){

        if (await UserExistis(registerDTO.Username)) return BadRequest("Username is already in use");

        using var hmac = new HMACSHA512();
        var user = new AppUser{
            UserName = registerDTO.Username.ToLower(),
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDTO.Password)),
            PasswordHashSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new UserDTO{
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }

    private async Task<bool> UserExistis(string username) {
        return await context.Users.AnyAsync(X => X.UserName.ToLower() == username.ToLower());
    }


    [HttpPost("login")]
    public async Task<ActionResult<UserDTO>> Login(LoginDTO loginDTO){
        var user = await context.Users.FirstOrDefaultAsync(x => 
        x.UserName == loginDTO.Username.ToLower());

        if (user ==  null) return Unauthorized("Invalid username or password");

        using var hmac = new HMACSHA512(user.PasswordHashSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDTO.Password));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) return Unauthorized("Invalid password");  
        }

        return new UserDTO{
            Username = user.UserName,
            Token = tokenService.CreateToken(user)
        };
    }
}
