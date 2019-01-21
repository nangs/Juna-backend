using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
//using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Juna.Feed.Service.Helpers;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;

namespace Juna.Feed.Service
{
	public class ThumbnailService
    {
		private long defaultWidth;
		private long defaultHeight;
		private HttpClient thumbnailServiceApiClient;
		private readonly StorageCredentials storageCreds;
		private readonly string thumbnailServiceUri;
        private BlobHelper pBlobHelper;
        private readonly string pResourceGroup;
        private readonly string pAzureAccountName;
        private IAzureMediaServicesClient pAzureMediaServicesClient;

        public ThumbnailService(
			HttpClient client, 
			string serviceUri,
			long thumbnailWidth, 
			long thumbnailHeight, 
			StorageCredentials creds,
			BlobHelper blobHelper,
			string resourceGroup,
            string azureAccountName,
            IAzureMediaServicesClient azureMediaServicesClient
            )
		{
			thumbnailServiceApiClient = client;
			thumbnailServiceUri = serviceUri;
			defaultHeight = thumbnailHeight;
			defaultWidth = thumbnailWidth;
			storageCreds = creds;
            pBlobHelper = blobHelper;
            pResourceGroup = resourceGroup;
            pAzureAccountName = azureAccountName;
            pAzureMediaServicesClient = azureMediaServicesClient;
        }

        public async Task GenerateThumbnail(string fileUrl, string thumbnailStoragePath, string mimeType, string title)
        {
            switch (mimeType)
            {
                // todo: Use constants
                case ("image/jpeg"):
                case ("image/jpg"):
                case ("image/png"):
                case ("image/gif"):
                case ("image/bmp"):
                    await GenerateImageThumbnail(fileUrl, thumbnailStoragePath);
                    break;
                case ("video/mp4"):
                    await GenerateVideoThumbnailAsync(fileUrl, thumbnailStoragePath, pResourceGroup, pAzureAccountName);
                    break;
            }
        }

