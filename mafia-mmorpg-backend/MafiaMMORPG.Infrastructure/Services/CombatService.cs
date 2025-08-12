using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.DTOs;
using MafiaMMORPG.Application.Configuration;
using MafiaMMORPG.Domain.Entities;
using MafiaMMORPG.Application.Repositories;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Services;

public class CombatService : ICombatService
{
    private readonly IStatFormulaService _statFormulaService;
    private readonly IRepository<Player> _playerRepo;
    private readonly IRepository<PlayerInventory> _inventoryRepo;
    private readonly IRepository<Duel> _duelRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BalanceOptions _balance;
    private readonly ILogger<CombatService> _logger;

    public CombatService(
        IStatFormulaService statFormulaService,
        IRepository<Player> playerRepo,
        IRepository<PlayerInventory> inventoryRepo,
        IRepository<Duel> duelRepo,
        IUnitOfWork unitOfWork,
        IOptions<BalanceOptions> balanceOptions,
        ILogger<CombatService> logger)
    {
        _statFormulaService = statFormulaService;
        _playerRepo = playerRepo;
        _inventoryRepo = inventoryRepo;
        _duelRepo = duelRepo;
        _unitOfWork = unitOfWork;
        _balance = balanceOptions.Value;
        _logger = logger;
    }

    public CombatResult SimulatePve(CombatRequest req)
    {
        // TODO: Implement PvE combat logic
        throw new NotImplementedException("PvE combat not implemented yet");
    }

    public CombatResult SimulatePvp(CombatRequest req)
    {
        var seed = GenerateSeed(req.DuelId);
        var random = new Random(seed.GetHashCode());
        
        var attacker = LoadPlayerWithStats(req.AttackerId);
        var defender = LoadPlayerWithStats(req.DefenderId);
        
        var attackerStats = CalculateEffectiveStats(attacker);
        var defenderStats = CalculateEffectiveStats(defender);
        
        var log = new List<string>();
        var snapshots = new List<DuelSnapshot>();
        
        var attackerHp = attackerStats.TotalHp;
        var defenderHp = defenderStats.TotalHp;
        var maxTurns = 100; // Deadlock önleme
        
        for (int turn = 1; turn <= maxTurns; turn++)
        {
            // Attacker attacks defender
            var damage = CalculateDamage(attackerStats, defenderStats, random);
            defenderHp = Math.Max(0, defenderHp - damage);
            
            var logLine = $"Turn {turn}: {attacker.Username} deals {damage:F1} damage to {defender.Username}";
            log.Add(logLine);
            
            // Life steal
            var lifeSteal = _statFormulaService.CalcLifeSteal(damage, attackerStats.LifeSteal);
            attackerHp = Math.Min(attackerStats.TotalHp, attackerHp + lifeSteal);
            
            if (lifeSteal > 0)
                log.Add($"  {attacker.Username} steals {lifeSteal:F1} HP");
            
            // Create snapshot
            snapshots.Add(new DuelSnapshot(req.DuelId ?? Guid.NewGuid(), turn, attackerHp, defenderHp, logLine));
            
            // Check if defender is dead
            if (defenderHp <= 0)
            {
                log.Add($"{defender.Username} is defeated!");
                break;
            }
            
            // Swap roles for next turn
            (attacker, defender) = (defender, attacker);
            (attackerHp, defenderHp) = (defenderHp, attackerHp);
            (attackerStats, defenderStats) = (defenderStats, attackerStats);
        }
        
        // Determine winner
        var attackerWon = defenderHp <= 0;
        if (!attackerWon && maxTurns >= 100)
        {
            // Tie-breaker: higher remaining HP wins
            if (attackerHp > defenderHp)
                attackerWon = true;
            else if (attackerHp < defenderHp)
                attackerWon = false;
            else
                attackerWon = true; // Attacker wins in case of tie
        }
        
        // Save duel result
        SaveDuelResult(req, attackerWon, log, snapshots, seed);
        
        return new CombatResult(
            req.AttackerId,
            req.DefenderId,
            attackerWon ? attackerHp : 0,
            attackerWon ? 0 : defenderHp,
            JsonSerializer.Serialize(log),
            attackerWon
        );
    }
    
    private Player LoadPlayerWithStats(Guid playerId)
    {
        var player = _playerRepo.GetByIdAsync(playerId).Result;
        if (player == null)
            throw new InvalidOperationException($"Player {playerId} not found");
            
        return player;
    }
    
