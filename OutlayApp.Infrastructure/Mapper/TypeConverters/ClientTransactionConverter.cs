using AutoMapper;
using OutlayApp.Application;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Infrastructure.Database.InMemoryDb;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Mapper.TypeConverters;

public class ClientTransactionConverter : ITypeConverter<GroupedTransaction, ClientTransactionsGroupedResponse>
{
    private readonly IGoogleImageSearchService _googleImageSearchService;
    private readonly OutlayInMemoryContext _inMemoryContext;

    public ClientTransactionConverter(IGoogleImageSearchService googleImageSearchService, OutlayInMemoryContext inMemoryContext)
    {
        _googleImageSearchService = googleImageSearchService;
        _inMemoryContext = inMemoryContext;
    }

    public ClientTransactionsGroupedResponse Convert(GroupedTransaction source, ClientTransactionsGroupedResponse destination,
        ResolutionContext context)
    {
        return new ClientTransactionsGroupedResponse
        {
            Name = source.Name,
            Amount = source.Amount,
            Icon = _googleImageSearchService.GetCompanyLogo(source.Name, source.Mcc, CancellationToken.None).Result,
            Category = _inMemoryContext.MccInfos.FirstOrDefault(x => x.Mcc == source.Mcc)!.ShortDescription
        };
    }
}