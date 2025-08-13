using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using MafiaMMORPG.Application.Interfaces;
using MafiaMMORPG.Application.DTOs;
using MafiaMMORPG.Web.DTOs;

namespace MafiaMMORPG.Web.Hubs;

public class DuelHub : Hub
{
    private readonly IMatchmakingService _matchmakingService;
    private readonly ICombatService _combatService;
    private readonly ILogger<DuelHub> _logger;

    public DuelHub(
        IMatchmakingService matchmakingService,
        ICombatService combatService,
        ILogger<DuelHub> logger)
    {
        _matchmakingService = matchmakingService;
        _combatService = combatService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserIdFromContext();
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} connected to DuelHub", userId);
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var playerId = GetPlayerIdFromContext();
        if (playerId != Guid.Empty)
        {
            _logger.LogInformation("Player {PlayerId} disconnected from DuelHub", playerId);
            
            // Auto-dequeue player if they were in queue
            try
            {
                await _matchmakingService.DequeueAsync(playerId);
                _logger.LogInformation("Auto-dequeued player {PlayerId} due to disconnect", playerId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to auto-dequeue player {PlayerId}", playerId);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinQueue()
    {
        var playerId = GetPlayerIdFromContext();
        if (playerId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        try
        {
            _logger.LogInformation("Player {PlayerId} joining queue", playerId);
            
            var matchInfo = await _matchmakingService.EnqueueAsync(playerId);
            
            if (matchInfo != null)
            {
                await Clients.User(matchInfo.P1Id.ToString()).SendAsync("MatchFound", matchInfo);
                await Clients.User(matchInfo.P2Id.ToString()).SendAsync("MatchFound", matchInfo);
            }
            else
            {
                await Clients.Caller.SendAsync("QueueJoined", "Added to queue");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining queue for player {PlayerId}", playerId);
            await Clients.Caller.SendAsync("Error", "Failed to join queue");
        }
    }

    public async Task LeaveQueue()
    {
        var playerId = GetPlayerIdFromContext();
        if (playerId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        try
        {
            var success = await _matchmakingService.DequeueAsync(playerId);
            
            if (success)
            {
                await Clients.Caller.SendAsync("QueueLeft", "Removed from queue");
            }
            else
            {
                await Clients.Caller.SendAsync("Error", "Not in queue");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving queue for player {PlayerId}", playerId);
            await Clients.Caller.SendAsync("Error", "Failed to leave queue");
        }
    }

    public async Task AcceptMatch(Guid matchId)
    {
        var playerId = GetPlayerIdFromContext();
        if (playerId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        try
        {
            var accepted = await _matchmakingService.AcceptAsync(playerId, matchId);
            
            if (accepted)
            {
                // Her iki taraf da kabul etti, match başlat
                await Clients.Caller.SendAsync("MatchAccepted", matchId);
                
                // TODO: CombatService başlatma
                // var combatRequest = new CombatRequest(matchId, p1Id, p2Id);
                // var result = _combatService.SimulatePvp(combatRequest);
                // await Clients.User(p1Id.ToString()).SendAsync("DuelStarted", result);
                // await Clients.User(p2Id.ToString()).SendAsync("DuelStarted", result);
            }
            else
            {
                await Clients.Caller.SendAsync("MatchAcceptancePending", "Waiting for other player to accept");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting match {MatchId} for player {PlayerId}", matchId, playerId);
            await Clients.Caller.SendAsync("Error", "Failed to accept match");
        }
    }

    public async Task SubmitAction(UserAction action)
    {
        var playerId = GetPlayerIdFromContext();
        if (playerId == Guid.Empty)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized");
            return;
        }

        // Validation
        if (action == null || string.IsNullOrEmpty(action.Type) || action.TurnId <= 0)
        {
            await Clients.Caller.SendAsync("Error", "Invalid action data");
            return;
        }

        // Message size validation (64KB limit)
        var actionJson = System.Text.Json.JsonSerializer.Serialize(action);
        if (actionJson.Length > 64 * 1024)
        {
            await Clients.Caller.SendAsync("Error", "Action data too large");
            return;
        }

        // TODO: Implement idempotency check and throttle
        // TODO: Implement actual action processing

        try
        {
            await Clients.Caller.SendAsync("ActionReceived", "Action submitted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting action for player {PlayerId}", playerId);
            await Clients.Caller.SendAsync("Error", "Failed to submit action");
        }
    }

    private string? GetUserIdFromContext()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    private Guid GetPlayerIdFromContext()
    {
        var userId = GetUserIdFromContext();
        if (string.IsNullOrEmpty(userId))
            return Guid.Empty;

        if (Guid.TryParse(userId, out var playerId))
            return playerId;

        return Guid.Empty;
    }
}
