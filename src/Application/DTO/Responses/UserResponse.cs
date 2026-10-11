using API_RouteXFlow.Domain.Data.Entities;

namespace Application.DTO.Responses;

    public class MeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? Email { get; set; }
        public UserProfileDTO User { get; set; } = new();
        public UserPreferencesDTO Preferences { get; set; } = new();
    }