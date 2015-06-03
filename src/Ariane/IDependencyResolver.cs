using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	/// <summary>
	/// Dependency resolver
	/// </summary>
	public interface IDependencyResolver
	{
		/// <summary>
		/// Get service for type
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		object GetService(Type serviceType);
		/// <summary>
		/// Get services for type
		/// </summary>
		/// <param name="serviceType"></param>
		/// <returns></returns>
		IEnumerable<object> GetServices(Type serviceType);
		/// <summary>
		/// Get all services
		/// </summary>
		/// <returns></returns>
		IEnumerable<object> GetAllServices();
	}
}
