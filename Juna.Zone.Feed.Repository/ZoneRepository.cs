using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Juna.Feed.Dao;
using Juna.Feed.DomainModel;
using Juna.Feed.Repository.Util;
using Microsoft.Azure.Documents.Client;

namespace Juna.Feed.Repository
{
    public class ZoneRepository : DocumentDbRepository<Zone, ZoneDO>
    {
        private IMapper _mapper;
        private Uri CollectionUri { get; set; }

        public ZoneRepository(DocumentDbUtil documentDbUtil, string collectionName, IMapper mapper) : base(documentDbUtil, collectionName)
        {
            _mapper = mapper;
            CollectionUri = UriFactory.CreateDocumentCollectionUri(documentDbUtil.DatabaseName, collectionName);
        }

        public override Zone[] ConvertToDomainEntities(ZoneDO[] daos)
        {
            return daos?.Select(d => _mapper.Map<Zone>(d)).ToArray();
        }

        public override Zone GetById(Guid Id)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ZoneDO>(CollectionUri)
                            .Where(b => b.Id.Equals(Id.ToString()) && b.Type == typeof(ZoneDO).ToString())
                            .AsEnumerable().FirstOrDefault();
            return item == null
                ? null
                : _mapper.Map<Zone>(item);
        }

        public override Zone Save(Zone Zone)
        {
            var ZoneDO = _mapper.Map<ZoneDO>(Zone);

            ZoneDO.Type = ZoneDO.GetType().ToString();
            ZoneDO.Id = Guid.NewGuid().ToString();

            Trace.TraceInformation($"Inserting new board with id[{ZoneDO.Id}]");
            DbUtil.Client.CreateDocumentAsync(CollectionUri, ZoneDO);

            Zone = _mapper.Map<Zone>(ZoneDO);

            return Zone;
        }

        public override async Task<Zone> SaveAsync(Zone Zone)
        {
            var ZoneDO = _mapper.Map<ZoneDO>(Zone);

            ZoneDO.Type = ZoneDO.GetType().ToString();
            ZoneDO.Id = Guid.NewGuid().ToString();

            Trace.TraceInformation($"Inserting new board with id[{ZoneDO.Id}]");

            await DbUtil.Client.CreateDocumentAsync(CollectionUri, ZoneDO);

            Zone = _mapper.Map<Zone>(ZoneDO);

            return Zone;
        }

        public void Delete(Zone Zone)
        {
            var item = DbUtil.Client.CreateDocumentQuery<ZoneDO>(CollectionUri)
                            .Where(n => n.Id.Equals(Zone.Id.ToString()) && n.Type == typeof(ZoneDO).ToString())
                            .AsEnumerable().FirstOrDefault();

            var result = DbUtil.Client.DeleteDocumentAsync(item.SelfLink);
        }

        public Zone Upsert(Zone Zone)
        {
            var ZoneDO = _mapper.Map<ZoneDO>(Zone);
            ZoneDO.Type = ZoneDO.GetType().ToString();

            Trace.TraceInformation($"Upserting Zone item with id [{ Zone.Id }]");
            DbUtil.Client.UpsertDocumentAsync(CollectionUri, Zone);

            Zone = _mapper.Map<Zone>(ZoneDO);

            return Zone;
        }

        public List<Zone> GetAllZone()
        {
            var item = DbUtil.Client.CreateDocumentQuery<ZoneDO>(CollectionUri)
                        .AsEnumerable().ToList();

            return item == null ? null : _mapper.Map<List<Zone>>(item);
        }
    }
}
