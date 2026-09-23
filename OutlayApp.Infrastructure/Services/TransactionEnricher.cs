using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Repositories;
using OutlayApp.Infrastructure.Mcc;

namespace OutlayApp.Infrastructure.Services;

public sealed class TransactionEnricher : ITransactionEnricher
{
    private readonly MccDirectory _mcc;
    private readonly ILogoReferenceRepository _logos;

    public TransactionEnricher(MccDirectory mcc, ILogoReferenceRepository logos)
    {
        _mcc = mcc;
        _logos = logos;
    }

    public async Task<List<ClientTransactionDto>> ToDtos(IReadOnlyCollection<ClientTransaction> transactions,
        CancellationToken cancellationToken)
    {
        var names = transactions.Select(t => ITransactionEnricher.LogoName(t.Description)).Distinct().ToList();
        var logos = await LogosFor(names, cancellationToken);

        return transactions.Select(t =>
        {
            var name = ITransactionEnricher.LogoName(t.Description);
            return new ClientTransactionDto
            {
                Id = t.Id,
                DateOccured = t.DateOccured,
                Description = name,
                Category = CategoryOf(t.Mcc),
                Icon = logos.GetValueOrDefault(name, string.Empty),
                Amount = t.Amount,
                BalanceAfter = t.BalanceAfter,
                CounterName = t.CounterName,
                Comment = t.Comment,
                Cashback = t.Cashback,
                Hold = t.Hold,
            };
        }).ToList();
    }

    public string CategoryOf(int mcc) => _mcc.NameOf(mcc);

    public Task<Dictionary<string, string>> LogosFor(IReadOnlyCollection<string> names,
        CancellationToken cancellationToken) =>
        _logos.GetUrlsByNames(names, cancellationToken);
}
