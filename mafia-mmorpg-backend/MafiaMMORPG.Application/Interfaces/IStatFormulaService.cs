namespace MafiaMMORPG.Application.Interfaces;

public interface IStatFormulaService
{
    double CalcMitigation(double zEff, double flatMitigation, double capBonus = 0);
    double CalcTotalHp(int level, double hEff, double gEff, double pctHp);
    double CalcPreCritDamage(double wEff, double kEff, double gEff,
                             double alphaK, double betaG, double pctDmg);
    double CalcExpectedHit(double preCrit, double kEff,
                           double pctCritChance, double pctCritDamage,
                           double critCapBonus = 0);
    double CalcLifeSteal(double expectedHit, double pctLifesteal);
}
