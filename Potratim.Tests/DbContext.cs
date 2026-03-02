using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Potratim.Data;

namespace Potratim.Tests
{
    public static class DbContext
    {
        public static PotratimDbContext CreateInMemoryDbContext(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<PotratimDbContext>()
                .UseInMemoryDatabase("TestDataBase_" + Guid.NewGuid().ToString())
                .Options;
            return new PotratimDbContext(options);
        }

        public static void Dispose(PotratimDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}