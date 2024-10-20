using ASP_Rest_API.DTO;
using AutoMapper;
using MyDocDAL.Entities;

namespace ASP_Rest_API.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<MyDoc, MyDocDTO>()
                .ForMember(dest => dest.id, opt
                    => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.author, opt
                    => opt.MapFrom(src => $"*{src.author ?? string.Empty}*"))
                .ForMember(dest => dest.title, opt
                    => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.textfield, opt
                    => opt.MapFrom(src => src.textfield))
                .ReverseMap()
                .ForMember(dest => dest.id, opt
                    => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.author, opt
                    => opt.MapFrom(src => $"*{src.author ?? string.Empty}*"))
                .ForMember(dest => dest.title, opt
                    => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.textfield, opt
                    => opt.MapFrom(src => src.textfield));



        }
    }
}
