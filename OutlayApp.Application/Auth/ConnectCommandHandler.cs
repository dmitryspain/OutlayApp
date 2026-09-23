using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Monobank;
using OutlayApp.Application.Security;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Sessions;
using OutlayApp.Domain.Shared;

namespace OutlayApp.Application.Auth;

public class ConnectCommandHandler : ICommandHandler<ConnectCommand, ConnectResult>
{
    private readonly IClientRepository _clientRepository;
    private readonly IClientSessionRepository _sessionRepository;
    private readonly IMonobankClient _monobank;
    private readonly ITokenProtector _protector;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectCommandHandler(IClientRepository clientRepository, IClientSessionRepository sessionRepository,
        IMonobankClient monobank, ITokenProtector protector, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _sessionRepository = sessionRepository;
        _monobank = monobank;
        _protector = protector;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ConnectResult>> Handle(ConnectCommand request, CancellationToken cancellationToken)
    {
        var token = request.MonobankToken.Trim();
        if (token.Length < 20)
            return Result.Failure<ConnectResult>(new Error("Connect.InvalidToken", "Це не схоже на токен Монобанку."));

        // knowing the token is the proof: an existing client needs no call to the (rate-limited) bank
        var client = await _clientRepository.GetByTokenHash(TokenHash.Of(token), cancellationToken);
        if (client is null)
        {
            MonobankClientInfo info;
            try
            {
                info = await _monobank.GetClientInfo(token, cancellationToken);
            }
            catch (MonobankException ex)
            {
                return Result.Failure<ConnectResult>(MonobankErrors.From(ex));
            }

            client = Client.Create(info.Name, token, _protector.Protect(token));
            foreach (var a in info.Accounts)
            {
                client.AddCard(a.Id, a.Type, a.Balance / 100m, a.CreditLimit / 100m, a.CurrencyCode,
                    a.MaskedPan.FirstOrDefault(), a.Iban);
            }
            await _clientRepository.AddAsync(client, cancellationToken);
        }

        var (session, accessToken) = ClientSession.Open(client.Id, DateTime.UtcNow);
        await _sessionRepository.AddAsync(session, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ConnectResult(accessToken, DefaultCard(client.Cards)?.Id, client.Name);
    }

    /// <summary>The card with money on it; a zero-balance account is rarely the one people use.</summary>
    private static ClientCard? DefaultCard(IReadOnlyCollection<ClientCard> cards) =>
        cards.Where(c => c.Balance != 0).MaxBy(c => c.Balance) ?? cards.FirstOrDefault();
}
