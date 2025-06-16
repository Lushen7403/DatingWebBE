using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using AutoMapper;

namespace MatchLoveWeb.Mapping
{
    public class MappingProfile : AutoMapper.Profile
    {
        public MappingProfile()
        {

            CreateMap<LoginRequestDTO, Account>();
            CreateMap<RegisterRequestDTO, Account>();
            CreateMap<ProfileDTO, Models.Profile>()
            .ForMember(dest => dest.ProfileImages, opt => opt.Ignore()) // Bỏ qua ProfileImages vì nó được xử lý riêng
            .ForMember(dest => dest.Avatar, opt => opt.Ignore()) // Bỏ qua Avatar vì nó được xử lý riêng
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Account, opt => opt.Ignore()) // Bỏ qua navigation property
            .ForMember(dest => dest.Gender, opt => opt.Ignore()); // Bỏ qua navigation property
        }
    }
}
