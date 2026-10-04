using Microsoft.AspNetCore.Authorization;

namespace AB.SmokPrzewodnik.Api.Auth;

internal sealed record NavigationSessionAccessRequirement(
      string AuthenticatedRole) : IAuthorizationRequirement;
