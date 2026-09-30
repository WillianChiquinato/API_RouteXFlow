using System.Globalization;
using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using API_RouteXFlow.Interfaces.Services;
using API_RouteXFlow.Responses;
using Microsoft.Extensions.Logging;

namespace API_RouteXFlow.Services;

public class RouteService : IRouteService
{
    private const double EarthRadiusKm = 6371.0;
    private const string CompletedStatusName = "Concluído";

    private readonly IRouteRepository _routeRepository;
    private readonly ILogger<RouteService> _logger;

    public RouteService(IRouteRepository routeRepository, ILogger<RouteService> logger)
    {
        _routeRepository = routeRepository;
        _logger = logger;
    }

    public async Task<CustomResponse<List<RouteSummaryResponse>>> GetRoutes(RouteFilterRequest filter, int userId)
    {
        try
        {
            TypeApps? type = null;

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                if (!Enum.TryParse<TypeApps>(filter.Type, true, out var parsed))
                {
                    return new CustomResponse<List<RouteSummaryResponse>>(false, new List<string> { "Tipo de rota inválido." }, null);
                }

                type = parsed;
            }

            var routes = await _routeRepository.GetRoutesAsync(userId, filter, type);
            var response = routes.Select(ToSummary).ToList();

            return new CustomResponse<List<RouteSummaryResponse>>(true, new List<string>(), response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar histórico de rotas");
            throw;
        }
    }

    public async Task<CustomResponse<RouteDetailResponse>> GetRouteDetail(int id, int userId)
    {
        try
        {
            var route = await _routeRepository.GetRouteByIdAsync(userId, id);

            if (route is null || route.OriginGpsPosition is null)
            {
                return new CustomResponse<RouteDetailResponse>(false, new List<string> { "Rota não encontrada." }, null);
            }

            var stops = route.Stops.OrderBy(stop => stop.Sequence).ToList();
            var summary = ToSummary(route);

            var origin = ToPoint(route.OriginGpsPosition);
            var destination = route.DestinationGpsPosition is null ? null : ToPoint(route.DestinationGpsPosition);

            // O trajeto só conhece origem e destino: gps_position não tem vínculo com a rota
            // para pontos intermediários, então os blips do mapa vêm das paradas.
            var path = new List<RoutePointResponse> { origin };
            if (destination is not null)
            {
                path.Add(destination);
            }

            var detail = new RouteDetailResponse
            {
                Id = summary.Id,
                WorkSessionId = summary.WorkSessionId,
                Type = summary.Type,
                Status = summary.Status,
                StartTime = summary.StartTime,
                EndTime = summary.EndTime,
                OriginAddress = summary.OriginAddress,
                DestinationAddress = summary.DestinationAddress,
                TotalDistanceKm = summary.TotalDistanceKm,
                TotalMinutes = summary.TotalMinutes,
                StopsCount = summary.StopsCount,
                StopsCompletedCount = summary.StopsCompletedCount,
                Origin = origin,
                Destination = destination,
                Path = path,
                Stops = stops.Select(ToStop).ToList()
            };

            return new CustomResponse<RouteDetailResponse>(true, new List<string>(), detail);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Erro ao buscar detalhe da rota");
            throw;
        }
    }

    private static RouteSummaryResponse ToSummary(RoutePosition route)
    {
        var start = route.OriginGpsPosition?.Timestamp ?? route.CreatedAt;
        var end = route.DestinationGpsPosition?.Timestamp;

        var coordinates = new List<(double Lat, double Lng)>();
        AddCoordinate(coordinates, route.OriginGpsPosition);
        foreach (var stop in route.Stops.OrderBy(stop => stop.Sequence))
        {
            AddCoordinate(coordinates, stop.GpsPosition);
        }
        AddCoordinate(coordinates, route.DestinationGpsPosition);

        double distanceKm = 0;
        for (var i = 1; i < coordinates.Count; i++)
        {
            distanceKm += Haversine(coordinates[i - 1], coordinates[i]);
        }

        var minutes = ((end ?? DateTime.UtcNow) - start).TotalMinutes;

        return new RouteSummaryResponse
        {
            Id = route.Id,
            WorkSessionId = route.WorkSessionId,
            Type = route.Type == TypeApps.MarketPlace ? "Marketplace" : "Delivery",
            Status = end is null ? "InProgress" : "Finished",
            StartTime = start,
            EndTime = end,
            OriginAddress = route.OriginGpsPosition?.Address,
            DestinationAddress = route.DestinationGpsPosition?.Address,
            TotalDistanceKm = Math.Round((decimal)distanceKm, 2),
            TotalMinutes = Math.Round((decimal)Math.Max(minutes, 0), 1),
            StopsCount = route.Stops.Count,
            StopsCompletedCount = route.Stops.Count(stop => stop.StatusStop?.Name == CompletedStatusName)
        };
    }

    private static RoutePointResponse ToPoint(GpsPositionHistory position)
    {
        TryParse(position, out var lat, out var lng);

        return new RoutePointResponse
        {
            Id = position.Id,
            Latitude = lat,
            Longitude = lng,
            TypePosition = position.TypePosition.ToString(),
            RecordedAt = position.Timestamp
        };
    }

    private static RouteStopResponse ToStop(RoutePositionStops stop)
    {
        return new RouteStopResponse
        {
            Id = stop.Id,
            Sequence = stop.Sequence,
            Type = stop.Type.ToString(),
            Status = stop.StatusStop?.Name ?? "Processamento",
            Name = stop.GpsPosition?.Address ?? string.Empty,
            Address = stop.GpsPosition?.Address ?? string.Empty,
            AddressNumber = stop.GpsPosition?.AddressNumber ?? string.Empty,
            Latitude = stop.GpsPosition?.Latitude,
            Longitude = stop.GpsPosition?.Longitude
        };
    }

    private static void AddCoordinate(List<(double Lat, double Lng)> list, GpsPositionHistory? position)
    {
        if (position is not null && TryParse(position, out var lat, out var lng))
        {
            list.Add((lat, lng));
        }
    }

    private static bool TryParse(GpsPositionHistory position, out double lat, out double lng)
    {
        lat = 0;
        lng = 0;

        return double.TryParse(position.Latitude, NumberStyles.Float, CultureInfo.InvariantCulture, out lat)
               && double.TryParse(position.Longitude, NumberStyles.Float, CultureInfo.InvariantCulture, out lng);
    }

    private static double Haversine((double Lat, double Lng) a, (double Lat, double Lng) b)
    {
        var dLat = ToRadians(b.Lat - a.Lat);
        var dLng = ToRadians(b.Lng - a.Lng);

        var h = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(ToRadians(a.Lat)) * Math.Cos(ToRadians(b.Lat)) * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        return 2 * EarthRadiusKm * Math.Asin(Math.Sqrt(h));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
