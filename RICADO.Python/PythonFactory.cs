using System;
using System.Collections.Generic;
using Microsoft.Scripting.Hosting;
using IronPython.Hosting;

namespace RICADO.Python
{
    public class PythonFactory
    {
        #region Private Properties

        private ScriptEngine _scriptEngine;
        private object _scriptEngineLock = new object();

        private ScriptScope _scriptScope;
        private object _scriptScopeLock = new object();

        #endregion


        #region Private Properties

        private ScriptEngine ScriptEngine
        {
            get
            {
                lock (_scriptEngineLock)
                {
                    return _scriptEngine;
                }
            }
        }

        private ScriptScope ScriptScope
        {
            get
            {
                lock (_scriptScopeLock)
                {
                    return _scriptScope;
                }
            }
        }

        #endregion


        #region Constructor

        public PythonFactory()
        {
        }

        #endregion


        #region Public Methods

        public bool Initialize()
        {
            lock (_scriptEngineLock)
            {
                _scriptEngine = IronPython.Hosting.Python.CreateEngine();

                if (_scriptEngine == null)
                {
                    return false;
                }
            }

            lock (_scriptScopeLock)
            {
                _scriptScope = ScriptEngine.CreateScope();

                if (_scriptScope == null)
                {
                    return false;
                }
            }

            ScriptScope.ImportModule("clr");

            ScriptEngine.Execute("import clr", ScriptScope);

            ScriptEngine.Execute("clr.AddReference(\"System\")", ScriptScope);

            ScriptEngine.Execute("from System import Random, DateTime, Guid", ScriptScope);

            return true;
        }

        public void Destroy()
        {
            lock (_scriptScopeLock)
            {
                if (_scriptScope != null)
                {
                    _scriptScope = null;
                }
            }

            lock (_scriptEngineLock)
            {
                if (_scriptEngine != null)
                {
                    _scriptEngine.Runtime.Shutdown();

                    _scriptEngine = null;
                }
            }
        }

        public CompiledCode CompileScript(string script)
        {
            lock (_scriptEngineLock)
            {
                ScriptSource scriptSource = _scriptEngine.CreateScriptSourceFromString(script, Microsoft.Scripting.SourceCodeKind.AutoDetect);

                return scriptSource.Compile();
            }
        }

        public object ExecuteCode(CompiledCode code)
        {
            lock (_scriptScopeLock)
            {
                return code.Execute(_scriptScope);
            }
        }

        public T ExecuteCode<T>(CompiledCode code)
        {
            lock(_scriptScopeLock)
            {
                return code.Execute<T>(_scriptScope);
            }
        }

        public void PopulateScriptScopeVariables(IEnumerable<KeyValuePair<string, object>> variables)
        {
            lock (_scriptScopeLock)
            {
                foreach (KeyValuePair<string, object> variable in variables)
                {
                    _scriptScope.SetVariable(variable.Key, variable.Value);
                }
            }
        }

        public void UpdateScriptScopeVariable(string name, object value)
        {
            lock(_scriptScopeLock)
            {
                _scriptScope.SetVariable(name, value);
            }
        }

        #endregion


        #region Private Methods

        #endregion
    }
}
