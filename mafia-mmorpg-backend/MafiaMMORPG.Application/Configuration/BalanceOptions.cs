namespace MafiaMMORPG.Application.Configuration;

public class BalanceOptions
{
    public const string SectionName = "Balance";
    
    public int HpBase { get; set; } = 500;
    public int HpPerLevel { get; set; } = 25;
    public int HpPerH { get; set; } = 15;
    public int HpPerG { get; set; } = 5;

    public double AlphaK { get; set; } = 0.04;
    public double BetaG { get; set; } = 0.02;
    public double CritBase { get; set; } = 0.05;
    public double CritPerK { get; set; } = 0.003;
    public double CritCap { get; set; } = 0.50;
    public double CritMultBase { get; set; } = 1.50;
    public double CritMultPerK { get; set; } = 0.002;

    public double MitCap { get; set; } = 0.60;
    public double MitCurveConst { get; set; } = 100;

    public int DefaultWeaponDamage { get; set; } = 50;
}
