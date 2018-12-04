using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using GovUk.Education.DataDissemination.Meta.Api.Models;

namespace GovUk.Education.DataDissemination.Meta.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Theme> Themes { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Publication> Publications { get; set; }
    }
}