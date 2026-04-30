using Microsoft.EntityFrameworkCore;
using FartmaalerAPI.Models;

namespace FartmaalerAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Measurement> Measurements { get; set; }
    }
}