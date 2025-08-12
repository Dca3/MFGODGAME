using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class Quest
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    public QuestDifficulty Difficulty { get; set; } = QuestDifficulty.Easy;
    
    public string StoryJson { get; set; } = "{}";
    
    public string RewardsJson { get; set; } = "[]";
    
    public string RequirementsJson { get; set; } = "{}";
    
    [MaxLength(100)]
    public string Location { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string NpcName { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<PlayerQuest> PlayerQuests { get; set; } = new List<PlayerQuest>();
    
    // Helper properties
    public Dictionary<string, object> Story
    {
        get => JsonSerializer.Deserialize<Dictionary<string, object>>(StoryJson) ?? new Dictionary<string, object>();
        set => StoryJson = JsonSerializer.Serialize(value);
    }
    
    public List<QuestReward> Rewards
    {
        get => JsonSerializer.Deserialize<List<QuestReward>>(RewardsJson) ?? new List<QuestReward>();
        set => RewardsJson = JsonSerializer.Serialize(value);
    }
    
    public QuestRequirements Requirements
    {
        get => JsonSerializer.Deserialize<QuestRequirements>(RequirementsJson) ?? new QuestRequirements();
        set => RequirementsJson = JsonSerializer.Serialize(value);
    }
}

public enum QuestDifficulty
{
    Easy = 0,
    Medium = 1,
    Hard = 2,
    Legendary = 3
}

public class QuestReward
{
    public string Type { get; set; } = string.Empty; // money, experience, reputation, item
    public double Value { get; set; }
}

public class QuestRequirements
{
    public int? Level { get; set; }
    public int? Reputation { get; set; }
    public Dictionary<string, int>? Stats { get; set; }
}
