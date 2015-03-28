using System;

namespace NAppUpdate.Framework.Common
{
	// TODO: This isn't air tight, it is just "good enough"
	public class WorkScope : IDisposable
	{
		private readonly Action<bool> _isWorkingFunc;

		public WorkScope(Action<bool> b)
		{
			_isWorkingFunc = b;
			_isWorkingFunc(true);
		}

		internal static IDisposable New(Action<bool> action)
		{
			return new WorkScope(action);
		}

		public void Dispose()
		{
			_isWorkingFunc(false);
		}
	}
}
