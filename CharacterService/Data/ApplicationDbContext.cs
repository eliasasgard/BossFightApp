using CharacterService.Models;
using Microsoft.EntityFrameworkCore;

namespace CharacterService.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<Character> Characters { get; set; }

    }
}
