using AutoMapper;
using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Repositories;
using OutlayApp.Domain.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlayApp.Application.ChooseClientCards.Commands
{
    public class ChooseClientCardsCommandHandler : ICommandHandler<ChooseClientCardsCommand, List<ClientCardDto>>
    {
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;
        public ChooseClientCardsCommandHandler(IClientRepository clientRepository, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
        }
        public async Task<Result<List<ClientCardDto>>> Handle(ChooseClientCardsCommand request, CancellationToken cancellationToken)
        {
            var client = await _clientRepository.GetByPersonalToken(request.clientToken);
            List<ClientCardDto> clientCards = _mapper.Map<List<ClientCardDto>>(client.Cards);

            return Result.Success(clientCards);
        }


    }
}
