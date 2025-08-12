using Microsoft.Extensions.Options;
using MafiaMMORPG.Application.Configuration;
using MafiaMMORPG.Application.Interfaces;

namespace MafiaMMORPG.Infrastructure.Services;

public class StatFormulaService : IStatFormulaService
{
    private readonly BalanceOptions _balance;

    public StatFormulaService(IOptions<BalanceOptions> balanceOptions)
    {
        _balance = balanceOptions.Value;
    }

    public double CalcMitigation(double zEff, double flatMitigation, double capBonus = 0)
    {
        var mitigationZ = 0.60 * (zEff / (zEff + _balance.MitCurveConst));
        var totalMitigation = Math.Min(_balance.MitCap + capBonus, mitigationZ + flatMitigation);
        return Math.Max(0, totalMitigation);
    }

    public double CalcTotalHp(int level, double hEff, double gEff, double pctHp)
    {
        var hpBase = _balance.HpBase + _balance.HpPerLevel * (level - 1);
        var hpRaw = hpBase + _balance.HpPerH * hEff + _balance.HpPerG * gEff;
        return hpRaw * (1 + pctHp);
    }

    public double CalcPreCritDamage(double wEff, double kEff, double gEff, double alphaK, double betaG, double pctDmg)
    {
        return wEff * (1 + alphaK * kEff) * (1 + betaG * gEff) * (1 + pctDmg);
    }

    public double CalcExpectedHit(double preCrit, double kEff, double pctCritChance, double pctCritDamage, double critCapBonus = 0)
    {
        var critChance = Math.Min(_balance.CritCap + critCapBonus, 
            _balance.CritBase + _balance.CritPerK * kEff + pctCritChance);
        var critMult = (_balance.CritMultBase + _balance.CritMultPerK * kEff) * (1 + pctCritDamage);
        
        return preCrit * (1 + critChance * (critMult - 1));
    }

    public double CalcLifeSteal(double expectedHit, double pctLifesteal)
    {
        return expectedHit * pctLifesteal;
    }
}
