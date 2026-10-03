using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed record GetEventQuery(Guid EventId) : IRequest<EventDto?>;
