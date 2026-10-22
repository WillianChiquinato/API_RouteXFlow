public class UserComposeDTO
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class SubAccountComposeDTO
{
    public int Id { get; set; }
    public int OwnerUserId { get; set; }
    public string? OwnerEmail { get; set; }
    public string? UserName { get; set; }
}