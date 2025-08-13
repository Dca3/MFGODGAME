using System.ComponentModel.DataAnnotations;
using MafiaMMORPG.Domain.Enums;

namespace MafiaMMORPG.Domain.Entities;

public class ItemDefinition
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public ItemSlot Slot { get; set; }
    public ItemRarity Rarity { get; set; }
    public int ItemLevel { get; set; }
    public int RequiredLevel { get; set; }
    
    // Base stats (optional)
    public int? BaseK { get; set; }
    public int? BaseG { get; set; }
    public int? BaseZ { get; set; }
    public int? BaseH { get; set; }
    
    // Affix data (JSON)
    public string? AffixJson { get; set; }
    
    // Navigation properties
    public ICollection<PlayerInventory> PlayerInventories { get; set; } = new List<PlayerInventory>();
}
