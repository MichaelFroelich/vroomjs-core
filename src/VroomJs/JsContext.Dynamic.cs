﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace VroomJs
{
	public partial class JsContext {

		[DllImport(NativeFileName)]
		static extern JsValue jscontext_get_property_names(IntPtr engine, IntPtr ptr);

		[DllImport(NativeFileName)]
		static extern JsValue jscontext_get_property_value(IntPtr engine, IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name);

		[DllImport(NativeFileName)]
		static extern JsValue jscontext_set_property_value(IntPtr engine, IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue value);

		[DllImport(NativeFileName)]
		static extern JsValue jscontext_invoke_property(IntPtr engine, IntPtr ptr, [MarshalAs(UnmanagedType.LPWStr)] string name, JsValue args);
	
		public IEnumerable<string> GetMemberNames(JsObject obj) 
		{
			if (obj == null)
				throw new ArgumentNullException("obj");

			CheckDisposed();

			if (obj.Handle == IntPtr.Zero)
				throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

			JsValue v = jscontext_get_property_names(_context, obj.Handle);
			object res = _convert.FromJsValue(v);
			jsvalue_dispose(v);

			Exception e = res as JsException;
			if (e != null)
				throw e;

			object[] arr = (object[])res;
			return arr.Cast<string>();
		}


        public object GetPropertyValue(JsObject obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

			JsValue v = jscontext_get_property_value(_context, obj.Handle, name);
            object res = _convert.FromJsValue(v);
            jsvalue_dispose(v);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }

        public void SetPropertyValue(JsObject obj, string name, object value)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue a = _convert.ToJsValue(value);
			JsValue v = jscontext_set_property_value(_context, obj.Handle, name, a);
            object res = _convert.FromJsValue(v);
            jsvalue_dispose(v);
            jsvalue_dispose(a);

            Exception e = res as JsException;
            if (e != null)
                throw e;
        }

        public object InvokeProperty(JsObject obj, string name, object[] args)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");
            if (name == null)
                throw new ArgumentNullException("name");

            CheckDisposed();

            if (obj.Handle == IntPtr.Zero)
                throw new JsInteropException("wrapped V8 object is empty (IntPtr is Zero)");

            JsValue a = JsValue.Null; // Null value unless we're given args.
            if (args != null)
                a = _convert.ToJsValue(args);

			JsValue v = jscontext_invoke_property(_context, obj.Handle, name, a);
            object res = _convert.FromJsValue(v);
            jsvalue_dispose(v);
            jsvalue_dispose(a);

            Exception e = res as JsException;
            if (e != null)
                throw e;
            return res;
        }
	}
}
