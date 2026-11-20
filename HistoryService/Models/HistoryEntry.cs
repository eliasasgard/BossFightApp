namespace HistoryService.Models
{
    public class HistoryEntry
    {
        public int Id { get; set; }
        public int CharacterId { get; set; }
        public string Field { get; set; } = "";
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public DateTime TimeChanged { get; set; }
        public string Description { get; set; } = "";
    }

    public record HistoryEntryRequest(int CharacterId, string Field, string? OldValue, string? NewValue, string? Description);

}
