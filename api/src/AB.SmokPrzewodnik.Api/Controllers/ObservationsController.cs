using AB.SmokPrzewodnik.Application.Common.Querying;
using AB.SmokPrzewodnik.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AB.SmokPrzewodnik.Api.Controllers;

[Authorize(Policy = AppConsts.AuthPolicyName)]
[Route("/observations")]
public sealed class ObservationsController : ControllerBase
{
    //[HttpPost]
    //public async Task InsertObservationAsync(CancellationToken cancellationToken)
    //{
    //}

    //[HttpGet]
    //[Route("/{observationId:guid}")]
    //public async Task<> GetObservationAsync(Guid observationId, CancellationToken cancellationToken)
    //{
    //}

    //[HttpPost]
    //[Route("/{observationId:guid}/votes")]
    //public async Task SubmitVoteAsync(Guid observationId, CancellationToken cancellationToken)
    //{
    //}
}
