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
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => $"*{src.Author ?? string.Empty}*"))
                .ForMember(dest => dest.Title, opt
                    => opt.MapFrom(src => src.Titel))
                .ForMember(dest => dest.TextField, opt
                    => opt.MapFrom(src => src.Textfield))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Author, opt
                    => opt.MapFrom(src => $"*{src.Author ?? string.Empty}*"))
                .ForMember(dest => dest.Titel, opt
                    => opt.MapFrom(src => src.Title))
                .ForMember(dest => dest.Textfield, opt
                    => opt.MapFrom(src => src.TextField));



        }
    }
}
