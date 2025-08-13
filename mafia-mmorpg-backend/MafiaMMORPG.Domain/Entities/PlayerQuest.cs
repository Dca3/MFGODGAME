using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using MafiaMMORPG.Domain.ValueObjects;

namespace MafiaMMORPG.Domain.Entities;

public class PlayerQuest
{
    public Guid Id { get; set; }
    public Guid PlayerId { get; set; }
    public Guid QuestId { get; set; }
    
    public PlayerQuestState State { get; set; } = PlayerQuestState.Available;
    
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Cooldown tracking
    public DateTime? CooldownUntil { get; set; }

    // Navigation properties
    public Player Player { get; set; } = null!;
    public Quest Quest { get; set; } = null!;
    
    // ÖNEMLİ: Tipli koleksiyon yap
    public List<QuestProgress> Progress { get; set; } = new();

    // Helper methods
    public bool IsOnCooldown => CooldownUntil.HasValue && CooldownUntil.Value > DateTime.UtcNow;
    public bool IsActive => State == PlayerQuestState.Active && StartedAt.HasValue;
    public bool CanStart => State == PlayerQuestState.Available && !IsOnCooldown;
}

public enum PlayerQuestState
{
    Available = 0,
    Active = 1,
    Completed = 2,
    Failed = 3
}
