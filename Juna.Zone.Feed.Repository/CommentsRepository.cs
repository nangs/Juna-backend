using AutoMapper;
using Juna.Feed.DomainModel;
using Juna.Feed.Dao;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents.Client;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Juna.Feed.Repository
{
    public class CommentsRepository : DocumentDbRepository<Comment, CommentDO>
    {
        private IMapper _mapper;
        private Uri CollectionUri { get; set; }

        public CommentsRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper)
            : base(documentDbUtil, collectionName)
        {
            _mapper = mapper;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
        }

        public override Comment GetById(Guid Id)
        {
            var item = DbUtil.Client.CreateDocumentQuery<CommentDO>(CollectionUri)
                            .Where(n => n.Id.Equals(Id.ToString()) && n.Type == typeof(CommentDO).ToString())
                            .AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Comment>(item);
        }


        public void Delete(Comment activity)
        {
            // todo:We are querying for activity twice. Once in the module and once in repository. We need to remove this duplication
            var item = DbUtil.Client.CreateDocumentQuery<CommentDO>(CollectionUri)
                            .Where(n => n.Id.Equals(activity.Id.ToString()) && n.Type == typeof(CommentDO).ToString())
                            .AsEnumerable().FirstOrDefault();
            var result = DbUtil.Client.DeleteDocumentAsync(item.SelfLink);
        }

        public List<Comment> GetByFeedItemAndVerb(FeedItem feedItem, string verb)
        {
            var item = DbUtil.Client.CreateDocumentQuery<CommentDO>(CollectionUri)
                           .Where(n =>
                                n.Verb.Equals(verb) &&
                                n.Object.Equals($"{feedItem.ContentType}:{feedItem.Id}") &&
                                n.Type == typeof(CommentDO).ToString()
                            ).AsEnumerable().ToList();
            return item == null
                ? null
                : _mapper.Map<List<Comment>>(item);
        }

        public List<Comment> GetByParentCommentId(Guid id)
        {
            var item = DbUtil.Client.CreateDocumentQuery<CommentDO>(CollectionUri)
                           .Where(n => n.ParentCommentId.Equals($"Comment:{id.ToString()}") && n.Type == typeof(CommentDO).ToString()
                            ).AsEnumerable().ToList();
            return item == null
                ? null
                : _mapper.Map<List<Comment>>(item);
        }





        public Comment GetByActorVerbObjectAndTimestamp(string actor, string verb, string objectString, string timestamp, string message)
        {
            var item = DbUtil.Client.CreateDocumentQuery<CommentDO>(CollectionUri)
                           .Where(n =>
                                n.Actor.Equals(actor) &&
                                n.Verb.Equals(verb) &&
                                n.Object.Equals(objectString) &&
                                n.Time.Equals(timestamp) &&
                                n.Message.Equals(message)&&
                                n.Type == typeof(CommentDO).ToString()
                            ).AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Comment>(item);
        }

        // todo: Figure out a way to make this async
        public override Comment Save(Comment comment)
        {
            var commentDO = _mapper.Map<CommentDO>(comment);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            commentDO.Type = commentDO.GetType().ToString();
            commentDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new activity with id[{commentDO.Id}]");
            DbUtil.Client.CreateDocumentAsync(CollectionUri, commentDO);
            comment = _mapper.Map<Comment>(commentDO);
            return comment;
        }

        // Praneeth - This is the only PersistentRepository method that exposes the Data Object
        // todo: Find a way to manipulate the inheritance hierarchy to return Feeditem
        // which is a domain object, but QueryAndContinueAsync has nothing to do with domain
        public async Task<DocumentDbQueryResult<CommentDO>> QueryAndContinueAsync(string continuationToken)
        {
            // todo: This could become a performance issue later on
            // todo: Ugly hack for orderBy descending Ugh!!
            return await base.QueryAndContinue(
                    continuationToken, 20, null, null, "time DESC", null);
        }

        public override Comment[] ConvertToDomainEntities(CommentDO[] daos)
        {
            return daos?.Select(d => _mapper.Map<Comment>(d)).ToArray();
        }

        public override async Task<Comment> SaveAsync(Comment comment)
        {
            var commentDO = _mapper.Map<CommentDO>(comment);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            commentDO.Type = commentDO.GetType().ToString();
            commentDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new activity with id[{commentDO.Id}]");
            await DbUtil.Client.CreateDocumentAsync(CollectionUri, commentDO);
            comment = _mapper.Map<Comment>(commentDO);
            return comment;
        }
    }
}
