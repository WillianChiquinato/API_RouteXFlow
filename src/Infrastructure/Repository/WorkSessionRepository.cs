using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class WorkSessionRepository : IWorkSessionRepository
{
    private readonly AppDbContext _dbContext;

    public WorkSessionRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ContainerToDevicesDTO>> GetActiveContainersWithDevicesAsync(int userId)
    {
        return await _dbContext.Containers
            .AsNoTracking()
            .Where(container => container.UserId == userId && container.IsActive)
            .Select(container => new ContainerToDevicesDTO
            {
                Id = container.Id,
                Name = container.Name,
                IsActive = container.IsActive,
                Devices = container.ContainerDevices
                    .Where(containerDevice => containerDevice.IsActive)
                    .Select(containerDevice => containerDevice.Device!)
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<WorkSession?> GetOpenWorkSessionAsync(int userId)
    {
        return await _dbContext.WorkSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(session => session.UserId == userId && session.EndTime == null);
    }

    public async Task<WorkSession?> GetWorkSessionByIdAsync(int id)
    {
        return await _dbContext.WorkSessions.AsNoTracking().FirstOrDefaultAsync(session => session.Id == id);
    }

    public async Task<WorkSession> CreateWorkSessionAsync(WorkSession session)
    {
        _dbContext.Add(session);
        await _dbContext.SaveChangesAsync();

        return session;
    }

    public async Task<GpsPositionHistory> AddGpsPositionAsync(GpsPositionHistory position)
    {
        _dbContext.Add(position);
        await _dbContext.SaveChangesAsync();

        return position;
    }

    public async Task<bool> FinishWorkSessionAsync(int id, DateTime endTime)
    {
        var session = await _dbContext.WorkSessions.FirstOrDefaultAsync(session => session.Id == id);

        if (session is null)
        {
            return false;
        }

        session.EndTime = endTime;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<List<WorkSessionSummaryResponse>> GetSessionsAsync(int userId, WorkSessionFilterRequest filter)
    {
        var query = _dbContext.WorkSessions
            .AsNoTracking()
            .Where(session => session.UserId == userId);

        if (filter.StartDate.HasValue)
        {
            query = query.Where(session => session.StartTime >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(session => session.StartTime <= filter.EndDate.Value);
        }

        return await query
            .OrderByDescending(session => session.StartTime)
            .Select(session => new WorkSessionSummaryResponse
            {
                Id = session.Id,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                ContainerId = session.ContainerId,
                ContainerName = session.Container != null ? session.Container.Name : string.Empty,
                DeliveryOffersCount = _dbContext.DeliveryOffers.Count(offer => offer.WorkSessionId == session.Id)
            })
            .ToListAsync();
    }

    public async Task<WorkSessionDetailResponse?> GetSessionDetailAsync(int userId, int id)
    {
        return await _dbContext.WorkSessions
            .AsNoTracking()
            .Where(session => session.Id == id && session.UserId == userId)
            .Select(session => new WorkSessionDetailResponse
            {
                Id = session.Id,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                ContainerId = session.ContainerId,
                ContainerName = session.Container != null ? session.Container.Name : string.Empty,
                DeliveryOffersCount = _dbContext.DeliveryOffers.Count(offer => offer.WorkSessionId == session.Id),
                DeliveryOffers = _dbContext.DeliveryOffers
                    .Where(offer => offer.WorkSessionId == session.Id)
                    .OrderByDescending(offer => offer.DetectedAt)
                    .Select(offer => new DeliveryOfferResponse
                    {
                        Id = offer.Id,
                        Name = offer.Name,
                        Description = offer.Description,
                        Value = offer.Value,
                        BonusValue = offer.BonusValue,
                        TotalDistanceKm = offer.TotalDistanceKm,
                        EstimatedMinutes = offer.EstimatedMinutes,
                        Status = offer.Status!.Name,
                        DetectedAt = offer.DetectedAt,
                        Stops = _dbContext.DeliveryStops
                            .Where(stop => stop.DeliveryOfferId == offer.Id)
                            .OrderBy(stop => stop.Sequence)
                            .Select(stop => new DeliveryStopResponse
                            {
                                Id = stop.Id,
                                Name = stop.Name,
                                Address = stop.Address,
                                Sequence = stop.Sequence,
                                Type = stop.Type.ToString(),
                                Latitude = stop.Latitude,
                                Longitude = stop.Longitude
                            })
                            .ToList(),
                        RouteEvaluation = _dbContext.RouteEvaluations
                            .Where(evaluation => evaluation.DeliveryOfferId == offer.Id)
                            .Select(evaluation => new RouteEvaluationResponse
                            {
                                Recommended = evaluation.Recommended,
                                AdditionalDistanceKm = evaluation.AdditionalDistanceKm,
                                AdditionalTimeMinutes = evaluation.AdditionalTimeMinutes,
                                RouteDeviationKm = evaluation.RouteDeviationKm,
                                ValuePerKm = evaluation.ValuePerKm,
                                EvaluationScore = evaluation.EvaluationScore,
                                Comments = evaluation.Comments
                            })
                            .FirstOrDefault()
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();
    }
}
