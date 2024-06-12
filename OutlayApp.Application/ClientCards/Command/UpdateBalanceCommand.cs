using OutlayApp.Application.Abstractions.Messaging;
using OutlayApp.Application.Clients.Commands;
using OutlayApp.Domain.ClientCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlayApp.Application.ClientCards.Command;
public sealed record class UpdateBalanceCommand(string ClientToken) : ICommand;
