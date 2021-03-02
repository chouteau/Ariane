using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane
{
	/// <summary>
	/// Dynamic message reader base
	/// </summary>
	public abstract class DynamicMessageReaderBase : MessageReaderBase<System.Dynamic.ExpandoObject>
	{
		/// <summary>
		/// Process dynamic message
		/// </summary>
		/// <param name="message"></param>
		public override async Task ProcessMessageAsync(System.Dynamic.ExpandoObject message)
		{
			if (!message.IsDynamicPropertyExists("MessageName"))
			{
				return;
			}

			var d = message as dynamic;
			await ProcessAsync(d.MessageName as string, d);
		}

		/// <summary>
		/// Process dynamic named message
		/// </summary>
		/// <param name="messageName"></param>
		/// <param name="message"></param>
		protected abstract Task ProcessAsync(string messageName, dynamic message);
	}
}
