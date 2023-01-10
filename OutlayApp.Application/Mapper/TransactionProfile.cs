using AutoMapper;

namespace OutlayApp.Application.Mapper;

public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        // CreateMap<MonobankTransaction, ClientTransaction>()
        //     .ForMember(x => x.Amount, opt => opt.MapFrom(x => x.Amount))
        //     .ForMember(x => x.DateOccured, opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.Time)));
        // CreateMap<BrandFetchInfo, TransactionResponse>()
        //     .ConvertUsing<TransactionConverter>();
        //
        // CreateMap<Transaction, TransactionByDescriptionResponse>()
        //     .ForMember(x => x.Amount, opt => opt.MapFrom(x => x.Amount / 100))
        //     .ForMember(x => x.Date, opt => opt.MapFrom(x => DateTimeOffset.FromUnixTimeSeconds(x.Time)
        //         .LocalDateTime.ToString(CultureInfo.InvariantCulture)))
        //     .ForMember(x => x.Name, opt => opt.MapFrom(x => x.Description));
    }
}