﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Decorator for synchronized bus for tests
	/// </summary>
	public class SyncBusManager : Ariane.IServiceBus
	{
		private Ariane.IServiceBus m_Decorated;

		public SyncBusManager(Ariane.IServiceBus decorated)
		{
			m_Decorated = decorated;
		}

		public void PauseReading()
		{
			m_Decorated.PauseReading();
		}

		public Ariane.IFluentRegister Register
		{
			get { return m_Decorated.Register; }
		}

		public void Send<T>(string queueName, T body, string label = null)
		{
			m_Decorated.SyncProcess(queueName, body, label);
		}

		public void StartReading()
		{
			m_Decorated.StartReading();
		}

		public void StopReading()
		{
			m_Decorated.StopReading();
		}

		public void SyncProcess<T>(string queueName, T body, string label = null)
		{
			m_Decorated.SyncProcess(queueName, body, label);
		}

		public dynamic CreateMessage(string name)
		{
			return m_Decorated.CreateMessage(name);
		}
	}
}