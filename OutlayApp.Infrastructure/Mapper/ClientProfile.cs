using AutoMapper;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Domain.ClientCards;
using OutlayApp.Domain.Clients;

namespace OutlayApp.Infrastructure.Mapper;

public class ClientProfile : Profile
{
    public ClientProfile()
    {
        CreateMap<Client, ClientDto>()
            .ForMember(x => x.FullName, opt => opt.MapFrom(x => x.Name))
            .ForMember(x => x.Cards, opt => opt.MapFrom(x => x.Cards));
        CreateMap<ClientCard, ClientCardDto>();
    }
}