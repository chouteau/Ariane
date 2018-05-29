using Microsoft.ServiceBus.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Azure
{
	public static class AzureMessageExtensions
	{
		public static BrokeredMessage CreateSerializedBrokeredMessage(this object body)
		{
			var content = Newtonsoft.Json.JsonConvert.SerializeObject(body);
			var bytes = Encoding.UTF8.GetBytes(content);
			var stream = new System.IO.MemoryStream(bytes, false);
			var brokeredMessage = new BrokeredMessage(stream);
			brokeredMessage.ContentType = "application/json";
			return brokeredMessage;
		}

		public static T GetAndDeserializeBody<T>(this BrokeredMessage brokeredMessage)
		{
			var body = default(T);
			var stream = brokeredMessage.GetBody<System.IO.Stream>();
			using (var reader = new System.IO.StreamReader(stream, Encoding.UTF8))
			{
				var content = reader.ReadToEnd();
				try
				{
					body = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(content);
				}
				catch (Exception ex)
				{
					ex.Data.Add("content", content);
					throw ex;
				}
			}
			return body;
		}
	}
}
