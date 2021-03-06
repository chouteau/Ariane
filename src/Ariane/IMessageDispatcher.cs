﻿using System;
using System.Collections.Generic;
namespace Ariane
{
	internal interface IMessageDispatcher : IDisposable
	{
		void AddMessageSubscriberTypeList(IList<Type> list);
		void AddMessageSubscriberList(IList<object> subscriber);
		void Start(IMessageQueue queue);
		void Stop();
	}
}
