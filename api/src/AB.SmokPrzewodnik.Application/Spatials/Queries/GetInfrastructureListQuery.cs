using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Application.Spatials.Dtos;
using MediatR;

namespace AB.SmokPrzewodnik.Application.Spatials.Queries;

public sealed record GetInfrastructureListQuery(CursorPageRequest Page) : IRequest<CursorPageResponse<InfrastructureDto>>;
