using AutoMapper;
using System;
using Juna.Feed.DomainModel;
using Juna.Feed.Dao;

namespace Juna.Feed.Repository.Mapping
{
    public class ActivityProfile : Profile
    {
        public ActivityProfile()
        {
            CreateMap<Activity, ActivityDO>()
                        .ForMember(dest => dest.Type, opt => opt.Ignore())
                        .ReverseMap()
                        .ForMember(src => src.Id, opt => opt.MapFrom(dest => Guid.Parse(dest.Id)))
                        .PreserveReferences();
            CreateMap<Board, BoardDO>()
                        .ForMember(dest => dest.Type, opt => opt.Ignore())
                        .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate.ToString()))
                        .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate.ToString()))
                        .ReverseMap()
                        .ForMember(src => src.StartDate, opt => opt.MapFrom(dest => DateTime.Parse(dest.StartDate)))
                        .ForMember(src => src.EndDate, opt => opt.MapFrom(dest => DateTime.Parse(dest.EndDate)))
                        .ForMember(src => src.Id, opt => opt.MapFrom(dest => Guid.Parse(dest.Id)))
                        .PreserveReferences();
            CreateMap<Comment, CommentDO>()
						.IncludeBase<Activity, ActivityDO>()
                        .ReverseMap()
                        .ForMember(src => src.ParentCommentId, opt => opt.MapFrom(dest => Guid.Parse(dest.Id)))
                        .PreserveReferences();
        }
    }
}
