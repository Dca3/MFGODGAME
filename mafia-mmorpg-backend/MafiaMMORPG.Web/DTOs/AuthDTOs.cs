using System.ComponentModel.DataAnnotations;

namespace MafiaMMORPG.Web.DTOs;

public record RegisterRequest(
    [Required] [StringLength(50)] string Username,
    [Required] [EmailAddress] string Email,
    [Required] [StringLength(100, MinimumLength = 6)] string Password
);

public record LoginRequest(
    [Required] string Username,
    [Required] string Password
);

public record RefreshRequest(
    [Required] string RefreshToken
);

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);

public record AllocateStatsRequest(
    [Required] int Karizma,
    [Required] int Guc,
    [Required] int Zeka,
    [Required] int Hayat
);

public record UserAction(
    string Type,
    string? Target,
    Dictionary<string, object>? Data
);
