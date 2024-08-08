﻿using System.Threading;
using System.Threading.Tasks;
using Ardalis.ApiEndpoints;
using AutoMapper;
using BlazorShared.Models.Client;
using FrontDesk.Core.SyncedAggregates;
using FrontDesk.Core.SyncedAggregates.Specifications;
using Microsoft.AspNetCore.Mvc;
using PluralsightDdd.SharedKernel.Interfaces;
using Swashbuckle.AspNetCore.Annotations;

namespace FrontDesk.Api.ClientEndpoints
{
  public class GetById : EndpointBaseAsync
    .WithRequest<GetByIdClientRequest>
    .WithActionResult<GetByIdClientResponse>
  {
    private readonly IReadRepository<Client> _repository;
    private readonly IMapper _mapper;

    public GetById(IReadRepository<Client> repository,
      IMapper mapper)
    {
      _repository = repository;
      _mapper = mapper;
    }

    [HttpGet(GetByIdClientRequest.Route)]
    [SwaggerOperation(
        Summary = "Get a Client by Id",
        Description = "Gets a Client by Id",
        OperationId = "clients.GetById",
        Tags = new[] { "ClientEndpoints" })
    ]
    public override async Task<ActionResult<GetByIdClientResponse>> HandleAsync([FromRoute] GetByIdClientRequest request,
      CancellationToken cancellationToken)
    {
      var response = new GetByIdClientResponse(request.CorrelationId());

      var spec = new ClientByIdIncludePatientsSpecification(request.ClientId);
      var client = await _repository.SingleOrDefaultAsync(spec);
      if (client is null) return NotFound();

      response.Client = _mapper.Map<ClientDto>(client);

      return Ok(response);
    }
  }
}
