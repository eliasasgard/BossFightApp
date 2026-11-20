//Elias Åsgård, 2026-10-29
using System.ComponentModel.DataAnnotations;

namespace CharacterService.Models;

// What the API returns
public record CharacterDto(
    int Id,
    string Name,
    string Species,
    string Weapon,
    int MaxHp,
    int CurrentHp,
    DateTime LastModified);

// What the API accepts for POST and PUT
public class CharacterRequest
{
    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    // Must be nullable: with nullable reference types on, a non-nullable
    // string is treated as [Required] by [ApiController] validation.
    [StringLength(100)]
    public string? Species { get; set; }

    [Required]
    public string Weapon { get; set; } = "";

    [Range(1, 1000)]
    public int MaxHp { get; set; }
}