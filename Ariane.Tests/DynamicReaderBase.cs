using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane.Tests
{
	public abstract class DynamicReaderBase : Ariane.MessageReaderBase<System.Dynamic.ExpandoObject>
	{
		public override void ProcessMessage(System.Dynamic.ExpandoObject message)
		{
			ProcessMessage(message as dynamic);
		}

		public abstract void ProcessMessage(dynamic message);
	}
}
