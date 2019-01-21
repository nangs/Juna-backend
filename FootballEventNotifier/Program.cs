using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace FCMNotificer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
                var footballEvent = new
                {
                    id = Guid.NewGuid(),
                    eventType = "yellow-card",
                    playerName = "Paul Pogba",
                    teamName = "Manchester United",
                    matchId = 33
                };

                var postedFeedItem = new
                {
                    feedItemType = "Image",
                   // thumbnail = < binarydata >,
                  
                 
                };
                // Parse football events
                // Load data
                // Send notification
                SendFcmNotification(footballEvent);
              //  SendFcmNotification(postedFeedItem);
            }
			catch (Exception ex)
			{
				Console.Write(ex.StackTrace);
			}
		}

		private static void SendFcmNotification(dynamic footballEvent)
		{
			var applicationID = "AAAA9wiibRE:APA91bHVdYaUCK01Oz1ske68KY_YWM4XvCQG8oTuwbond2Mb3Ctz38u2XBOgIjBmKS2-sOTJX35e_RjGu2xjQdNxbqyW7yv_eN5x700yOafIL4MEn0bsTFw7csN1DNVr8K8X5sjOn21D";
			var senderId = "1061001784593";
			var deviceId = "ee4ehKJW10Y:APA91bEu8YLtWgX8v2xaJOSlbAnRDLoObO7KOsPkLb95gaf6UblanI7fyNr-euxMLB76j0f_t7lmXmvFsTIb648WYnjYSVZMWJrRBmouQRaejp2n2rTrXtnkfQayabmQhTLe6S6TrVar";
			WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
			tRequest.Method = "post";
			tRequest.ContentType = "application/json";
			var data = new
			{
				//to = deviceId,
                to = "/topics/ManCityVsManU",
				notification = new
				{
					title = "title",
					body = footballEvent
				}
			};
			var json = JsonConvert.SerializeObject(data);
			Byte[] byteArray = Encoding.UTF8.GetBytes(json);
			tRequest.Headers.Add($"Authorization: key={applicationID}");
			tRequest.Headers.Add($"Sender: id={senderId}");
			tRequest.ContentLength = byteArray.Length;

			using (Stream dataStream = tRequest.GetRequestStream())
			{
				dataStream.Write(byteArray, 0, byteArray.Length);
				using (WebResponse tResponse = tRequest.GetResponse())
				{
					using (Stream dataStreamResponse = tResponse.GetResponseStream())
					{
						using (StreamReader tReader = new StreamReader(dataStreamResponse))
						{
							var responseBody = tReader.ReadToEnd();
							Console.WriteLine($"Received Response: [{responseBody}]");
							Console.ReadLine();
						}
					}
				}
			}
		}
	}
}
