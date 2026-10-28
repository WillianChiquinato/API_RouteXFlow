using API_RouteXFlow.Domain.Data.Entities;

namespace Application.DTO.Responses;

public class PendingUserResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Role { get; set; }
    public int RoleId { get; set; }
    public AccountType AccountType { get; set; }
    public List<string> Apps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
}
