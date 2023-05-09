using Microsoft.EntityFrameworkCore;
using VK_ASP_NET_API.Models;

namespace VK_ASP_NET_API.Data
{
    public class VK_ASP_NET_APIDbContext : DbContext
    {
        public VK_ASP_NET_APIDbContext(DbContextOptions<VK_ASP_NET_APIDbContext> options) : base(options)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<UserState> UserStates { get; set; }
    }
}
