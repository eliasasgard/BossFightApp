namespace CharacterWebsite.Models;

// Everything the Fight page needs
public class Fight
{
    public required CharacterDto Player { get; init; }
    public required CharacterDto Boss { get; init; }
    public bool Guarding { get; init; }
    public List<HistoryEntryDto> Log { get; init; } = [];

    public bool PlayerDead => Player.CurrentHp <= 0;
    public bool BossDead => Boss.CurrentHp <= 0;
    public bool IsOver => PlayerDead || BossDead;
}

public record HistoryEntryDto(int Id, int CharacterId, string Field, string? OldValue,
                              string? NewValue, string Description, DateTime TimeChanged);