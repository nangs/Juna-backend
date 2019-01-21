using AutoMapper;
using Juna.Feed.Dao;
using Juna.Feed.DomainModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Repository.Mapping
{
	public class FeedItemProfile : Profile
	{
		public FeedItemProfile()
		{
			CreateMap<FeedItem, FeedItemDO>()
						.ForMember(dest => dest.Type, opt => opt.Ignore())
						.ForMember(dest => dest.Thumbnail, opt => opt.MapFrom(src => new Image(src.Thumbnail.ImageUrl,
									src.Thumbnail.ImageHeight,
									src.Thumbnail.ImageWidth)))
						.ReverseMap()
						.ForMember(src => src.Id, opt => opt.MapFrom(dest => Guid.Parse(dest.Id)))
						.ForMember(src => src.Thumbnail, opt => opt.MapFrom(dest => new Image(dest.Thumbnail.ImageUrl,
									dest.Thumbnail.ImageHeight,
									dest.Thumbnail.ImageWidth)))
						.PreserveReferences();
		}
	}
}
