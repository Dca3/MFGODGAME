namespace MafiaMMORPG.Application.Interfaces;

public interface IProgressionService
{
    int GetXpToNextLevel(int currentLevel);
    (int newLevel, int gainedFreePoints) ApplyXp(ref int level, ref long xp, int gainedXp, int pointsPerLevel = 5, int levelCap = 100);
    List<(int level, int xpNeeded, int totalXp)> GetXpProgression();
}
