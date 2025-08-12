using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;
    
    public int Level { get; set; } = 1;
    public long Experience { get; set; } = 0;
    public long Money { get; set; } = 1000;
    public int Reputation { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public PlayerStats Stats { get; set; } = null!;
    public ICollection<PlayerInventory> Inventory { get; set; } = new List<PlayerInventory>();
    public ICollection<PlayerQuest> Quests { get; set; } = new List<PlayerQuest>();
    public ICollection<Duel> DuelsAsPlayer1 { get; set; } = new List<Duel>();
    public ICollection<Duel> DuelsAsPlayer2 { get; set; } = new List<Duel>();
    public Rating Rating { get; set; } = null!;
    public ICollection<Leaderboard> Leaderboards { get; set; } = new List<Leaderboard>();
}
