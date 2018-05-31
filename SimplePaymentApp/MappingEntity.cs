using System;
using AutoMapper;
using SimplePaymentApp.Models;

namespace SimplePaymentApp
{
    public class MappingEntity : Profile  
    {  
        public MappingEntity()  
        {
            CreateMap<PaymentModel, TokenModel>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.Month, opt => opt.MapFrom(src => src.Valid.Value.Month))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Valid.Value.Year))
                .ForMember(dest => dest.Verification, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.Amount3D, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Holder, opt => opt.MapFrom(src => Uri.EscapeUriString(src.Name)));
        }  
    } 
}
