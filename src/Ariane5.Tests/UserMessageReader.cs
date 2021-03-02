using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ariane.Tests
{
    public class UserMessageReader : Ariane.MessageReaderBase<User>
    {
		public UserMessageReader()
		{
		}

		public override Task ProcessMessageAsync(User message)
		{
			return Task.CompletedTask;
		}

	}
}
