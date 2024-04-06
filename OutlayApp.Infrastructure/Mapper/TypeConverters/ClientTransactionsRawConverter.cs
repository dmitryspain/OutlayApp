using AutoMapper;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Database.InMemoryDb;

namespace OutlayApp.Infrastructure.Mapper.TypeConverters;

public class ClientTransactionsRawConverter : ITypeConverter<ClientTransaction, ClientTransactionDto>
{
    private readonly OutlayInMemoryContext _inMemoryContext;
    private readonly ILogoReferenceRepository _logoReferenceRepository;

    public ClientTransactionsRawConverter(OutlayInMemoryContext inMemoryContext, ILogoReferenceRepository logoReferenceRepository)
    {
        _inMemoryContext = inMemoryContext;
        _logoReferenceRepository = logoReferenceRepository;
    }

    public ClientTransactionDto Convert(ClientTransaction source,
        ClientTransactionDto destination,
        ResolutionContext context)
    {
        var cat = _inMemoryContext.MccInfos.FirstOrDefault(x => x.Mcc == source.Mcc)!.ShortDescription;
        var name = source.Description.Replace("Скасування. ", string.Empty);
        var icon = _logoReferenceRepository.GetByName(source.Description, CancellationToken.None).Result?.Url ?? string.Empty;
        return new ClientTransactionDto
        {
            Description = name,
            Amount = source.Amount,
            Icon = icon,
            Category = cat
        };
    }
}