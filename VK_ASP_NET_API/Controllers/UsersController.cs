using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VK_ASP_NET_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Query;
using VK_ASP_NET_API.Data;

namespace VK_ASP_NET_API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly VK_ASP_NET_APIDbContext _context;

        public UsersController(VK_ASP_NET_APIDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll(int page = 1, int pageSize = 10)
        {
            var users = await _context.Users.Include(u => u.UserGroup).Include(u => u.UserState)
                .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(int id)
        {
            var user = await _context.Users.Include(u => u.UserGroup).Include(u => u.UserState).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User userModel)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == userModel.Login);
            if (existingUser != null)
            {
                return BadRequest("User with this login already exists");
            }

            var adminExists = await _context.Users.AnyAsync(u => u.UserGroup.Code == "ADMIN");
            if (userModel.UserGroupId == 1 && adminExists)
            {
                return BadRequest("There can be only one Admin user");
            }

            var userGroup = await _context.UserGroups.AnyAsync(g => g.Id == userModel.UserGroupId);
            if (!userGroup)
            {
                return BadRequest("Invalid user group");
            }

            var userState = await _context.UserStates.AnyAsync(s => s.Id == userModel.UserStateId);
            if (!userState)
            {
                return BadRequest("Invalid user state");
            }

            var newUser = new User
            {
                Login = userModel.Login,
                Password = userModel.Password,
                CreatedDate = DateTime.UtcNow,
                UserGroupId = userModel.UserGroupId,
                UserStateId = 1
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("User successfully added");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            if (user.UserGroupId == 1)
            {
                return BadRequest("You can't delete Admin user");
            }

            user.UserStateId = 2; // Blocked state
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}