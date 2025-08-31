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
            // User
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

            //Game
            modelBuilder.Entity<Game>()
            .HasMany(r => r.Reviews)
            .WithOne(g => g.Game)
            .HasForeignKey(g => g.GameId);

            modelBuilder.Entity<Game>()
            .HasMany(g => g.Categories)
            .WithMany(c => c.Games)
            .UsingEntity(e => e.ToTable("CategoryToGame"));

            modelBuilder.Entity<Game>()
            .HasMany(g => g.Carts)
            .WithMany(c => c.Games)
            .UsingEntity(e => e.ToTable("CartToGame"));

            

            base.OnModelCreating(modelBuilder);
        }
    }
}