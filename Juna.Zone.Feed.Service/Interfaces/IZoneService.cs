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

namespace Juna.Feed.Service.Interfaces
{
    public interface IZoneService
    {
        List<Zone> GetZones();

        Zone SaveZone(Zone Zone);

        Task<Zone> SaveZoneAsync(Zone Zone);

        void DeleteZone(Guid id);

        Zone GetZone(Guid id);
    }
}
