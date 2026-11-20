using CharacterWebsite.Models;

namespace CharacterWebsite.ViewModel;

/// <summary>
/// Used for the Details view of a character
/// </summary>
public class CharacterDetails
{
    public required CharacterDto Character { get; init; }
    public List<HistoryEntryDto> History { get; init; } = [];
}
