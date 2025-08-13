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

        return _options.BaseXp + (int)Math.Floor(_options.CurveK * Math.Pow(currentLevel, _options.CurvePow));
    }

    public (int newLevel, int gainedFreePoints) ApplyXp(ref int level, ref long xp, int gainedXp, int pointsPerLevel = 5, int levelCap = 100)
    {
        xp += gainedXp;
        int gainedFreePoints = 0;
        int originalLevel = level;

        // Level up loop
        while (level < levelCap)
        {
            int xpNeeded = GetXpToNextLevel(level);
            if (xp < xpNeeded)
                break;

            xp -= xpNeeded;
            level++;
            gainedFreePoints += pointsPerLevel;
        }

        return (level, gainedFreePoints);
    }
}

public class ProgressionOptions
{
    public int PointsPerLevel { get; set; } = 5;
    public int BaseXp { get; set; } = 100;
    public double CurveK { get; set; } = 25.0;
    public double CurvePow { get; set; } = 1.5;
    public int LevelCap { get; set; } = 100;
}
