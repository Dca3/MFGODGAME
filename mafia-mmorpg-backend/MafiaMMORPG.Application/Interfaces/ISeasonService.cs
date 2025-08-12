namespace MafiaMMORPG.Application.Interfaces;

public interface ISeasonService
{
    Task CloseSeasonAsync(Guid seasonId);
    Task OpenNextSeasonAsync();
}
