using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	public class MSMQMessageWrapper : IMessage
	{
		private System.Messaging.Message m_Message;

		public MSMQMessageWrapper(System.Messaging.Message message)
		{
			m_Message = message;
		}

		public System.Messaging.Message GetMessage()
		{
			return m_Message;
		}

		#region IMessage Members

		public string QueueName { get; set; }

		public string Label
		{
			get
			{
				return m_Message.Label;
			}
			set
			{
				m_Message.Label = value;
			}
		}

		public object Body
		{
			get
			{
				return m_Message.Body;
			}
			set
			{
				m_Message.Body = value;
			}
		}

		public bool Recoverable
		{
			get
			{
				return m_Message.Recoverable;
			}
			set
			{
				m_Message.Recoverable = value;
			}
		}

		#endregion

		public void Dispose()
		{
			m_Message.Dispose();
		}
	}
}
