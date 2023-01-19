using AutoMapper;
using OutlayApp.Application;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Infrastructure.Database.InMemoryDb;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Mapper.TypeConverters;

public class ClientTransactionConverter : ITypeConverter<BrandFetchInfo, ClientTransactionsGroupedResponse>
{
    private readonly IBrandFetchService _brandFetchService;
    private readonly OutlayInMemoryContext _inMemoryContext;

    public ClientTransactionConverter(IBrandFetchService brandFetchService, OutlayInMemoryContext inMemoryContext)
    {
        _brandFetchService = brandFetchService;
        _inMemoryContext = inMemoryContext;
    }

    public ClientTransactionsGroupedResponse Convert(BrandFetchInfo source, ClientTransactionsGroupedResponse destination,
        ResolutionContext context)
    {
        return new ClientTransactionsGroupedResponse
        {
            Name = source.Name,
            Amount = source.Amount,
            Icon = _brandFetchService.GetCompanyLogo(source.Name, CancellationToken.None).Result,
            Category = _inMemoryContext.MccInfos.FirstOrDefault(x => x.Mcc == source.Mcc)!.ShortDescription
        };
    }
}