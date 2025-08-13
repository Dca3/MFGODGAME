using Quartz;
using MafiaMMORPG.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace MafiaMMORPG.Infrastructure.Jobs;

[DisallowConcurrentExecution]
public class SeasonCloseJob : IJob
{
    private readonly ISeasonService _seasonService;
    private readonly ILogger<SeasonCloseJob> _logger;

    public SeasonCloseJob(ISeasonService seasonService, ILogger<SeasonCloseJob> logger)
    {
        _seasonService = seasonService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting season close job");

            // Get current active season
            var currentSeasonId = await _seasonService.GetCurrentSeasonIdAsync();

            // Close current season
            await _seasonService.CloseSeasonAsync(currentSeasonId);

            // Open next season
            var newSeasonId = await _seasonService.OpenNextSeasonAsync();

            _logger.LogInformation("Season close job completed. Closed season {ClosedSeasonId}, opened season {NewSeasonId}", 
                currentSeasonId, newSeasonId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during season close job");
            throw;
        }
    }
}
