//Elias Åsgård, 2026-11-13
using CharacterWebsite.Clients;
using CharacterWebsite.Models;
using CharacterWebsite.ViewModel;
using Microsoft.AspNetCore.Mvc;

namespace CharacterWebsite.Controllers;

/// <summary>
/// Controller to pair with the Character View. Sends requests to and fetches info from CharacterClient.
/// </summary>
/// <param name="characters"></param>
public class CharacterController(CharacterClient characters) : Controller
{
    [HttpGet]
    public async Task<IActionResult> List()
    {
        return View(await characters.ListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Weapons = await characters.WeaponsAsync();
        ViewBag.Species = await characters.SpeciesAsync();
        return View(new CharacterForm());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CharacterForm form)
    {
        if (ModelState.IsValid)
        {
            var result = await characters.CreateAsync(form);
            if (result.Ok) return RedirectToAction(nameof(List));
            AddErrors(result);
        }
        ViewBag.Weapons = await characters.WeaponsAsync();
        ViewBag.Species = await characters.SpeciesAsync();
        return View(form);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await characters.DeleteAsync(id);
        if (result.Ok) return RedirectToAction(nameof(List));
        AddErrors(result);
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        var character = await characters.GetAsync(id);
        if (character is null) return NotFound();

        return View(new CharacterDetails
        {
            Character = character,
            History = await characters.HistoryAsync(id)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var c = await characters.GetAsync(id);
        if (c is null) return NotFound();
        ViewBag.Weapons = await characters.WeaponsAsync();
        ViewBag.Species = await characters.SpeciesAsync();
        var character = new CharacterForm { Name = c.Name, Species = c.Species, Weapon = c.Weapon, MaxHp = c.MaxHp };
        return View(character);
    }


    private void AddErrors(ApiResult r)
    {
        foreach (var (field, messages) in r.Errors ?? new Dictionary<string, string[]>())
            foreach (var m in messages) ModelState.AddModelError(field, m);
    }
}
