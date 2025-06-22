namespace ReviewGenie.Application.DTO;

public record RegisterDto(
    string FullName,
    string Email,
    string Password,
    string ConfirmPassword);

public record LoginDto(
    string Email,
    string Password);
