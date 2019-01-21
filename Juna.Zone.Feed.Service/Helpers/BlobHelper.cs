using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Juna.Feed.Service.Helpers
{
    public class BlobHelper
    {
		private readonly string _storageUrl;
		public BlobHelper(string storageUrl)
		{
			_storageUrl = storageUrl;
		}

        public string GenerateFilePath(Guid guid, string itemType, string username)
        {
            // todo: Currently the generated content url is too long. We need to find a way to shorten this url
            return $"{_storageUrl}/{username}/{guid.ToString()}/{DateTime.UtcNow.Ticks}.{itemType}";
        }

		public string GenerateThumbnailFilePath(Guid guid, string itemType, string username)
		{
			return $"{_storageUrl}/{username}/{guid.ToString()}/thumbnails/{DateTime.UtcNow.Ticks}.{itemType}";
		}

		public Guid GetFileGuidFromFilePath(string filePath)
		{
			return Guid.Parse(filePath.Split(new char[]{ '/' }).Last());
		}
    }
}
