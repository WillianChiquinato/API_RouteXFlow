using API_RouteXFlow.Domain.Data.Entities;

namespace Application.DTO.Responses;

public class SubAccountDTO
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public static SubAccountDTO From(SubAccount subAccount) => new()
    {
        Id = subAccount.Id,
        Username = subAccount.Username,
        CreatedAt = subAccount.CreatedAt
    };
}
