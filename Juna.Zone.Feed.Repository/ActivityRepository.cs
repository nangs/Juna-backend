using AutoMapper;
using Juna.Feed.DomainModel;
using Juna.Feed.Dao;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.Repository
{
    public class ActivityRepository : DocumentDbRepository<Activity, ActivityDO>
    {
        private IMapper _mapper;
        private Uri CollectionUri { get; set; }
        public ActivityRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper)
            : base(documentDbUtil, collectionName)
        {
            _mapper = mapper;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
        }
        public override Activity GetById(Guid Id)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                            .Where(n => n.Id.Equals(Id.ToString()) && n.Type == typeof(ActivityDO).ToString())
                            .AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Activity>(item);
        }

        public Activity GetByActorVerbAndObject(string actor, string verb, string objectString)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                           .Where(n => 
                                n.Actor.Equals(actor) && 
                                n.Verb.Equals(verb) &&
                                n.Object.Equals(objectString) &&
                                n.Type == typeof(ActivityDO).ToString()
                            ).AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Activity>(item);
        }

        public Activity[] GetByVerbAndObject(string verb, string objectString)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                           .Where(n =>
                                n.Verb.Equals(verb) &&
                                n.Object.Equals(objectString) &&
                                n.Type == typeof(ActivityDO).ToString()
                            ).AsEnumerable().ToList();
            return item == null
                ? null
                : _mapper.Map<Activity[]>(item);
        }

        public Activity[] GetByVerbAndActor(string verb, string actor)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                           .Where(n =>
                                n.Verb.Equals(verb) &&
                                n.Actor.Equals(actor) &&
                                n.Type == typeof(ActivityDO).ToString()
                            ).AsEnumerable().ToList();
            return item == null
                ? null
                : _mapper.Map<Activity[]>(item);
        }

        public Activity GetByActorVerbObjectandTarget(string actor, string verb, string objectString , string target)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                           .Where(n =>
                                n.Actor.Equals(actor) &&
                                n.Verb.Equals(verb) &&
                                n.Object.Equals(objectString) &&
                                n.Target.Equals(target)&&
                                n.Type == typeof(ActivityDO).ToString()
                            ).AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Activity>(item);
        }

        public Activity[] GetByBoardId(string target)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
                           .Where(n =>
                                n.Target.Equals(target)
                            ).AsEnumerable().ToList();
            return item == null
                ? null
                : _mapper.Map<Activity[]>(item);
        }

        public void Delete(Activity activity)
		{
			// todo:We are querying for activity twice. Once in the module and once in repository. We need to remove this duplication
			var item = DbUtil.Client.CreateDocumentQuery<ActivityDO>(CollectionUri)
							.Where(n => n.Id.Equals(activity.Id.ToString()) && n.Type == typeof(ActivityDO).ToString())
							.AsEnumerable().FirstOrDefault();
			var result = DbUtil.Client.DeleteDocumentAsync(item.SelfLink);   
        }

        // todo: Figure out a way to make this async
        public override Activity Save(Activity activity)
        {
            var activityDO = _mapper.Map<ActivityDO>(activity);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            activityDO.Type = activityDO.GetType().ToString();
            activityDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new activity with id[{activityDO.Id}]");
            DbUtil.Client.CreateDocumentAsync(CollectionUri, activityDO);
            activity = _mapper.Map<Activity>(activityDO);
            return activity;
        }

        // Praneeth - This is the only PersistentRepository method that exposes the Data Object
        // todo: Find a way to manipulate the inheritance hierarchy to return Feeditem
        // which is a domain object, but QueryAndContinueAsync has nothing to do with domain
        public async Task<DocumentDbQueryResult<ActivityDO>> QueryAndContinueAsync(string continuationToken)
        {
            // todo: This could become a performance issue later on
            // todo: Ugly hack for orderBy descending Ugh!!
            return await base.QueryAndContinue(
                    continuationToken, 20, null, null, "time DESC", null);
        }

        public override Activity[] ConvertToDomainEntities(ActivityDO[] daos)
        {
            return daos?.Select(d => _mapper.Map<Activity>(d)).ToArray();
        }

        public override async Task<Activity> SaveAsync(Activity activity)
        {
            var activityDO = _mapper.Map<ActivityDO>(activity);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            activityDO.Type = activityDO.GetType().ToString();
            activityDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new activity with id[{activityDO.Id}]");
            await DbUtil.Client.CreateDocumentAsync(CollectionUri, activityDO);
            activity = _mapper.Map<Activity>(activityDO);
            return activity;
        }
    }
}
