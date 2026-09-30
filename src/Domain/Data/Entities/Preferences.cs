using System.ComponentModel.DataAnnotations.Schema;

namespace API_RouteXFlow.Domain.Data.Entities;

[Table("preferences")]
public class Preferences : BaseEntity
{
    [Column("user_id")]
    public int UserId { get; set; }
    
    [Column("vehicle_name")]
    public string? VehicleName { get; set; } = string.Empty;

    [Column("vehicle_km_per_liter")]
    public decimal? VehicleKmPerLiter { get; set; }

    [Column("fuel_price_per_liter")]
    public decimal? FuelPricePerLiter { get; set; }

    [Column("date_format")]
    public string DateFormat { get; set; } = "dd/MM/yyyy";

    [Column("time_format")]
    public string TimeFormat { get; set; } = "24h";

    [Column("time_zone")]
    public string TimeZone { get; set; } = "America/Sao_Paulo";

    [Column("distance_unit")]
    public string DistanceUnit { get; set; } = "km";

    [Column("fuel_unit")]
    public string FuelUnit { get; set; } = "l";
    
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}