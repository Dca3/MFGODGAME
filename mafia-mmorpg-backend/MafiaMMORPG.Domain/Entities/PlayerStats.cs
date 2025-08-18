using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Domain.Entities;

public class PlayerStats
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    
    public int Karizma { get; set; } = 5;
    public int Guc { get; set; } = 5;
    public int Zeka { get; set; } = 5;
    public int Hayat { get; set; } = 5;
    public int FreePoints { get; set; } = 0;
    
    // Navigation property
    public Player Player { get; set; } = null!;
}