        private async Task GenerateImageThumbnail(string fileUrl, string thumbnailStoragePath)
        {
            byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                Url = fileUrl
            }));

            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString["width"] = defaultWidth.ToString();
            queryString["height"] = defaultHeight.ToString();
            queryString["smartCropping"] = "true";
            var uri = $"{thumbnailServiceUri}?{queryString.ToString()}";
            using (var content = new ByteArrayContent(byteData))
            {
                var response = await thumbnailServiceApiClient.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    var thumbnailData = await response.Content.ReadAsByteArrayAsync();
                    var blob = new CloudBlockBlob(new Uri(thumbnailStoragePath), storageCreds);
                    await blob.UploadFromByteArrayAsync(thumbnailData, 0, thumbnailData.Length);
                }
                else
                {
                    // todo: Find a better way to manage response status codes
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            throw new Exception("Unauthorized. Invalid Security keys");
                        case HttpStatusCode.InternalServerError:
                            throw new Exception();
                        case HttpStatusCode.BadRequest:
                            var responseError = await response.Content.ReadAsStringAsync();
                            throw new InvalidOperationException(responseError);
                        default:
                            throw new InvalidOperationException();
                    }
                }
            }
        }

        #region azure media service version 3

        private async Task<List<Uri>> GenerateVideoThumbnailAsync(string fileUrl, string thumbnailStoragePath, string resourceGroup, string accountName)
        {
            pAzureMediaServicesClient.LongRunningOperationRetryTimeout = 2;
            var streamUris = new List<Uri>();

            try
            {
                // Ensure that you have customized encoding Transform.  This is really a one time setup operation.
                Transform adaptiveEncodeTransform = await EnsureTransformExistsAsync(pAzureMediaServicesClient, resourceGroup, accountName, fileUrl);

                // Creating a unique suffix so that we don't have name collisions if you run the sample
                // multiple times without cleaning up.
                var uniqueness = Guid.NewGuid().ToString().Substring(0, 13);
                var jobName = "job-" + uniqueness;
                var inputAssetName = "input-" + uniqueness;
                var outputAssetName = "output-" + uniqueness;

                var input = new JobInputHttp(files: new[] { fileUrl });
                Asset asetInput = new Asset();
                JobOutput[] jobOutputs = { new JobOutputAsset(outputAssetName)};

                await pAzureMediaServicesClient.Assets.CreateOrUpdateAsync(resourceGroup, accountName, outputAssetName, asetInput);
                Job job = await pAzureMediaServicesClient.Jobs.CreateAsync(resourceGroup, accountName, fileUrl, jobName, new Job { Input = input, Outputs = jobOutputs });
                job = await pAzureMediaServicesClient.Jobs.GetAsync(resourceGroup, accountName, fileUrl, jobName);

                var feedItemGuid = pBlobHelper.GetFileGuidFromFilePath(fileUrl);

                if (job.State == JobState.Finished)
                {
                    streamUris = await GetStreamingUrlsAsync(pAzureMediaServicesClient, resourceGroup, accountName, feedItemGuid.ToString());
                }
            }
            catch (ApiErrorException ex)
            {
                string code = ex.Body.Error.Code;
                string message = ex.Body.Error.Message;
            }

            return streamUris;
        }

        private async Task<List<Uri>> GetStreamingUrlsAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, String feedItemGuid)
        {
            const string DefaultStreamingEndpointName = "default";
            var streamingEndpoint = await client.StreamingEndpoints.GetAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);

            if (streamingEndpoint != null && 
				streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
			{ 
                await client.StreamingEndpoints.StartAsync(resourceGroupName, accountName, DefaultStreamingEndpointName);
            }

            var paths = await client.StreamingLocators.ListPathsAsync(resourceGroupName, accountName, feedItemGuid);

			var streamingUrls = new List<Uri>();

			paths.StreamingPaths.ToList().ForEach(path => 
            {
				var uriBuilder = new UriBuilder
				{
					Scheme = "https",
					Host = streamingEndpoint.HostName,
					Path = path.Paths[0]
				};
				streamingUrls.Add(uriBuilder.Uri);
            });

            return streamingUrls;
        }

        private async Task<Transform> EnsureTransformExistsAsync(IAzureMediaServicesClient client, string resourceGroupName, string accountName, string transformName)
        {
            // Does a Transform already exist with the desired name? Assume that an existing Transform with the desired name
            // also uses the same recipe or Preset for processing content.
            var transform = client.Transforms.Get(resourceGroupName, accountName, transformName);

            if (transform == null)
            {
                // Create a new Transform Outputs array - this defines the set of outputs for the Transform
                TransformOutput[] outputs = new TransformOutput[]
                {
                    // Create a new TransformOutput with a custom Standard Encoder Preset
                    // This demonstrates how to create custom codec and layer output settings

                    new TransformOutput(
                        new StandardEncoderPreset(
                            codecs: new Codec[]
                            {
                                // Add an AAC Audio layer for the audio encoding
                                new AacAudio(
                                    channels: 2,
                                    samplingRate: 48000,
                                    bitrate: 128000,
                                    profile: AacAudioProfile.AacLc
                                ),
                                // Next, add a H264Video for the video encoding
                               new H264Video (
                                    // Set the GOP interval to 2 seconds for both H264Layers
                                    keyFrameInterval:TimeSpan.FromSeconds(2),
                                     // Add H264Layers, one at HD and the other at SD. Assign a label that you can use for the output filename
                                    layers:  new H264Layer[]
                                    {
                                        new H264Layer (
                                            bitrate: 1000000, // Note that the units is in bits per second
                                            width: "1280",
                                            height: "720",
                                            label: "HD" // This label is used to modify the file name in the output formats
                                        ),
                                        new H264Layer (
                                            bitrate: 600000,
                                            width: "640",
                                            height: "480",
                                            label: "SD"
                                        )
                                    }
                                ),
                                // Also generate a set of PNG thumbnails
                                new PngImage(
                                    start: "25%",
                                    step: "25%",
                                    range: "80%",
                                    layers: new PngLayer[]{
                                        new PngLayer(
                                            width: "50%",
                                            height: "50%"
                                        )
                                    }
                                )
                            },
                            // Specify the format for the output files - one for video+audio, and another for the thumbnails
                            formats: new Format[]
                            {
                                // Mux the H.264 video and AAC audio into MP4 files, using basename, label, bitrate and extension macros
                                // Note that since you have multiple H264Layers defined above, you have to use a macro that produces unique names per H264Layer
                                // Either {Label} or {Bitrate} should suffice
                                 
                                new Mp4Format(
                                    filenamePattern: "Video-{Basename}-{Label}-{Bitrate}{Extension}"
                                ),
                                new PngFormat(
                                    filenamePattern: "Thumbnail-{Basename}-{Index}{Extension}"
                                )
                            }
                        ),
                        onError: OnErrorType.StopProcessingJob,
                        relativePriority: Priority.Normal
                    )
                };

                string description = "A simple custom encoding transform with 2 MP4 bitrates";
                // Create the custom Transform with the outputs defined above
                transform = await client.Transforms.CreateOrUpdateAsync(resourceGroupName, accountName, transformName, outputs, description);
            }

            return transform;
        }

        #endregion
    }
}

