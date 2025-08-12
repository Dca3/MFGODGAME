using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Domain.Entities;

public class Rating
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    
    public int MMR { get; set; } = 1200;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    
    public DateTime? LastMatchAt { get; set; }
    
    // Navigation property
    public Player Player { get; set; } = null!;
    
    // Helper properties
    public double WinRate => Wins + Losses > 0 ? (double)Wins / (Wins + Losses) * 100 : 0;
}
