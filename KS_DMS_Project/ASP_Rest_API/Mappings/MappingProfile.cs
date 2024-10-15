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
                    => opt.MapFrom(src => $"*{src.Name ?? string.Empty}*"))
                .ForMember(dest => dest.Title, opt
                    => opt.MapFrom(src => src.IsComplete))
                .ReverseMap()
                .ForMember(dest => dest.Id, opt
                    => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt
                    => opt.MapFrom(src => (src.TextField ?? string.Empty).Replace("*", "")))
                .ForMember(dest => dest.IsComplete, opt
                    => opt.MapFrom(src => src.Title));



        }
    }
}
