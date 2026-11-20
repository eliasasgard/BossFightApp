//Elias Åsgård, 2026-10-29
using System.ComponentModel.DataAnnotations;

namespace CharacterWebsite.Models;

public record CharacterDto(int Id, string Name, string Species, string Weapon,
                       int MaxHp, int CurrentHp, DateTime LastModified, int? WeaponDamage);

public record WeaponDto(string Name, int Damage, int AttackSpeed, int Defence, int DefenceSpeed);
public record SpeciesDto(string Name);

public class DamageRequest
{
    [Range(1, 10000)] public int Amount { get; set; }
    [Required] public string Attacker { get; set; } = "";
}

public class CharacterForm
{
    [Required]
    public string Name { get; set; } = "";
    
    public string? Species { get; set; }
    
    [Required]
    public string? Weapon { get; set; }

    [Range(1, 100)]
    public int MaxHp { get; set; } = 50;
    public int CurrentHp { get; set; }
    public int WeaponDamage { get; set; }
    public int WeaponAttackSpeed { get; set; }
    public int WeaponDefence { get; set; }
    public int WeaponDefenceSpeed { get; set; }
    public DateTime LastModified;
}
