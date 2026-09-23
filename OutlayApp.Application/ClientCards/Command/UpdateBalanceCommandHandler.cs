using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Security;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.ClientCards.Command;

public class UpdateBalanceCommandHandler : ICommandHandler<UpdateBalanceCommand>
{
    private readonly IClientRepository _clientRepository;
    private readonly IMonobankClient _monobank;
    private readonly ITokenProtector _protector;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBalanceCommandHandler(IClientRepository clientRepository, IMonobankClient monobank,
        ITokenProtector protector, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _monobank = monobank;
        _protector = protector;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(UpdateBalanceCommand request, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetByIdWithCards(request.ClientId, cancellationToken);
        if (client?.EncryptedToken is null)
            return Result.Failure(new Error("Client.NotFound", "Клієнта не знайдено."));

        MonobankClientInfo info;
        try
        {
            info = await _monobank.GetClientInfo(_protector.Unprotect(client.EncryptedToken), cancellationToken);
        }
        catch (MonobankException ex)
        {
            return Result.Failure(MonobankErrors.From(ex));
        }

        foreach (var account in info.Accounts)
        {
            var card = client.Cards.FirstOrDefault(c => c.ExternalCardId == account.Id);
            if (card is null)
                client.AddCard(account.Id, account.Type, account.Balance / 100m, account.CreditLimit / 100m,
                    account.CurrencyCode, account.MaskedPan.FirstOrDefault(), account.Iban);
            else
                card.UpdateAccount(account.Balance / 100m, account.CreditLimit / 100m, account.Type,
                    account.MaskedPan.FirstOrDefault(), account.Iban);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
