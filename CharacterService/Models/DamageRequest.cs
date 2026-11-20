//Elias Åsgård, 2026-10-29
using System.ComponentModel.DataAnnotations;

namespace CharacterService.Models;

public class DamageRequest
{
    [Range(1, 1000)]
    [Required]
    public int Amount { get; set; }
    [Required]
    public string Attacker { get; set; } = string.Empty;
}
