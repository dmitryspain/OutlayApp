using OutlayApp.Application.Transactions;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Domain.Clients;

namespace OutlayApp.Tests;

public class StatementImporterTests
{
    private static readonly DateTime T0 = new(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc);

    private readonly FakeTransactionRepository _repo = new();
    private readonly ClientCard _card;
    private readonly StatementImporter _importer;

    public StatementImporterTests()
    {
        var client = Client.Create("Test", "token-token-token-token-token", "enc");
        _card = client.AddCard("acc-1", "black", 0, 0, 980, null, null).Value!;
        _importer = new StatementImporter(_repo);
    }

    private static MonobankTransaction Item(string id, int minutes, long amount, string description = "Glovo", bool hold = false) => new()
    {
        Id = id,
        Time = new DateTimeOffset(T0.AddMinutes(minutes)).ToUnixTimeSeconds(),
        Amount = amount,
        Balance = 100_00,
        Description = description,
        Mcc = 5814,
        Hold = hold,
        CashbackAmount = 150,
        CounterName = "",
    };

    private ClientTransaction Legacy(int minutes, decimal amount, string description = "Glovo")
    {
        var row = ClientTransaction.Create(_card.Id,
            new TransactionDetails(description, amount, 0, T0.AddMinutes(minutes), 5814, ExternalId: null));
        _repo.Rows.Add(row);
        return row;
    }

    [Fact]
    public async Task Adds_new_items_with_all_details()
    {
        var result = await _importer.Import(_card, new[] { Item("a", 0, -12345) }, default);

        var row = Assert.Single(_repo.Rows);
        Assert.Single(result.Added);
        Assert.Equal("a", row.ExternalId);
        Assert.Equal(-123.45m, row.Amount);
        Assert.Equal(1.5m, row.Cashback);
        Assert.Equal(T0, row.DateOccured);
        Assert.Equal(DateTimeKind.Utc, row.DateOccured.Kind);
        Assert.Null(row.CounterName); // empty strings are not kept
    }

    [Fact]
    public async Task Never_stores_an_item_twice()
    {
        await _importer.Import(_card, new[] { Item("a", 0, -100) }, default);
        var again = await _importer.Import(_card, new[] { Item("a", 0, -100), Item("b", 1, -200) }, default);

        Assert.Equal(2, _repo.Rows.Count);
        Assert.Equal(new[] { "b" }, again.Added.Select(x => x.ExternalId));
    }

    [Fact]
    public async Task Settles_a_hold()
    {
        await _importer.Import(_card, new[] { Item("a", 0, -100, hold: true) }, default);
        var result = await _importer.Import(_card, new[] { Item("a", 0, -100, hold: false) }, default);

        Assert.False(Assert.Single(_repo.Rows).Hold);
        Assert.Equal(1, result.Updated);
    }

    [Fact]
    public async Task Old_rows_take_over_the_bank_id_and_extra_copies_go()
    {
        var kept = Legacy(0, -1m);
        Legacy(0, -1m); // a copy left by the old importer

        var result = await _importer.Import(_card, new[] { Item("a", 0, -100) }, default);

        Assert.Same(kept, Assert.Single(_repo.Rows));
        Assert.Equal("a", kept.ExternalId);
        Assert.Empty(result.Added);
        Assert.Equal(1, result.RemovedDuplicates);
    }

    [Fact]
    public async Task Genuinely_repeated_items_are_all_kept()
    {
        Legacy(0, 3m, "Скасування. PDFe");
        Legacy(0, 3m, "Скасування. PDFe");

        var items = new[] { Item("a", 0, 300, "Скасування. PDFe"), Item("b", 0, 300, "Скасування. PDFe"), Item("c", 0, 300, "Скасування. PDFe") };
        var result = await _importer.Import(_card, items, default);

        Assert.Equal(3, _repo.Rows.Count);
        Assert.Equal(new[] { "a", "b", "c" }, _repo.Rows.Select(x => x.ExternalId).Order());
        Assert.Single(result.Added);
        Assert.Equal(0, result.RemovedDuplicates);
    }

    [Fact]
    public async Task Leaves_old_rows_the_bank_did_not_mention()
    {
        var other = Legacy(0, -9m, "Somewhere else");
        await _importer.Import(_card, new[] { Item("a", 0, -100) }, default);

        Assert.Contains(other, _repo.Rows);
        Assert.Null(other.ExternalId);
    }
}
