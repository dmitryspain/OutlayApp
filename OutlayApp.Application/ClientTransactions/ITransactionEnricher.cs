using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Application.ClientTransactions;

/// <summary>Turns stored rows into DTOs with category names and logos — in one pass for the whole list.</summary>
public interface ITransactionEnricher
{
    Task<List<ClientTransactionDto>> ToDtos(IReadOnlyCollection<ClientTransaction> transactions,
        CancellationToken cancellationToken);

    /// <summary>Short category name of an MCC ("" when unknown).</summary>
    string CategoryOf(int mcc);

    Task<Dictionary<string, string>> LogosFor(IReadOnlyCollection<string> names, CancellationToken cancellationToken);

    /// <summary>The name logos are stored under ("Скасування. X" is X).</summary>
    static string LogoName(string description) => description.Replace("Скасування. ", string.Empty);
}
