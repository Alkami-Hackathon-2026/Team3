using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Alkami.Services.Subscriptions.ParticipatingClient
{
	/// <summary>
	/// The <see cref="SecurityBehavior"/> adds a <see cref="SecurityInjectorInspector"/> into the WCF workflow.
	/// </summary>
	/// <seealso cref="System.ServiceModel.Description.IOperationBehavior" />
	public class SecurityBehavior : IOperationBehavior
	{
		/// <summary>
		/// Implement to pass data at runtime to bindings to support custom behavior.
		/// </summary>
		/// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
		/// <param name="bindingParameters">The collection of objects that binding elements require to support the behavior.</param>
		public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
		{
		}

		/// <summary>
		/// Implements a modification or extension of the client across an operation.
		/// </summary>
		/// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
		/// <param name="clientOperation">The run-time object that exposes customization properties for the operation described by <paramref name="operationDescription" />.</param>
		public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
		{
			clientOperation.ClientParameterInspectors.Add(new SecurityInjectorInspector());
		}

		/// <summary>
		/// Implements a modification or extension of the service across an operation.
		/// </summary>
		/// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
		/// <param name="dispatchOperation">The run-time object that exposes customization properties for the operation described by <paramref name="operationDescription" />.</param>
		/// <exception cref="NotImplementedException"></exception>
		public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Implement to confirm that the operation meets some intended criteria.
		/// </summary>
		/// <param name="operationDescription">The operation being examined. Use for examination only. If the operation description is modified, the results are undefined.</param>
		public void Validate(OperationDescription operationDescription)
		{
		}
	}
}