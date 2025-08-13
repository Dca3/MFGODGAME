using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class PlayerInventory
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid ItemDefinitionId { get; set; }
    
    public bool IsEquipped { get; set; } = false;
    
    public string RollDataJson { get; set; } = "{}";
    
    // Navigation properties
    public Player Player { get; set; } = null!;
    public ItemDefinition ItemDefinition { get; set; } = null!;
    
    // Helper properties
    public Dictionary<string, object> RollData
    {
        get => JsonSerializer.Deserialize<Dictionary<string, object>>(RollDataJson) ?? new Dictionary<string, object>();
        set => RollDataJson = JsonSerializer.Serialize(value);
    }
}
