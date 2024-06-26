using AutoMapper;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database.InMemoryDb;

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
        var cat = _inMemoryContext.MccInfos.FirstOrDefault(x => x.Mcc == source.Mcc)!.ShortDescription;
        var name = source.Name.Replace("Скасування. ", string.Empty);
        var icon = _logoReferenceRepository.GetByName(name, CancellationToken.None).Result?.Url ?? string.Empty;
        return new ClientTransactionsGroupedResponse
        {
            Name = source.Name,
            Amount = source.Amount,
            Icon = icon,
            Category = cat
        };
    }
}