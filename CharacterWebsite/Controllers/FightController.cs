using System.Collections.Concurrent;
using CharacterWebsite.Clients;
using CharacterWebsite.Models;
using Microsoft.AspNetCore.Mvc;

namespace CharacterWebsite.Controllers;

/// <summary>
/// Controller to pair with the Fight Views. Uses CharacterClient for requests.
/// </summary>
/// <param name="characters"></param>
public class FightController(CharacterClient characters) : Controller
{
    private const int BossId = 0;

    // Guard is when you Defend
    private static readonly ConcurrentDictionary<int, bool> Guards = new();


    /// <summary>
    /// Async GET: Gets a view for a new fight instance
    /// Example: /Fight/Index/1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {
        if (id == BossId)                                   // also what a missing id binds to
            return RedirectToAction("List", "Character");

        var fight = await LoadFightAsync(id);
        if (fight is null) return NotFound("Character or The Boss (id 0) was not found.");
        return View(fight);
    }

    // GET /Fight/State/3 : refresh, to see damage dealt by other players
    //[HttpGet]
    //public Task<IActionResult> State(int id) => StateAsync(id);

    /// <summary>
    /// Async POST to attack the boss
    /// Example: /Fight/Attack/1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Attack(int id)
    {
        var player = await characters.GetAsync(id);
        if (player is null) return NotFound();

        if (player.CurrentHp > 0)                           // the dead don't fight
        {
            var weapons = await characters.WeaponsAsync();
            var weapon = weapons.FirstOrDefault(w => w.Name == player.Weapon);
            if (weapon is null) return Problem("Could not find the player's weapon.");

            // CharacterService saves the damage and writes the log entry. It ignores
            // hits on a character who is already dead.
            await characters.DamageAsync(BossId, weapon.Damage, player.Name);
        }

        return await StateAsync(id);
    }

    
    /// <summary>
    /// Async POST to get attacked by the boss
    /// Example: /Fight/BossAttack/1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> BossAttack(int id)
    {
        var player = await characters.GetAsync(id);
        var boss = await characters.GetAsync(BossId);
        if (player is null || boss is null) return NotFound();

        if (player.CurrentHp > 0 && boss.CurrentHp > 0)
        {
            var weapons = await characters.WeaponsAsync();
            var playerWeapon = weapons.FirstOrDefault(w => w.Name == player.Weapon);
            var bossWeapon = weapons.FirstOrDefault(w => w.Name == boss.Weapon);
            if (playerWeapon is null || bossWeapon is null)
                return Problem("Could not find the weapon of the player or of the boss.");

            var damage = bossWeapon.Damage;
            if (Guards.TryRemove(id, out _)) // a raised guard absorbs this one hit
                damage = Math.Max(1, damage - playerWeapon.Defence);

            await characters.DamageAsync(id, damage, boss.Name);
        }

        return await StateAsync(id);
    }

    
    /// <summary>
    /// Async POST to Defend against an attack
    /// Example: /Fight/Defend/1
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> Defend(int id)
    {
        Guards[id] = true;
        return await StateAsync(id);
    }

    /// <summary>
    /// Async method to get the current state of a character
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<IActionResult> StateAsync(int id)
    {
        var fight = await LoadFightAsync(id);
        if (fight is null) return NotFound();
        return Json(fight);
    }

    /// <summary>
    /// Async method to create a new fight between a given character and THE BOSS
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private async Task<Fight?> LoadFightAsync(int id)
    {
        var player = await characters.GetAsync(id);
        var boss = await characters.GetAsync(BossId);
        if (player is null || boss is null) return null;

        return new Fight
        {
            Player = player,
            Boss = boss,
            Guarding = Guards.ContainsKey(id),
            Log = await characters.FightLogAsync(id, BossId)
        };
    }
}