﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace VroomJs {
	public class JsEngine : IDisposable {

		delegate void KeepaliveRemoveDelegate(int context, int slot);
		delegate JsValue KeepAliveGetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
		delegate JsValue KeepAliveSetPropertyValueDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);
		delegate JsValue KeepAliveValueOfDelegate(int context, int slot);
		delegate JsValue KeepAliveInvokeDelegate(int context, int slot, JsValue args);
		delegate JsValue KeepAliveDeletePropertyDelegate(int context, int slot, [MarshalAs(UnmanagedType.LPWStr)] string name);
		delegate JsValue KeepAliveEnumeratePropertiesDelegate(int context, int slot);

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern void js_set_object_marshal_type(JsObjectMarshalType objectMarshalType);

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern void js_dump_allocated_items();

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern IntPtr jsengine_new(
			KeepaliveRemoveDelegate keepaliveRemove,
			KeepAliveGetPropertyValueDelegate keepaliveGetPropertyValue,
			KeepAliveSetPropertyValueDelegate keepaliveSetPropertyValue,
			KeepAliveValueOfDelegate keepaliveValueOf,
			KeepAliveInvokeDelegate keepaliveInvoke,
			KeepAliveDeletePropertyDelegate keepaliveDeleteProperty,
			KeepAliveEnumeratePropertiesDelegate keepaliveEnumerateProperties,
			int maxYoungSpace, int maxOldSpace
		);
		
		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_terminate_execution(IntPtr engine);
			
		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_dump_heap_stats(IntPtr engine);

		[DllImport(JsContext.NativeFileName, CallingConvention = CallingConvention.StdCall)]
		static extern void jsengine_dispose(IntPtr engine);

		[DllImport(JsContext.NativeFileName)]
		static extern void jsengine_dispose_object(IntPtr engine, IntPtr obj);
		
		// Make sure the delegates we pass to the C++ engine won't fly away during a GC.
		readonly KeepaliveRemoveDelegate _keepalive_remove;
		readonly KeepAliveGetPropertyValueDelegate _keepalive_get_property_value;
		readonly KeepAliveSetPropertyValueDelegate _keepalive_set_property_value;
		readonly KeepAliveValueOfDelegate _keepalive_valueof;
		readonly KeepAliveInvokeDelegate _keepalive_invoke;
		readonly KeepAliveDeletePropertyDelegate _keepalive_delete_property;
		readonly KeepAliveEnumeratePropertiesDelegate _keepalive_enumerate_properties;

		private readonly Dictionary<int, JsContext> _aliveContexts = new Dictionary<int, JsContext>();
		private readonly Dictionary<int, JsScript> _aliveScripts = new Dictionary<int, JsScript>();

		private int _currentContextId = 0;
		private int _currentScriptId = 0;

		public static void DumpAllocatedItems() {
			js_dump_allocated_items();
		}

		static JsEngine() {
			js_set_object_marshal_type(JsObjectMarshalType.Dynamic);
		}

		readonly IntPtr _engine;

		public JsEngine(int maxYoungSpace = -1, int maxOldSpace = -1) {
			_keepalive_remove = KeepAliveRemove;
			_keepalive_get_property_value = KeepAliveGetPropertyValue;
			_keepalive_set_property_value = KeepAliveSetPropertyValue;
			_keepalive_valueof = KeepAliveValueOf;
			_keepalive_invoke = KeepAliveInvoke;
			_keepalive_delete_property = KeepAliveDeleteProperty;
			_keepalive_enumerate_properties = KeepAliveEnumerateProperties;
			
			_engine = jsengine_new(
				_keepalive_remove,
				_keepalive_get_property_value,
				_keepalive_set_property_value, 
				_keepalive_valueof,
				_keepalive_invoke,
				_keepalive_delete_property,
				_keepalive_enumerate_properties,
				maxYoungSpace, 
				maxOldSpace);
		}

		public void TerminateExecution() {
			jsengine_terminate_execution(_engine);
		}

		public void DumpHeapStats() {
			jsengine_dump_heap_stats(_engine);
		}

		public void DisposeObject(IntPtr ptr) {
			// If the engine has already been explicitly disposed we pass Zero as
			// the first argument because we need to free the memory allocated by
			// "new" but not the object on the V8 heap: it has already been freed.
			if (_disposed)
				jsengine_dispose_object(IntPtr.Zero, ptr);
			else
				jsengine_dispose_object(_engine, ptr);
		}

		private JsValue KeepAliveValueOf(int contextId, int slot) {
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveValueOf(slot);
		}
		
		private JsValue KeepAliveInvoke(int contextId, int slot, JsValue args) {
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveInvoke(slot, args);
		}

		private JsValue KeepAliveSetPropertyValue(int contextId, int slot, string name, JsValue value) {
#if DEBUG_TRACE_API
			Console.WriteLine("set prop " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
			return context.KeepAliveSetPropertyValue(slot, name, value);
		}

		
		private JsValue KeepAliveGetPropertyValue(int contextId, int slot, string name) {
#if DEBUG_TRACE_API
			Console.WriteLine("get prop " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}
	
			JsValue value = context.KeepAliveGetPropertyValue(slot, name);
			return value;
		}

		private JsValue KeepAliveDeleteProperty(int contextId, int slot, string name) {
#if DEBUG_TRACE_API
			Console.WriteLine("delete prop " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}

			JsValue value = context.KeepAliveDeleteProperty(slot, name);
			return value;
		}

		private JsValue KeepAliveEnumerateProperties(int contextId, int slot) {
#if DEBUG_TRACE_API
			Console.WriteLine("enumerate props " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				throw new Exception("fail");
			}

			JsValue value = context.KeepAliveEnumerateProperties(slot);
			return value;
		}

		private void KeepAliveRemove(int contextId, int slot) {
#if DEBUG_TRACE_API
			Console.WriteLine("Keep alive remove for " + contextId + " " + slot);
#endif
			JsContext context;
			if (!_aliveContexts.TryGetValue(contextId, out context)) {
				return;
			}
			context.KeepAliveRemove(slot);
		}

		public JsContext CreateContext() {
			CheckDisposed();
			int id = Interlocked.Increment(ref _currentContextId);
			JsContext ctx = new JsContext(id, this, _engine, ContextDisposed);
			_aliveContexts.Add(id, ctx);
			return ctx;
		}

		public JsScript CompileScript(string code, string name = "<Unamed Script>") {
			CheckDisposed();
			int id = Interlocked.Increment(ref _currentScriptId);
			JsScript script = new JsScript(id, this, _engine, new JsConvert(null), code, name, ScriptDisposed);
			_aliveScripts.Add(id, script);
			return script;
		}

		private void ContextDisposed(int id) {
			_aliveContexts.Remove(id);
		}

		private void ScriptDisposed(int id) {
			_aliveScripts.Remove(id);
		}

		#region IDisposable implementation

        bool _disposed;

        public bool IsDisposed {
            get { return _disposed; }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            CheckDisposed();

            _disposed = true;

            if (disposing) {
            	foreach (var aliveContext in _aliveContexts) {
					JsContext.jscontext_dispose(aliveContext.Value.Handle);
            	}
				_aliveContexts.Clear();
				foreach (var aliveScript in _aliveScripts) {
					JsScript.jsscript_dispose(aliveScript.Value.Handle);
				}
            }
#if DEBUG_TRACE_API
				Console.WriteLine("Calling jsEngine dispose: " + _engine.Handle.ToInt64());
#endif
        
			jsengine_dispose(_engine);
        }

        void CheckDisposed()
        {
            if (_disposed)
               throw new ObjectDisposedException("JsEngine:" + _engine);
        }

        ~JsEngine()
        {
            if (!_disposed)
                Dispose(false);
        }

        #endregion
	}
}
