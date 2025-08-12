using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Domain.Entities;

public class Leaderboard
{
    public Guid Id { get; set; }
    public Guid SeasonId { get; set; }
    public Guid PlayerId { get; set; }
    
    public int Rank { get; set; }
    public int MMRSnapshot { get; set; }
    
    // Navigation properties
    public Season Season { get; set; } = null!;
    public Player Player { get; set; } = null!;
}
