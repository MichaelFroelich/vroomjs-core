﻿using System;
using System.Runtime.InteropServices;

namespace VroomJs {
	public class JsScript : IDisposable {
		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern IntPtr jsscript_new(IntPtr engine);

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode)]
		static extern JsValue jsscript_compile(IntPtr script, [MarshalAs(UnmanagedType.LPWStr)] string str,
													  [MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		public static extern IntPtr jsscript_dispose(IntPtr script);

		private readonly int _id;
		private readonly JsEngine _engine;

		public JsEngine Engine => _engine;

	  private readonly IntPtr _script;

		internal IntPtr Handle => _script;

	  internal JsScript(int id, JsEngine engine, IntPtr engineHandle, JsConvert convert, string code, string name, Action<int> notifyDispose) {
			_id = id;
			_engine = engine;
			_notifyDispose = notifyDispose;

			_script = jsscript_new(engineHandle);
			
			JsValue v = jsscript_compile(_script, code, name);
			object res = convert.FromJsValue(v);
			Exception e = res as JsException;
			if (e != null) {
				throw e;
			}
		}

		#region IDisposable implementation

		private readonly Action<int> _notifyDispose;
		bool _disposed;

		public bool IsDisposed => _disposed;

	  public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			CheckDisposed();

			_disposed = true;

			jsscript_dispose(_script);

			_notifyDispose(_id);
		}

		void CheckDisposed() {
			if (_engine.IsDisposed) {
				throw new ObjectDisposedException("JsScript: engine has been disposed");
			}
			if (_disposed)
				throw new ObjectDisposedException("JsScript:" + _script);
		}

		~JsScript() {
			if (!_engine.IsDisposed && !_disposed)
				Dispose(false);
		}

		#endregion

	}
}
