using AutoMapper;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database.InMemoryDb;
using OutlayApp.Infrastructure.Services.Interfaces;

namespace OutlayApp.Infrastructure.Mapper.TypeConverters;

public class ClientTransactionConverter : ITypeConverter<GroupedTransaction, ClientTransactionsGroupedResponse>
{
    private readonly OutlayInMemoryContext _inMemoryContext;
    private readonly ILogoReferenceRepository _logoReferenceRepository;

    public ClientTransactionConverter(OutlayInMemoryContext inMemoryContext, ILogoReferenceRepository logoReferenceRepository)
    {
        _inMemoryContext = inMemoryContext;
        _logoReferenceRepository = logoReferenceRepository;
    }

    public ClientTransactionsGroupedResponse Convert(GroupedTransaction source,
        ClientTransactionsGroupedResponse destination,
        ResolutionContext context)
    {
        return new ClientTransactionsGroupedResponse
        {
            Name = source.Name,
            Amount = source.Amount,
            Icon = _logoReferenceRepository.GetByName(source.Name, CancellationToken.None).Result.Url,
            Category = _inMemoryContext.MccInfos.FirstOrDefault(x=>x.Mcc == source.Mcc)!.ShortDescription
        };
    }
}