using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class PlayerQuest
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid QuestId { get; set; }
    
    public PlayerQuestState State { get; set; } = PlayerQuestState.Available;
    
    public string ProgressJson { get; set; } = "{}";
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Navigation properties
    public Player Player { get; set; } = null!;
    public Quest Quest { get; set; } = null!;
    
    // Helper properties
    public Dictionary<string, object> Progress
    {
        get => JsonSerializer.Deserialize<Dictionary<string, object>>(ProgressJson) ?? new Dictionary<string, object>();
        set => ProgressJson = JsonSerializer.Serialize(value);
    }
}

public enum PlayerQuestState
{
    Available = 0,
    Active = 1,
    Completed = 2,
    Failed = 3
}
