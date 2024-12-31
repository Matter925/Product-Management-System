using AutoMapper;

using ProductManagement.API.DTOs;
using ProductManagement.EFCore.IdentityModels;
using ProductManagement.EFCore.Models;

namespace ProductManagement.API.Helpers;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<Nationality, LookupDto>().ReverseMap();
        CreateMap<Language, LookupDto>().ReverseMap();
        CreateMap<Nationality, LookupDto>().ReverseMap();
        CreateMap<Language, LookupDto>().ReverseMap();
        CreateMap<Nationality, LookupDto>().ReverseMap();
        CreateMap<Language, LookupDto>().ReverseMap();
        CreateMap<PaymentsType, LookupDto>().ReverseMap();
        CreateMap<Audit, AuditDto>().ReverseMap();
        CreateMap<LoginLog, LoginLogDto>().ReverseMap();
        CreateMap<ApplicationUser, IdentityUserModel>().ReverseMap();
        CreateMap<ApplicationUserModel, ApplicationUser>().ReverseMap();
        

        CreateMap<Product, ProductDto>().ReverseMap();
        CreateMap<Product, CreateProductDto>().ReverseMap();
        CreateMap<Product, UpdateProductDto>().ReverseMap();

    }
}