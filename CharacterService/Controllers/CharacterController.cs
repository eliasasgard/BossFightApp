// Elias Åsgård, 2026-10-29
using CharacterService.Data;
using CharacterService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CharacterService.Controllers
{
    /// <summary>
    /// Data Transfer Object to set the structure of data sent to the CharactersController class
    /// </summary>
    public record CharacterDto( int Id, string Name, string Species, string Weapon,
                                int MaxHp, int CurrentHp, DateTime LastModified );


    /// <summary>
    /// View-less controller using a database connection to the Characters database
    /// and a client to HistoryService. Routes start with 'characters'
    /// </summary>
    /// <param name="db"></param>
    /// <param name="history"></param>
    [ApiController]
    [Route("characters")]
    public class CharactersController(ApplicationDbContext db, HistoryClient history) : ControllerBase
    {
        /// <summary>
        /// GET: Default route, lists all characters from the Characters database. Async method.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<List<CharacterDto>>> List()
        {
            var characters = await db.Characters.AsNoTracking().OrderBy(c => c.Id).ToListAsync();
            return characters.Select(ToDto).ToList();
        }

        /// <summary>
        /// GET: characters/id to get info about a specific character. Async method.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<CharacterDto>> Get(int id)
        {
            var character = await db.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (character is null) return NotFound();
            return ToDto(character);
        }

        /// <summary>
        /// GET: Lists all weapons. Routes to /weapons instead of /characters. Async method.
        /// </summary>
        /// <returns></returns>
        [HttpGet("/weapons")]
        public IActionResult Weapons()
        {
            return Ok(WeaponCatalog.All);
        }

        [HttpGet("/species")]
        public IActionResult Species()
        {
            return Ok(SpeciesCatalog.All);
        }

        /// <summary>
        /// Async POST: Creates a character (if the request is valid) and saves it to the database.
        /// Also adds an entry to the HistoryService.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<CharacterDto>> Create(CharacterRequest request)
        {
            // Find weapon
            var weapon = WeaponCatalog.Find(request.Weapon);
            if (weapon is null)
            {
                ModelState.AddModelError(nameof(request.Weapon), "Unknown weapon.");
                return ValidationProblem(ModelState);
            }

            var character = new Character
            {
                Name = request.Name,
                Species = request.Species ?? "",
                Weapon = weapon.Name,
                MaxHp = request.MaxHp,
                CurrentHp = request.MaxHp,
                LastModified = DateTime.UtcNow
            };

            db.Characters.Add(character);
            await db.SaveChangesAsync();

            // The Id exists only after saving, so history is recorded afterwards.
            await history.RecordAsync(character.Id, "Created", null, character.Name);

            return CreatedAtAction(nameof(Get), new { id = character.Id }, ToDto(character));
        }

        /// <summary>
        /// Async PUT: Updates a specified character given a valid request. Saves changes to the 
        /// Characters database and sends a request to HistoryService.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, CharacterRequest request)
        {
            var character = await db.Characters.FindAsync(id);
            if (character is null) return NotFound();

            var weapon = WeaponCatalog.Find(request.Weapon);
            if (weapon is null)
            {
                ModelState.AddModelError(nameof(request.Weapon), "Unknown weapon.");
                return ValidationProblem(ModelState);
            }

            // Compare old and new values before overwriting
            var changes = new List<(string Field, string? OldValue, string? NewValue)>();
            void Track(string field, object? oldValue, object? newValue)
            {
                if (!Equals(oldValue, newValue))
                    changes.Add((field, oldValue?.ToString(), newValue?.ToString()));
            }

            Track(nameof(Character.Name), character.Name, request.Name);
            Track(nameof(Character.Species), character.Species, request.Species);
            Track(nameof(Character.Weapon), character.Weapon, weapon.Name);
            //Track(nameof(Character.Damage), character.Damage, weapon.Damage);
            Track(nameof(Character.MaxHp), character.MaxHp, request.MaxHp);

            if (changes.Count == 0) return NoContent();

            character.Name = request.Name ?? "";
            character.Species = request.Species ?? "";
            //character.Weapon = weapon.Name;
            //character.Damage = weapon.Damage;
            character.MaxHp = request.MaxHp;
            character.LastModified = DateTime.UtcNow;
            await db.SaveChangesAsync();

            foreach (var change in changes)
                await history.RecordAsync(id, change.Field, change.OldValue, change.NewValue);

            return NoContent();
        }

        /// <summary>
        /// Async DELETE: Deletes a character from the database and sends a request to HistoryService.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var character = await db.Characters.FindAsync(id);
            if (character is null) return NotFound();

            db.Characters.Remove(character);
            await db.SaveChangesAsync();

            await history.RecordAsync(id, "Deleted", character.Name, null);
            return NoContent();
        }

        /// <summary>
        /// Async POST: Changes the HP of a character
        /// </summary>
        /// <param name="id"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("{id:int}/damage")]
        public async Task<ActionResult<CharacterDto>> Damage(int id, DamageRequest request)
        {
            var before = await db.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (before is null) return NotFound();

            // One update so simultaneous hits from different players all count
            // Skip characters at 0 HP
            var rows = await db.Characters
                .Where(c => c.Id == id && c.CurrentHp > 0)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.CurrentHp, c => Math.Max(0, c.CurrentHp - request.Amount))
                    .SetProperty(c => c.LastModified, DateTime.UtcNow));

            var after = await db.Characters.AsNoTracking().FirstAsync(c => c.Id == id);

            if (rows > 0)
            {
                var text = $"{request.Attacker} dealt {request.Amount} damage to {after.Name}! " +
                           $"{after.Name} now has {after.CurrentHp} HP.";
                if (after.CurrentHp <= 0) text += $" {after.Name} has died.";

                await history.RecordAsync(id, nameof(Character.CurrentHp),
                                          before.CurrentHp.ToString(), after.CurrentHp.ToString(), text);
            }
            return ToDto(after);
        }


        /// <summary>
        /// Async GET. Gets history of one or more characters given earliest time and row limit
        /// Example: // GET /characters/history?ids=3&ids=0&field=CurrentHp&from=2026-11-12T18:12:11Z39182300&limit=20
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="field"></param>
        /// <param name="from"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet("history")]
        public async Task<IActionResult> History([FromQuery] int[] ids, string? field, DateTime? from=null, int limit = 200) =>
            Ok(await history.GetManyAsync(ids, field, from, limit));

        /// <summary>
        /// Converts a character to a Character Data Transfer Object
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static CharacterDto ToDto(Character c) =>
            new(c.Id, c.Name, c.Species, c.Weapon, c.MaxHp, c.CurrentHp, c.LastModified);
    }
}
