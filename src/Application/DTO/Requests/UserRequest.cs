public class UserRegisterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public List<int> AppsActives = new List<int>();
}

public class UpdateProfileRequest
{
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class UpdatePreferencesRequest
{
    public string VehicleName { get; set; } = string.Empty;
    public decimal? VehicleKmPerLiter { get; set; }
    public decimal? FuelPricePerLiter { get; set; }
    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string TimeZone { get; set; } = "America/Sao_Paulo";
    public string DistanceUnit { get; set; } = "km";
    public string FuelUnit { get; set; } = "l";
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class CreateSubAccountRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
