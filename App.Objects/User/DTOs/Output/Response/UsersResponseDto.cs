namespace App.Objects.User.DTOs.Output.Response;

public record UsersResponseDto(
    Guid Id,
    string Email,
    DateTime CreatedAt
    );