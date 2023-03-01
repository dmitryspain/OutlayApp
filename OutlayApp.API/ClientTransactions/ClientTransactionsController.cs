using Amazon;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutlayApp.API.Options.DatabaseOptions;
using OutlayApp.Application.ClientTransactions.Commands;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsWeekly;
using OutlayApp.Application.LogoReferences;

namespace OutlayApp.API.ClientTransactions;

[ApiController]
[Route("api/transactions")]
public class ClientTransactionsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IOptions<DatabaseOptions> _databaseOptions;

    public ClientTransactionsController(ISender sender, IOptions<DatabaseOptions> databaseOptions)
    {
        _sender = sender;
        _databaseOptions = databaseOptions;
    }

    
    
    
    [HttpGet("test")]
    public async Task<IActionResult> Test(CancellationToken cancellationToken)
    {
        string secretName = "ApiKey";
        string region = "us-east-1";

        IAmazonSecretsManager client = new AmazonSecretsManagerClient(RegionEndpoint.GetBySystemName(region));

        GetSecretValueRequest request = new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = "AWSCURRENT", // VersionStage defaults to AWSCURRENT if unspecified.
        };

        GetSecretValueResponse response;

        try
        {
            response = await client.GetSecretValueAsync(request);
        }
        catch (Exception e)
        {
            // For a list of the exceptions thrown, see
            // https://docs.aws.amazon.com/secretsmanager/latest/apireference/API_GetSecretValue.html
            throw e;
        }

        string secret = response.SecretString;
        return null;
    }
    [HttpGet("latest")]
    public async Task<IActionResult> FetchLatestTransactions(string externalCardId, CancellationToken cancellationToken)
    {
        var command = new FetchLatestTransactionsCommand(externalCardId);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok() : BadRequest(result.Error);
    }


    [HttpGet("by-period")]
    public async Task<IActionResult> GetTransactionsByPeriod(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsQuery(clientCardId, dateFrom, dateTo);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("grouped")]
    public async Task<IActionResult> GetTransactionsGrouped(Guid clientCardId, DateTime? dateFrom, DateTime? dateTo,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsGroupedQuery(clientCardId, dateFrom, dateTo);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("most-freq")]
    public async Task<IActionResult> GetMostFrequency(Guid clientCardId, CancellationToken cancellationToken)
    {
        var command = new FetchMostFrequencyIconsCommand(null);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result) : BadRequest(result.Error);
    }

    [HttpGet("by-description")]
    public async Task<IActionResult> GetTransactionsByDescription(Guid clientCardId, string description,
        CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsByDescriptionQuery(clientCardId, description);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklyTransactions(Guid clientCardId, int skipWeeks, CancellationToken cancellationToken)
    {
        var command = new GetClientTransactionsWeeklyQuery(clientCardId, skipWeeks);
        var result = await _sender.Send(command, cancellationToken);
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}