    private EffectiveStats CalculateEffectiveStats(Player player)
    {
        var baseStats = player.Stats;
        var equippedItems = _inventoryRepo.ListAsync(
            pi => pi.PlayerId == player.Id && pi.IsEquipped
        ).Result;
        
        var effectiveStats = new EffectiveStats
        {
            Karizma = baseStats.Karizma,
            Guc = baseStats.Guc,
            Zeka = baseStats.Zeka,
            Hayat = baseStats.Hayat,
            Level = player.Level
        };
        
        // Apply item bonuses
        foreach (var item in equippedItems)
        {
            foreach (var affix in item.Item.Affixes)
            {
                ApplyAffix(effectiveStats, affix);
            }
        }
        
        // Calculate derived stats
        effectiveStats.TotalHp = _statFormulaService.CalcTotalHp(
            effectiveStats.Level,
            effectiveStats.Hayat,
            effectiveStats.Guc,
            effectiveStats.HpPercent
        );
        
        effectiveStats.WeaponDamage = _balance.DefaultWeaponDamage;
        // CritChance calculation will be done in CalculateDamage method
        
        return effectiveStats;
    }
    
    private void ApplyAffix(EffectiveStats stats, ItemAffix affix)
    {
        var value = affix.Value;
        if (affix.IsPercent)
            value = value / 100.0;
            
        switch (affix.Type.ToLower())
        {
            case "karizma":
                stats.Karizma += (int)value;
                break;
            case "guc":
                stats.Guc += (int)value;
                break;
            case "zeka":
                stats.Zeka += (int)value;
                break;
            case "hayat":
                stats.Hayat += (int)value;
                break;
            case "hp_percent":
                stats.HpPercent += value;
                break;
            case "crit_chance":
                stats.CritChancePercent += value;
                break;
            case "crit_damage":
                stats.CritDamagePercent += value;
                break;
            case "lifesteal":
                stats.LifeSteal += value;
                break;
            case "weapon_damage":
                stats.WeaponDamage += (int)value;
                break;
        }
    }
    
    private double CalculateDamage(EffectiveStats attacker, EffectiveStats defender, Random random)
    {
        var preCritDamage = _statFormulaService.CalcPreCritDamage(
            attacker.WeaponDamage,
            attacker.Karizma,
            attacker.Guc,
            _balance.AlphaK,
            _balance.BetaG,
            attacker.DamagePercent
        );
        
        var expectedHit = _statFormulaService.CalcExpectedHit(
            preCritDamage,
            attacker.Karizma,
            attacker.CritChancePercent,
            attacker.CritDamagePercent
        );
        
        // Apply defender mitigation
        var mitigation = _statFormulaService.CalcMitigation(
            defender.Zeka,
            defender.FlatMitigation,
            defender.MitigationCapBonus
        );
        
        var finalDamage = expectedHit * (1 - mitigation);
        
        // Add some randomness for critical hits
        if (random.NextDouble() < attacker.CritChancePercent)
        {
            finalDamage *= 1.5; // Critical hit multiplier
        }
        
        return finalDamage;
    }
    
    private string GenerateSeed(Guid? duelId)
    {
        var seed = (duelId ?? Guid.NewGuid()).ToString("N") + DateTime.UtcNow.Ticks;
        return seed;
    }
    
    private void SaveDuelResult(CombatRequest req, bool attackerWon, List<string> log, List<DuelSnapshot> snapshots, string seed)
    {
        var duel = new Duel
        {
            Id = req.DuelId ?? Guid.NewGuid(),
            Player1Id = req.AttackerId,
            Player2Id = req.DefenderId,
            WinnerId = attackerWon ? req.AttackerId : req.DefenderId,
            Status = Domain.Entities.DuelStatus.Finished,
            LogJson = JsonSerializer.Serialize(log),
            Seed = seed,
            EndedAt = DateTime.UtcNow
        };
        
        _duelRepo.AddAsync(duel).Wait();
        _unitOfWork.SaveChangesAsync().Wait();
    }
}

public class EffectiveStats
{
    public int Karizma { get; set; }
    public int Guc { get; set; }
    public int Zeka { get; set; }
    public int Hayat { get; set; }
    public int Level { get; set; }
    
    // Derived stats
    public double TotalHp { get; set; }
    public int WeaponDamage { get; set; }
    public double CritChancePercent { get; set; }
    public double CritDamagePercent { get; set; }
    public double LifeSteal { get; set; }
    public double HpPercent { get; set; }
    public double DamagePercent { get; set; }
    public double FlatMitigation { get; set; }
    public double MitigationCapBonus { get; set; }
}
