﻿using ASP_Rest_API.DTO;
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
                    => opt.MapFrom(src => $"{src.author ?? string.Empty}"))
                .ForMember(dest => dest.title, opt
                    => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.textfield, opt
                    => opt.MapFrom(src => src.textfield))
                .ForMember(dest => dest.createddate, opt
                    => opt.MapFrom(src => src.createddate))
                .ForMember(dest => dest.editeddate, opt
                    => opt.MapFrom(src => src.editeddate))
                .ForMember(dest => dest.filename, opt
                    => opt.MapFrom(src => src.filename))
                .ForMember(dest => dest.ocrtext, opt
                    => opt.MapFrom(src => src.ocrtext))
                .ReverseMap()
                .ForMember(dest => dest.id, opt
                    => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.author, opt
                    => opt.MapFrom(src => $"{src.author ?? string.Empty}"))
                .ForMember(dest => dest.title, opt
                    => opt.MapFrom(src => src.title))
                .ForMember(dest => dest.textfield, opt
                    => opt.MapFrom(src => src.textfield))
                .ForMember(dest => dest.createddate, opt
                    => opt.MapFrom(src => src.createddate))
                .ForMember(dest => dest.editeddate, opt
                    => opt.MapFrom(src => src.editeddate))
                .ForMember(dest => dest.filename, opt
                    => opt.MapFrom(src => src.filename))
                .ForMember(dest => dest.ocrtext, opt
                    => opt.MapFrom(src => src.ocrtext));



        }
    }
}
