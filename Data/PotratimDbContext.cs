using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Potratim.Models;

namespace Potratim.Data
{
    public class PotratimDbContext : DbContext
    {
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<Game> Games { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;

        public PotratimDbContext(DbContextOptions<PotratimDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
            .HasMany(u => u.Games)
            .WithMany(g => g.Users)
            .UsingEntity(j => j.ToTable("UserToGame"));

            modelBuilder.Entity<User>()
            .HasMany(u => u.Reviews)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

            


            base.OnModelCreating(modelBuilder);
        }
    }
}