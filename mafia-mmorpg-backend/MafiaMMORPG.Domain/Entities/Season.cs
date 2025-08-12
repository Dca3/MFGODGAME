using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class Season
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    public SeasonStatus Status { get; set; } = SeasonStatus.Active;
    
    public string RewardsJson { get; set; } = "{}";
    
    // Navigation properties
    public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
    
    // Helper properties
    public SeasonRewards Rewards
    {
        get => JsonSerializer.Deserialize<SeasonRewards>(RewardsJson) ?? new SeasonRewards();
        set => RewardsJson = JsonSerializer.Serialize(value);
    }
}

public enum SeasonStatus
{
    Active = 0,
    Ended = 1
}

public class SeasonRewards
{
    public List<string> Top1000 { get; set; } = new List<string>();
    public List<string> Top100 { get; set; } = new List<string>();
    public List<string> Top10 { get; set; } = new List<string>();
}
