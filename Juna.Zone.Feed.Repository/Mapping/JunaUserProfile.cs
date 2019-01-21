using AutoMapper;
using Juna.Feed.Dao;
using Juna.Feed.DomainModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Juna.Feed.Repository.Mapping
{
    public class JunaUserProfile: Profile
    {
		public JunaUserProfile()
		{
			CreateMap<JunaUser, JunaUserDO>().ReverseMap();
			CreateMap<Image, Image>();
		}
    }
}
