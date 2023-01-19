using System.Globalization;
using AutoMapper;
using OutlayApp.Application;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Infrastructure.Mapper.TypeConverters;

namespace OutlayApp.Infrastructure.Mapper;

public class ClientTransactionsProfile : Profile
{
    public ClientTransactionsProfile()
    {
        CreateMap<BrandFetchInfo, ClientTransactionsGroupedResponse>()
            .ConvertUsing<ClientTransactionConverter>();
        
        CreateMap<ClientTransaction, ClientTransactionByDescriptionResponse>()
            .ForMember(x => x.Date, opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.DateOccured)
                .LocalDateTime.ToString(CultureInfo.InvariantCulture)))
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Description));
    }
}