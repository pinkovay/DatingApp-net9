using System;
using API.Data;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")] // https://localhost:5000/api/users
public class UsersController(DataContext context) : ControllerBase
{


    // IEnumerable serve para fazer referencia a seleção de um tipo... algo parecido com List<AppUser> faria em outras linguagens
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppUser>>> GetUsers(){
        var users = await context.Users.ToListAsync();
        return users;
    }

    
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AppUser>> GetUsers(int id){
        var user = await context.Users.FindAsync(id);

        if (user == null) return NotFound();

        return user;
    }


}
