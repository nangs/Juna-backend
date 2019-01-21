using AutoMapper;
using Juna.Feed.DomainModel;
using Juna.Feed.Dao;
using Microsoft.Azure.Documents.Client;
using System;
using System.Diagnostics;
using System.Linq;
using Juna.Feed.Repository.Util;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Juna.Feed.Repository
{
	public class JunaUserRepository : DocumentDbRepository<JunaUser, JunaUserDO>
	{
		private IMapper _mapper;
		private Uri CollectionUri { get; set; }

		public JunaUserRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper)
			: base(documentDbUtil, collectionName)
		{
			_mapper = mapper;
			CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
		}

		public override JunaUser GetById(Guid Id)
		{
			var user = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
							.Where(u => u.Id.Equals(Id) && u.Type == typeof(JunaUserDO).ToString())
							.AsEnumerable().SingleOrDefault();
			return user == null ? null : _mapper.Map<JunaUser>(user);
		}

        public List<JunaUser> GetByObjectIds(IList<string> ids)
        {
            var users = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
                            .Where(u => ids.Contains(u.ObjectId) && u.Type == typeof(JunaUserDO).ToString())
                            .AsEnumerable().ToList();
            return (users == null || users.Count == 0) ? null : _mapper.Map<List<JunaUser>>(users);
        }

        public List<JunaUser> GetByDisplayName(string displayName)
        {
            var users = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
                            .Where(u => u.DisplayName.ToLower().Contains(displayName.ToLower()) && u.Type == typeof(JunaUserDO).ToString())
                            .AsEnumerable().ToList();
            return (users == null || users.Count == 0) ? null : _mapper.Map<List<JunaUser>>(users);
        }


        public JunaUser GetByEmail(string email)
		{
			var user = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
							.Where(u => u.EmailAddress.Equals(email) && u.Type == typeof(JunaUserDO).ToString())
							.AsEnumerable().SingleOrDefault();
			return user == null ? null : _mapper.Map<JunaUser>(user);
		}

        public JunaUser GetByEmailAndObjectId(string email, string oid)
        {
            var user = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
                            .Where(u => u.EmailAddress.Equals(email) && 
                            u.ObjectId.Equals(oid) &&
                            u.Type == typeof(JunaUserDO).ToString())
                            .AsEnumerable().SingleOrDefault();
            return user == null ? null : _mapper.Map<JunaUser>(user);
        }

        public JunaUser GetByObjectId(string oid)
		{
			var item = DbUtil.Client.CreateDocumentQuery<JunaUserDO>(CollectionUri)
							.Where(u => oid.Equals(u.ObjectId) && u.Type == typeof(JunaUserDO).ToString())
							.AsEnumerable().FirstOrDefault();
			return item == null ? null : _mapper.Map<JunaUser>(item);
		}

		// todo: Figure out a way to make this async
		public override JunaUser Save(JunaUser user)
		{
			var userDO = _mapper.Map<JunaUserDO>(user);
			// todo: This contains a major flaw. What if the type method doesn't match the class name?
			// There are not checks currently
			// todo: Move to a factory method or a builder method
			userDO.Type = userDO.GetType().ToString();
			userDO.Id = Guid.NewGuid().ToString();
			DbUtil.Client.CreateDocumentAsync(CollectionUri, userDO);
			Trace.TraceInformation($"Inserting new user with id[{userDO.Id}]");
			user = _mapper.Map<JunaUser>(userDO);
			return user;
		}

		public override JunaUser[] ConvertToDomainEntities(JunaUserDO[] daos)
		{
			return daos?.Select(d => _mapper.Map<JunaUser>(d)).ToArray();
		}

        public override async Task<JunaUser> SaveAsync(JunaUser user)
        {
            var userDO = _mapper.Map<JunaUserDO>(user);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are not checks currently
            // todo: Move to a factory method or a builder method
            userDO.Type = userDO.GetType().ToString();
            userDO.Id = Guid.NewGuid().ToString();
            await DbUtil.Client.CreateDocumentAsync(CollectionUri, userDO);
            Trace.TraceInformation($"Inserting new user with id[{userDO.Id}]");
            user = _mapper.Map<JunaUser>(userDO);
            return user;
        }
    }
}
