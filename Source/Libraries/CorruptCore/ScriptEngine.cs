namespace RTCV.CorruptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using ScriptCore;

    static class ScriptEngine
    {
        public static ScriptRunner Runner { get; private set; }

        static ScriptEngine()
        {
            Initialize();
        }

        static void Initialize()
        {
            ScriptInitializer.Start(typeof(ScriptEngine).Assembly);
            Runner = new ScriptRunner();
        }

        public static void Execute(string hook, params object[] args)
        {
            Runner?.Execute(hook, args);
        }

        public static void LoadScript(string script)
        {
            Runner.LoadScript(script);
        }

        public static void Reset()
        {
            Runner.Abort();
            Runner = new ScriptRunner();
        }
    }
}
