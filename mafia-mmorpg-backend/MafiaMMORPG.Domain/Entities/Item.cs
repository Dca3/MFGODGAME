using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class Item
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Slot { get; set; } = string.Empty;
    
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    
    public int? BaseWeaponDamage { get; set; }
    
    public string TagsJson { get; set; } = "[]";
    
    public string ImageUrl { get; set; } = string.Empty;
    
    // Navigation properties
    public ICollection<ItemAffix> Affixes { get; set; } = new List<ItemAffix>();
    public ICollection<PlayerInventory> PlayerInventories { get; set; } = new List<PlayerInventory>();
    
    // Helper properties
    public List<string> Tags
    {
        get => JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
        set => TagsJson = JsonSerializer.Serialize(value);
    }
}

public enum ItemRarity
{
    Common = 0,
    Rare = 1,
    Epic = 2,
    Legendary = 3
}
