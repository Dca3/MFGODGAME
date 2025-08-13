using Microsoft.Extensions.Options;
using MafiaMMORPG.Application.Interfaces;

namespace MafiaMMORPG.Infrastructure.Services;

public class ProgressionService : IProgressionService
{
    private readonly ProgressionOptions _options;

    public ProgressionService(IOptions<ProgressionOptions> options)
    {
        _options = options.Value;
    }

    public int GetXpToNextLevel(int currentLevel)
    {
        if (currentLevel >= _options.LevelCap)
            return 0;

        // XP formülü: BaseXp * (1 + CurveK * level^CurvePow)
        // Örnek: Level 1→2: 100 * (1 + 1.5 * 1^1.2) = 100 * 2.5 = 250 XP
        // Level 2→3: 100 * (1 + 1.5 * 2^1.2) = 100 * 3.8 = 380 XP
        return _options.BaseXp + (int)Math.Floor(_options.BaseXp * _options.CurveK * Math.Pow(currentLevel, _options.CurvePow));
    }

    public (int newLevel, int gainedFreePoints) ApplyXp(ref int level, ref long xp, int gainedXp, int pointsPerLevel = 5, int levelCap = 50)
    {
        Console.WriteLine($"DEBUG: ApplyXp - Level: {level}, XP: {xp}, Gained XP: {gainedXp}, LevelCap: {levelCap}");
        
        xp += gainedXp;
        int gainedFreePoints = 0;
        int originalLevel = level;

        Console.WriteLine($"DEBUG: After adding XP - Level: {level}, XP: {xp}");

        // Level up loop
        while (level < levelCap)
        {
            int xpNeeded = GetXpToNextLevel(level);
            Console.WriteLine($"DEBUG: Level {level} needs {xpNeeded} XP, current XP: {xp}");
            
            if (xp < xpNeeded)
                break;

            xp -= xpNeeded;
            level++;
            gainedFreePoints += pointsPerLevel;
            Console.WriteLine($"DEBUG: Level up! New level: {level}, XP remaining: {xp}, Free points: {gainedFreePoints}");
        }

        Console.WriteLine($"DEBUG: Final result - Level: {level}, XP: {xp}, Free points: {gainedFreePoints}");
        return (level, gainedFreePoints);
    }

    // XP progression analizi için yardımcı metod
    public List<(int level, int xpNeeded, int totalXp)> GetXpProgression()
    {
        var progression = new List<(int level, int xpNeeded, int totalXp)>();
        int totalXp = 0;

        for (int level = 1; level < _options.LevelCap; level++)
        {
            int xpNeeded = GetXpToNextLevel(level);
            totalXp += xpNeeded;
            progression.Add((level, xpNeeded, totalXp));
        }

        return progression;
    }
}

public class ProgressionOptions
{
    public int PointsPerLevel { get; set; } = 5;
    public int BaseXp { get; set; } = 100;
    public double CurveK { get; set; } = 1.5;
    public double CurvePow { get; set; } = 1.2;
    public int LevelCap { get; set; } = 50;
}
