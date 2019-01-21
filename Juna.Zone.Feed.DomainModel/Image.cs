using Juna.DDDCore.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Juna.Feed.DomainModel
{
	public class Image //: ValueObject<Image> DDD will have to wait for a bit
	{
		[JsonProperty("imageUrl")]
		public string ImageUrl { get; set; }
		[JsonProperty("height")]
		public int ImageHeight { get; set; }
		[JsonProperty("width")]
		public int ImageWidth { get; set; }

		// todo: Doing this only because of Automapper
		public Image() { }

		public Image(string url, int height, int width)
		{
			// todo: Validate that url is a proper url
			//if (string.IsNullOrEmpty(url))
			//	throw new InvalidOperationException();
			if (height < 0)
				throw new InvalidOperationException();
			if (width < 0)
				throw new InvalidOperationException();

			ImageUrl = url;
			ImageHeight = height;
			ImageWidth = width;
		}

		//protected override IEnumerable<object> GetEqualityComponents()
		//{
		//	yield return ImageUrl;
		//	yield return ImageHeight;
		//	yield return ImageWidth;
		//}
	}
}
