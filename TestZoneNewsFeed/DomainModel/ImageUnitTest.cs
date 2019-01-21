using System;

using Xunit;
using FluentAssertions;
using Juna.Zone.Feed.DomainModel;

namespace Juna.Zone.FeedFlows.DomainModel
{
	public class ImageUnitTest
	{
		[Theory]
		//todo: Enable after url validation
		//[InlineData(null, 5, 5)]
		//[InlineData("", 5, 5)]
		[InlineData("http://", -5, 5)]
		[InlineData("http://", 5, -5)]
		public void Cannot_Create_Image_With_Invalid_Data(string url, int height, int width)
		{
			Action action = () => new Image
			(
				url: url,
				height: height,
				width: width
			);

			action.Should().Throw<InvalidOperationException>();
		}
	}
}
