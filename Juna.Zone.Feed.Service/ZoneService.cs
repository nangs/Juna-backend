using System;
using System.Linq;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using Juna.Feed.DomainModel;
using Juna.Feed.DomainModel.Builder;
using Juna.Feed.Repository;
using Juna.Feed.Repository.Util;
using Juna.Feed.Service.Helpers;
using Juna.Feed.Service.Interfaces;

namespace Juna.Feed.Service
{
    public class ZoneService : IZoneService
    {
        private ZoneRepository _ZoneRepositorý;

        public ZoneService(ZoneRepository ZoneRepository)
        {
            _ZoneRepositorý = ZoneRepository;
        }

        public List<Zone> GetZones()
        {
            return _ZoneRepositorý.GetAllZone();
        }

        public Zone SaveZone(Zone Zone)
        {
            var storedItem = _ZoneRepositorý.GetById(Zone.Id);

            if (storedItem != null)
            {
                throw new DuplicateEntityException($"zone with Id [{Zone.Id}] already exists");
            }

            return _ZoneRepositorý.Save(Zone);
        }

        public async Task<Zone> SaveZoneAsync(Zone Zone)
        {
            var storedItem = _ZoneRepositorý.GetById(Zone.Id);

            if (storedItem != null)
            {
                throw new DuplicateEntityException($"zone with Id [{Zone.Id}] already exists");
            }

            return await _ZoneRepositorý.SaveAsync(Zone);
        }

        public void DeleteZone(Guid id)
        {
            var Zone = new Zone();

            if (id != null)
            {
                Zone = _ZoneRepositorý.GetById(id);

                if (Zone != null)
                {
                    _ZoneRepositorý.Delete(Zone);
                }
            }
        }

        public Zone GetZone(Guid id)
        {
            var Zone = new Zone();

            if (id != null)
            {
                Zone = _ZoneRepositorý.GetById(id);
            }
            return Zone;
        }
    }
}
