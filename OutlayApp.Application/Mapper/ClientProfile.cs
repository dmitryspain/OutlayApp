using AutoMapper;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;
using OutlayApp.Domain.ClientTransactions;

namespace OutlayApp.Application.Mapper;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        CreateMap<Client, ClientDto>()
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name))
            .ForMember(x => x.Cards, opt => opt.MapFrom(x => x.Cards));
        CreateMap<ClientCard, ClientCardDto>();
        CreateMap<ClientTransaction, ClientTransactionDto>()
            .ForMember(x => x.DateOccured,
                opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.DateOccured).Date));
    }
}