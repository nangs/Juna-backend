using AutoMapper;
using Juna.Feed.Dao;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.Repository
{
    public class BoardRepository : DocumentDbRepository<Board, BoardDO>
    {
        private IMapper _mapper;
        private Uri CollectionUri { get; set; }

        public BoardRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper)
            : base(documentDbUtil, collectionName)
        {
            _mapper = mapper;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
        }

        public override Board GetById(Guid Id)
        {
            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                            .Where(b => b.Id.Equals(Id.ToString()) && b.Type == typeof(BoardDO).ToString())
                            .AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Board>(item);
        }

        public Board GetByBoardEvent(BoardEvent boardEvent)
        {
            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                            .Where(b => b.BoardEvent.Type == boardEvent.Type && b.BoardEvent.ForeignId == boardEvent.ForeignId)
                            .AsEnumerable().FirstOrDefault();
            return item == null ? null : _mapper.Map<Board>(item);
        }

        public Board GetByBoardEventAndForeignId(BoardEvent boardEvent, long foreignId)
        {
            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                            .Where(b => b.BoardEvent.Type == boardEvent.Type && b.BoardEvent.ForeignId == foreignId)
                            .AsEnumerable().FirstOrDefault();
            return item == null ? null : _mapper.Map<Board>(item);
        }

        public List<Board> GetByDate(string date)
        {

            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                        .Where(b => b.StartDate == date)
                        .AsEnumerable().ToList();
            return item == null ? null : _mapper.Map<List<Board>>(item);
        }

        public List<Board> GetBoardsCreatedByUser(JunaUser user)
        {
            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                        .Where(b => b.CreatedBy.ObjectId == user.ObjectId && b.BoardType == "private")
                        .AsEnumerable().ToList();
            return item == null ? null : _mapper.Map<List<Board>>(item);
        }

        public List<Board> GetAllBoards()
        {
            var item = DbUtil.Client.CreateDocumentQuery<BoardDO>(CollectionUri)
                        .AsEnumerable().ToList();
            return item == null ? null : _mapper.Map<List<Board>>(item);
        }

        // todo: Figure out a way to make this async
        public override Board Save(Board boards)
        {
            var boardsDO = _mapper.Map<BoardDO>(boards);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            boardsDO.Type = boardsDO.GetType().ToString();
            boardsDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new board with id[{boardsDO.Id}]");
            DbUtil.Client.CreateDocumentAsync(CollectionUri, boardsDO);
            boards = _mapper.Map<Board>(boardsDO);
            return boards;
        }

        public Board Upsert(Board board)
        {
            var boardDO = _mapper.Map<BoardDO>(board);
            boardDO.Type = boardDO.GetType().ToString();
            Trace.TraceInformation($"Upserting board with id [{ board.Id }]");
            DbUtil.Client.UpsertDocumentAsync(CollectionUri, boardDO);
            board = _mapper.Map<Board>(boardDO);
            return board;
        }

        // Praneeth - This is the only PersistentRepository method that exposes the Data Object
        // todo: Find a way to manipulate the inheritance hierarchy to return Feeditem
        // which is a domain object, but QueryAndContinueAsync has nothing to do with domain
        public async Task<DocumentDbQueryResult<BoardDO>> QueryAndContinueAsync(string continuationToken)
        {
            // todo: This could become a performance issue later on
            // todo: Ugly hack for orderBy descending Ugh!!
            return await base.QueryAndContinue(
                    continuationToken, 20, null, null, "time DESC", null);
        }

        public override Board[] ConvertToDomainEntities(BoardDO[] daos)
        {
            return daos?.Select(d => _mapper.Map<Board>(d)).ToArray();
        }

        public override async Task<Board> SaveAsync(Board boards)
        {
            var boardsDO = _mapper.Map<BoardDO>(boards);
            // todo: This contains a major flaw. What if the type method doesn't match the class name?
            // There are no checks currently
            // todo: Move to a factory method or a builder method
            boardsDO.Type = boardsDO.GetType().ToString();
            boardsDO.Id = Guid.NewGuid().ToString();
            Trace.TraceInformation($"Inserting new board with id[{boardsDO.Id}]");
            await DbUtil.Client.CreateDocumentAsync(CollectionUri, boardsDO);
            boards = _mapper.Map<Board>(boardsDO);
            return boards;
        }

        public Boolean IsActive(string id)
        {
            var boardId = Guid.Parse((string)id);
            var board = GetById(boardId);
            var now = DateTime.UtcNow;
            return board.StartDate < now && now < board.EndDate;
        }
    }
}
