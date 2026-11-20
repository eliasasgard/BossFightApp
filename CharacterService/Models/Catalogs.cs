//Elias Åsgård, 2026-10-29

namespace CharacterService.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Damage">How much damage one hits deals</param>
    /// <param name="AttackSpeed">How many times per minute you can attack</param>
    /// <param name="Defence">How much damage you reduce when defending</param>
    /// <param name="DefenceSpeed">How many times per minute you can defend</param>
    public record WeaponInfo(string Name, int Damage, int AttackSpeed, int Defence, int DefenceSpeed);

    public static class WeaponCatalog
    {
        public static readonly IReadOnlyList<WeaponInfo> All = new[]
        {
            new WeaponInfo("Sword", 30, 50, 10, 30),
            new WeaponInfo("Hammer", 20, 40, 5, 30),
            new WeaponInfo("Knife", 40, 60, 4, 40),
            new WeaponInfo("Club", 15, 30, 8, 10),
            new WeaponInfo("Axe", 20, 40, 10, 20)
        };

        public static WeaponInfo? Find(string? name) =>
            All.FirstOrDefault(w => w.Name == name);
    }

    public static class SpeciesCatalog
    {
        public static readonly IReadOnlyList<string> All = new[]
        {
            "Human",
            "Elf",
            "Dwarf",
            "Orc",
            "Goblin",
            "Neanderthal"
        };
    }
}
