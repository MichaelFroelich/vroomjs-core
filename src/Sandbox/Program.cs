﻿
using System;

namespace Sandbox
{
    public class Program
    {
        public static void Main(string[] args)
        {
            VroomJs.AssemblyLoader.EnsureLoaded();

            while (true)
            {
                GC.Collect(GC.MaxGeneration);
                using (var engine = new VroomJs.JsEngine())
                {
                    using (var context = engine.CreateContext())
                    {
                        context.SetVariable("name", "value");
                        var result = context.GetVariable("name");

                        engine.DumpHeapStats();
                    }
                    engine.DumpHeapStats();
                }
            }
        }
    }
}
