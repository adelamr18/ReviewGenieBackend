namespace ReviewGenie.Application.DTO;

public record BusinessDto(
    string Name,
    string Type,
    string Street,
    string City,
    string State,
    string Zip,
    string Phone,
    string? Website,
    string Description);
