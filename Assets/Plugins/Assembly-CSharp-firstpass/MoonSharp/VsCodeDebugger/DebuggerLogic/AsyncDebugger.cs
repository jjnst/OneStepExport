using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace MoonSharp.VsCodeDebugger.DebuggerLogic
{
	internal class AsyncDebugger : IDebugger
	{
		private static object s_AsyncDebuggerIdLock = new object();

		private static int s_AsyncDebuggerIdCounter = 0;

		private object m_Lock = new object();

		private IAsyncDebuggerClient m_Client__;

		private DebuggerAction m_PendingAction = null;

		private List<WatchItem>[] m_WatchItems;

		private Dictionary<int, SourceCode> m_SourcesMap = new Dictionary<int, SourceCode>();

		private Dictionary<int, string> m_SourcesOverride = new Dictionary<int, string>();

		private Func<SourceCode, string> m_SourceFinder;

		public DebugService DebugService { get; private set; }

		public Regex ErrorRegex { get; set; }

		public Script Script { get; private set; }

		public bool PauseRequested { get; set; }

		public string Name { get; set; }

		public int Id { get; private set; }

		public IAsyncDebuggerClient Client
		{
			get
			{
				return m_Client__;
			}
			set
			{
				lock (m_Lock)
				{
					if (m_Client__ != null && m_Client__ != value)
					{
						m_Client__.Unbind();
					}
					if (value != null)
					{
						for (int i = 0; i < Script.SourceCodeCount; i++)
						{
							if (m_SourcesMap.ContainsKey(i))
							{
								value.OnSourceCodeChanged(i);
							}
						}
					}
					m_Client__ = value;
				}
			}
		}

		public AsyncDebugger(Script script, Func<SourceCode, string> sourceFinder, string name)
		{
			lock (s_AsyncDebuggerIdLock)
			{
				Id = s_AsyncDebuggerIdCounter++;
			}
			m_SourceFinder = sourceFinder;
			ErrorRegex = new Regex("\\A.*\\Z");
			Script = script;
			m_WatchItems = new List<WatchItem>[6];
			Name = name;
			for (int i = 0; i < m_WatchItems.Length; i++)
			{
				m_WatchItems[i] = new List<WatchItem>(64);
			}
		}

		DebuggerAction IDebugger.GetAction(int ip, SourceRef sourceref)
		{
			PauseRequested = false;
			lock (m_Lock)
			{
				if (Client != null)
				{
					Client.SendStopEvent();
				}
			}
			while (true)
			{
				lock (m_Lock)
				{
					if (Client == null)
					{
						return new DebuggerAction
						{
							Action = DebuggerAction.ActionType.Run
						};
					}
					if (m_PendingAction != null)
					{
						DebuggerAction pendingAction = m_PendingAction;
						m_PendingAction = null;
						return pendingAction;
					}
				}
				Sleep(10);
			}
		}

		public void QueueAction(DebuggerAction action)
		{
			while (true)
			{
				lock (m_Lock)
				{
					if (m_PendingAction == null)
					{
						m_PendingAction = action;
						break;
					}
				}
				Sleep(10);
			}
		}

		private void Sleep(int v)
		{
			Thread.Sleep(10);
		}

		private DynamicExpression CreateDynExpr(string code)
		{
			try
			{
				return Script.CreateDynamicExpression(code);
			}
			catch (Exception ex)
			{
				return Script.CreateConstantDynamicExpression(code, DynValue.NewString(ex.Message));
			}
		}

		List<DynamicExpression> IDebugger.GetWatchItems()
		{
			return new List<DynamicExpression>();
		}

		bool IDebugger.IsPauseRequested()
		{
			return PauseRequested;
		}

		void IDebugger.RefreshBreakpoints(IEnumerable<SourceRef> refs)
		{
		}

		void IDebugger.SetByteCode(string[] byteCode)
		{
		}

		void IDebugger.SetSourceCode(SourceCode sourceCode)
		{
			m_SourcesMap[sourceCode.SourceID] = sourceCode;
			bool flag = false;
			string text = m_SourceFinder(sourceCode);
			if (!string.IsNullOrEmpty(text))
			{
				try
				{
					if (!File.Exists(text))
					{
						flag = true;
					}
				}
				catch
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				text = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".lua");
				File.WriteAllText(text, sourceCode.Code + GetFooterForTempFile());
				m_SourcesOverride[sourceCode.SourceID] = text;
			}
			else if (text != sourceCode.Name)
			{
				m_SourcesOverride[sourceCode.SourceID] = text;
			}
			lock (m_Lock)
			{
				if (Client != null)
				{
					Client.OnSourceCodeChanged(sourceCode.SourceID);
				}
			}
		}

		private string GetFooterForTempFile()
		{
			return "\n\n----------------------------------------------------------------------------------------------------------\n-- This file has been generated by the debugger as a placeholder for a script snippet stored in memory. --\n-- If you restart the host process, the contents of this file are not valid anymore.                    --\n----------------------------------------------------------------------------------------------------------\n";
		}

		public string GetSourceFile(int sourceId)
		{
			if (m_SourcesOverride.ContainsKey(sourceId))
			{
				return m_SourcesOverride[sourceId];
			}
			if (m_SourcesMap.ContainsKey(sourceId))
			{
				return m_SourcesMap[sourceId].Name;
			}
			return null;
		}

		public bool IsSourceOverride(int sourceId)
		{
			return m_SourcesOverride.ContainsKey(sourceId);
		}

		void IDebugger.SignalExecutionEnded()
		{
			lock (m_Lock)
			{
				if (Client != null)
				{
					Client.OnExecutionEnded();
				}
			}
		}

		bool IDebugger.SignalRuntimeException(ScriptRuntimeException ex)
		{
			lock (m_Lock)
			{
				if (Client == null)
				{
					return false;
				}
			}
			Client.OnException(ex);
			PauseRequested = ErrorRegex.IsMatch(ex.Message);
			return PauseRequested;
		}

		void IDebugger.Update(WatchType watchType, IEnumerable<WatchItem> items)
		{
			List<WatchItem> list = m_WatchItems[(int)watchType];
			list.Clear();
			list.AddRange(items);
			lock (m_Lock)
			{
				if (Client != null)
				{
					Client.OnWatchesUpdated(watchType);
				}
			}
		}

		public List<WatchItem> GetWatches(WatchType watchType)
		{
			return m_WatchItems[(int)watchType];
		}

		public SourceCode GetSource(int id)
		{
			if (m_SourcesMap.ContainsKey(id))
			{
				return m_SourcesMap[id];
			}
			return null;
		}

		public SourceCode FindSourceByName(string path)
		{
			path = path.Replace('\\', '/').ToUpperInvariant();
			foreach (KeyValuePair<int, string> item in m_SourcesOverride)
			{
				if (item.Value.Replace('\\', '/').ToUpperInvariant() == path)
				{
					return m_SourcesMap[item.Key];
				}
			}
			return m_SourcesMap.Values.FirstOrDefault((SourceCode s) => s.Name.Replace('\\', '/').ToUpperInvariant() == path);
		}

		void IDebugger.SetDebugService(DebugService debugService)
		{
			DebugService = debugService;
		}

		public DynValue Evaluate(string expression)
		{
			DynamicExpression dynamicExpression = CreateDynExpr(expression);
			return dynamicExpression.Evaluate();
		}

		DebuggerCaps IDebugger.GetDebuggerCaps()
		{
			return DebuggerCaps.CanDebugSourceCode | DebuggerCaps.HasLineBasedBreakpoints;
		}
	}
}
