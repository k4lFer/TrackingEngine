namespace App.Objects.User.DTOs.Output.Response;

public record class ProfileResponseDto(
    Guid Id,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    DateTime RegisterDate 

)
{}