//Elias Åsgård, 2026-10-29
using System.ComponentModel.DataAnnotations;

namespace CharacterService.Models
{
    public class Character
    {
        private int _currentHp;
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; } = "";
        public string? Species { get; set; } = "";
        public string? Weapon { get; set; }
        //public int WeaponDamage { get; set; }
        //public int WeaponDefence { get; set; }
        //public int WeaponAttackSpeed { get; set; }
        //public int WeaponDefenceSpeed { get; set; }
        [Required]
        public int MaxHp { get; set; } = 30;
        public int CurrentHp { get => _currentHp; set { _currentHp = Math.Max(MaxHp, _currentHp); } }
        public DateTime LastModified { get; set; }

    }
}
