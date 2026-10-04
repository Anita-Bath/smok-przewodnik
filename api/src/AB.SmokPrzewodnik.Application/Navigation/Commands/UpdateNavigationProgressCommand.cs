using AB.SmokPrzewodnik.Application.Navigation.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Navigation.Commands;

public sealed record UpdateNavigationProgressCommand(
    Guid SessionId,
    UpdateNavigationProgressRequest Progress) : IRequest<NavigationProgressResponse>;
