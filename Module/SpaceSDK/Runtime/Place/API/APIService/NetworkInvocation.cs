using Castle.DynamicProxy;
using System;
using System.Reflection;

namespace MaxstXR.Place
{
	class NetworkInvocation : IInvocation
	{
		private readonly MethodInfo methodInfo;
		private readonly object[] arguments;
		public NetworkInvocation(MethodInfo methodInfo, object[] arguments)
		{
			this.methodInfo = methodInfo;
			this.arguments = arguments;

		}
		public object[] Arguments => arguments;

		public Type[] GenericArguments => throw new NotImplementedException();

		public object InvocationTarget => throw new NotImplementedException();

		public MethodInfo Method => methodInfo;

		public MethodInfo MethodInvocationTarget => throw new NotImplementedException();

		public object Proxy => throw new NotImplementedException();

		private object returnValue;
		public object ReturnValue { get => returnValue; set => returnValue = value; }

		public Type TargetType => throw new NotImplementedException();

		public object GetArgumentValue(int index)
		{
			throw new NotImplementedException();
		}

		public MethodInfo GetConcreteMethod()
		{
			throw new NotImplementedException();
		}

		public MethodInfo GetConcreteMethodInvocationTarget()
		{
			throw new NotImplementedException();
		}

		public void Proceed()
		{
			throw new NotImplementedException();
		}

		public void SetArgumentValue(int index, object value)
		{
			throw new NotImplementedException();
		}

		MethodInfo IInvocation.GetConcreteMethod()
		{
			throw new NotImplementedException();
		}

		MethodInfo IInvocation.GetConcreteMethodInvocationTarget()
		{
			throw new NotImplementedException();
		}
	}
}