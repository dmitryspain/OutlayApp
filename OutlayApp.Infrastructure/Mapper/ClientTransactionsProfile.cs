using AutoMapper;
using OutlayApp.Application.ClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactions;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsByDescription;
using OutlayApp.Application.ClientTransactions.Queries.GetClientTransactionsGrouped;
using OutlayApp.Domain.ClientTransactions;
using OutlayApp.Infrastructure.Mapper.TypeConverters;

namespace OutlayApp.Infrastructure.Mapper;

public class ClientTransactionsProfile : Profile
{
    public ClientTransactionsProfile()
    {
        CreateMap<GroupedTransaction, ClientTransactionsGroupedResponse>()
            .ConvertUsing<ClientTransactionConverter>();
        
        CreateMap<ClientTransaction, ClientTransactionByDescriptionResponse>()
            // .ForMember(x => x.DateOccured, opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.DateOccured)
            //     .LocalDateTime.ToString(CultureInfo.InvariantCulture)))
            .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Description));

        CreateMap<ClientTransaction, ClientTransactionDto>();
        // .ForMember(x => x.DateOccured,
        //     opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.DateOccured).LocalDateTime));
    }
}