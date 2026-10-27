using API_RouteXFlow.Domain.Data.Entities;
using API_RouteXFlow.Interfaces.Repository;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace API_RouteXFlow.Repository;

public class OAuthIntegrationRepository : IOAuthIntegrationRepository
{
    private readonly AppDbContext _dbContext;

    public OAuthIntegrationRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Uma conta externa só pode estar vinculada a um usuário da plataforma.
    public async Task<bool> IsExternalAccountLinkedToOtherUserAsync(string provider, long externalUserId, int userId)
    {
        return await _dbContext.OAuthIntegrations
            .AnyAsync(i => i.Provider == provider && i.ExternalUserId == externalUserId && i.UserId != userId);
    }

    // Upsert por (provider, user_id): reautorizar substitui os tokens da integração existente.
    public async Task<bool> SaveOAuthIntegrationAsync(OAuthIntegration integration)
    {
        var existing = await _dbContext.OAuthIntegrations
            .FirstOrDefaultAsync(i => i.Provider == integration.Provider && i.UserId == integration.UserId);

        if (existing == null)
        {
            _dbContext.OAuthIntegrations.Add(integration);
        }
        else
        {
            existing.AccessToken = integration.AccessToken;
            existing.RefreshToken = integration.RefreshToken;
            existing.TokenType = integration.TokenType;
            existing.ExpiresIn = integration.ExpiresIn;
            existing.ExpiresAt = integration.ExpiresAt;
            existing.Scope = integration.Scope;
            existing.ExternalUserId = integration.ExternalUserId;
            existing.RawData = integration.RawData;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<OAuthIntegration?> GetIntegrationAsync(string provider, int userId)
    {
        return await _dbContext.OAuthIntegrations
            .Include(i => i.ProviderApp)
            .FirstOrDefaultAsync(i => i.Provider == provider && i.UserId == userId);
    }

    // Upsert por (provider, user_id).
    public async Task<bool> SaveExternalProfileAsync(ProfilesExternal profile)
    {
        var existing = await _dbContext.ProfilesExternals
            .FirstOrDefaultAsync(p => p.Provider == profile.Provider && p.UserId == profile.UserId);

        if (existing == null)
        {
            _dbContext.ProfilesExternals.Add(profile);
        }
        else
        {
            existing.ExternalUserId = profile.ExternalUserId;
            existing.Nickname = profile.Nickname;
            existing.Email = profile.Email;
            existing.Permalink = profile.Permalink;
            existing.CountryId = profile.CountryId;
            existing.SiteId = profile.SiteId;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CreateStateAsync(OAuthState state)
    {
        // Aproveita para limpar states expirados.
        var expired = await _dbContext.OAuthStates.Where(s => s.ExpiresAt < DateTime.UtcNow).ToListAsync();
        _dbContext.OAuthStates.RemoveRange(expired);

        _dbContext.OAuthStates.Add(state);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    // Uso único: o state é removido ao ser consumido (protege contra replay/CSRF).
    public async Task<OAuthState?> ConsumeStateAsync(string state, string provider)
    {
        var entity = await _dbContext.OAuthStates
            .FirstOrDefaultAsync(s => s.State == state && s.Provider == provider);

        if (entity == null)
            return null;

        _dbContext.OAuthStates.Remove(entity);
        await _dbContext.SaveChangesAsync();

        return entity.ExpiresAt < DateTime.UtcNow ? null : entity;
    }

    public async Task<bool> SaveShipmentWebhookAsync(ShipmentWebhookDTO webhook)
    {
        var webhooktransform = new ShipmentWebhook
        {
            ShipmentId = webhook.ShipmentId,
            UserId = webhook.UserId,
            Topic = webhook.Topic ?? string.Empty,
            TypeShipment = webhook.TypeShipment,
            ReceivedAt = webhook.ReceivedAt ?? DateTime.UtcNow,
            ApplicationId = webhook.ApplicationId,
            Sent = webhook.Sent,
            Payload = webhook.Payload
        };

        var existing = await _dbContext.ShipmentWebhooks
            .FirstOrDefaultAsync(w => w.ShipmentId == webhook.ShipmentId && w.UserId == webhook.UserId);

        if (existing == null)
        {
            _dbContext.ShipmentWebhooks.Add(webhooktransform);
        }
        else
        {
            existing.UserId = webhooktransform.UserId;
            existing.ShipmentId = webhooktransform.ShipmentId;
            existing.Topic = webhooktransform.Topic;
            existing.TypeShipment = webhooktransform.TypeShipment;
            existing.ReceivedAt = webhooktransform.ReceivedAt;
            existing.ApplicationId = webhooktransform.ApplicationId;
            existing.Sent = webhooktransform.Sent;
            existing.Payload = webhooktransform.Payload;
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    // O UserId do webhook é o user_id do Mercado Livre (ExternalUserId da integração), não o do RouteXFlow.
    public async Task<ShipmentWebhook?> GetShipmentIntegrationAsync(long externalUserId)
    {
        return await _dbContext.ShipmentWebhooks
            .Where(w => w.UserId == externalUserId && w.IsActive)
            .OrderByDescending(w => w.ReceivedAt)
            .FirstOrDefaultAsync();
    }
}
