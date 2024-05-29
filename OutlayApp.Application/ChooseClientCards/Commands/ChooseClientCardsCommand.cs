using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlayApp.Application.ChooseClientCards.Commands
{
    public sealed record class ChooseClientCardsCommand(string clientToken) : ICommand<List<ClientCardDto>>;
}
