using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace MafiaMMORPG.Domain.Entities;

public class Duel
{
    public Guid Id { get; set; }
    public Guid Player1Id { get; set; }
    public Guid Player2Id { get; set; }
    
    public int CurrentTurn { get; set; } = 1;
    public int MaxTurns { get; set; } = 10;
    
    public double Player1Hp { get; set; } = 100;
    public double Player2Hp { get; set; } = 100;
    public double Player1MaxHp { get; set; } = 100;
    public double Player2MaxHp { get; set; } = 100;
    
    public string ActionsJson { get; set; } = "[]";
    
    public DuelStatus Status { get; set; } = DuelStatus.Waiting;
    
    public Guid? WinnerId { get; set; }
    
    public string LogJson { get; set; } = "[]";
    
    public string Seed { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    
    // Navigation properties
    public Player Player1 { get; set; } = null!;
    public Player Player2 { get; set; } = null!;
    public Player? Winner { get; set; }
    
    // Helper properties
    public List<DuelAction> Actions
    {
        get => JsonSerializer.Deserialize<List<DuelAction>>(ActionsJson) ?? new List<DuelAction>();
        set => ActionsJson = JsonSerializer.Serialize(value);
    }
    
    public List<string> Log
    {
        get => JsonSerializer.Deserialize<List<string>>(LogJson) ?? new List<string>();
        set => LogJson = JsonSerializer.Serialize(value);
    }
}

public enum DuelStatus
{
    Waiting = 0,
    Active = 1,
    Finished = 2
}

public class DuelAction
{
    public string Type { get; set; } = string.Empty; // attack, defend, special
    public string? Target { get; set; }
    public Dictionary<string, object>? Data { get; set; }
}
