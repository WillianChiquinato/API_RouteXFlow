using API_RouteXFlow.Domain.Data.Entities;

namespace Application.DTO;

public class UserProfileDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public static UserProfileDTO From(User user) => new()
    {
        Id = user.Id,
        Name = user.Username,
        Email = user.Email,
        Cpf = user.Cpf,
        PhoneNumber = user.PhoneNumber,
    };
}

public class UserPreferencesDTO
{
    public string VehicleName { get; set; } = string.Empty;
    public decimal? VehicleKmPerLiter { get; set; }
    public decimal? FuelPricePerLiter { get; set; }

    public string DateFormat { get; set; } = "dd/MM/yyyy";
    public string TimeFormat { get; set; } = "24h";
    public string TimeZone { get; set; } = "America/Sao_Paulo";
    public string DistanceUnit { get; set; } = "km";
    public string FuelUnit { get; set; } = "l";

    public static UserPreferencesDTO From(Preferences preferences) => new()
    {
        VehicleName  = preferences.VehicleName,
        VehicleKmPerLiter = preferences.VehicleKmPerLiter,
        FuelPricePerLiter = preferences.FuelPricePerLiter,
        DateFormat = preferences.DateFormat,
        TimeFormat = preferences.TimeFormat,
        TimeZone = preferences.TimeZone,
        DistanceUnit = preferences.DistanceUnit,
        FuelUnit = preferences.FuelUnit,
    };
}
