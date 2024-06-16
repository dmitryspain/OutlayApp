using AutoMapper;
using OutlayApp.Application.Clients.Queries.GetClientInfo;
using OutlayApp.Domain.ClientCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutlayApp.Infrastructure.Mapper
{
    public class ClientCardProfile : Profile
    {
        public ClientCardProfile()
        {
            CreateMap<ClientCard, ClientCardDto>()
        .ForMember(x => x.Id, opt => opt.MapFrom(x => x.Id))
        .ForMember(x => x.Balance, opt => opt.MapFrom(x => x.Balance))
        .ForMember(x => x.CurrencyCode, opt => opt.MapFrom(x => x.CurrencyCode))
        .ForMember(x => x.Type, opt => opt.MapFrom(x => x.Type));
        }
    }
}
