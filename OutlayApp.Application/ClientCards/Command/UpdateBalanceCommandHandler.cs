using Microsoft.Extensions.Options;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Clients.Commands;
using OutlayApp.Application.Configuration.Monobank;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace OutlayApp.Application.ClientCards.Command
{
    public class UpdateBalanceCommandHandler : ICommandHandler<UpdateBalanceCommand>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly IOptions<MonobankSettings> _monobankSettings;


        public UpdateBalanceCommandHandler(IClientRepository clientRepository,
            IUnitOfWork unitOfWork, IOptions<MonobankSettings> monobankSettings)
        {
            _clientRepository = clientRepository;
            _unitOfWork = unitOfWork;
            _monobankSettings = monobankSettings;
        }
        public async Task<Result> Handle(UpdateBalanceCommand request, CancellationToken cancellationToken)
        {
            var exist = await _clientRepository.GetByPersonalToken(request.ClientToken, cancellationToken);

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(_monobankSettings.Value.BaseUrl);
            httpClient.DefaultRequestHeaders.Add(MonobankConstants.TokenHeader, request.ClientToken);

            var result = await httpClient.GetAsync("/personal/client-info", cancellationToken);
            var clientInfo = await result.Content.ReadFromJsonAsync<ClientInfo>(cancellationToken: cancellationToken);

            foreach (var item in exist.Cards)
            {
                var newBalance = clientInfo.Accounts.Where(x => x.Id == item.ExternalCardId).Select(x => x.Balance).FirstOrDefault();
                item.UpdateBalance(newBalance);
                await _unitOfWork.SaveChangesAsync();
            }
            return Result.Success();
        }
    }
}
