
using System;
using System.Collections.Generic;

namespace BasicCommon
{
	public enum LogLevel
	{
		Debug = 0,
		Info = 1,
		Warn = 2,
		Error = 3
	};

	/// <summary>
	/// A log4j/log4net inspired API for logging.
	/// </summary>
	/// 
	public interface ILog
	{
		string Name { get; }
		void Debug(object message);
		void Debug(object message, Exception exception);
		void Info(object message);
		void Info(object message, Exception exception);
		void Warn(object message);
		void Warn(object message, Exception exception);
		void Error(object message);
		void Error(object message, Exception exception);
		void Log(LogLevel level, object message);
		void Log(LogLevel level, object message, Exception exception);
		bool IsDebugEnabled { get; }
		bool IsInfoEnabled { get; }
		bool IsWarnEnabled { get; }
		bool IsErrorEnabled { get; }
	}

	public class Logger
	{
		public static bool IsLogging = true;
	
		private static Dictionary<string,ILog> loggers = null; // Lazy init
		private static LogImpl rootLogger = null; // Lazy init
		

		public static void Init()
		{
			if( rootLogger == null )
				rootLogger = new LogImpl("", LogLevel.Debug);
			
			if( loggers == null )
			{
				loggers = new Dictionary<string, ILog>();
			}
		}
		
		public static ILog Exists(string name)
		{
			if( rootLogger == null || loggers == null )
				Init();
			
			// No parent for root logger
			if( string.IsNullOrEmpty(name) )
				return rootLogger;
			
			ILog log = null;
			if( loggers.TryGetValue(name, out log) )
				return log;
			return null;
		}
		
		public static ILog GetLogger(string name)
		{
			if( rootLogger == null || loggers == null )
				Init();

			// No parent for root logger
			if( string.IsNullOrEmpty(name) )
				return rootLogger;
			
			ILog log = null;
			if( loggers.TryGetValue(name, out log) )
				return log;
			
			log = new LogImpl(name, null);
			loggers[name] = log;
			
			LogImpl parentLog = null;
			
			int i = name.LastIndexOf('.', name.Length-1);
			if( i >= 0 )
			{
				string parentName = name.Substring(0, i);
				parentLog = (LogImpl)GetLogger(parentName);
			}
			else
			{
				parentLog = rootLogger;
			}
			
			((LogImpl)log).parent = parentLog;
			
			return log;
		}
		
		/// <summary>
		/// Main log implementation that allows its level to be set and redirects back to a common log output mechanism.
		/// </summary>
		class LogImpl : ILog
		{
			string name;
			
			public string Name { get { return name; } }
			
			public LogImpl parent = null;
						
			public LogLevel? level = null;
			
			public LogLevel EffectiveLevel
			{
				get
				{
					for( LogImpl l = this; l != null; l = l.parent )
					{
						LogLevel? level = l.level;
						if( level.HasValue )
							return level.Value;
					}
					// Would only happen if someone removed the root logger's level
					return LogLevel.Debug;
				}
			}
			
			public LogImpl(string name, LogLevel? level)
			{
				this.name = string.Intern(name);
				this.level = level;
			}
			
			public void Debug(object message)
			{
				Log(LogLevel.Debug, message, null);
			}
			
			public void Debug(object message, Exception exception)
			{
				Log(LogLevel.Debug, message, exception);
			}
			
			public void Info(object message)
			{
				Log(LogLevel.Info, message, null);	
			}
			
			public void Info(object message, Exception exception)
			{
				Log(LogLevel.Info, message, exception);
			}
			
			public void Warn(object message)
			{
				Log(LogLevel.Warn, message, null);
			}
			
			public void Warn(object message, Exception exception)
			{
				Log(LogLevel.Warn, message, exception);
			}
			
			public void Error(object message)
			{
				Log(LogLevel.Error, message, null);
			}
			
			public void Error(object message, Exception exception)
			{
				Log(LogLevel.Error, message, exception);
			}
			
			public void Log(LogLevel level, object message)
			{
				Log(level, message, null);
			}

			public void Log(LogLevel level, object message, Exception exception)
			{
				string logName = Name;
				if( IsEnabledFor(level) )
				{
					switch( level )
					{
					case LogLevel.Debug:
					case LogLevel.Info:
						UnityEngine.Debug.Log(FormatLogLine(logName, level, message, exception));
						break;
						
					case LogLevel.Warn:
						UnityEngine.Debug.LogWarning(FormatLogLine(logName, level, message, exception));
						break;
						
					case LogLevel.Error:
						UnityEngine.Debug.LogError(FormatLogLine(logName, level, message, exception));
						break;
					}
				}
			}
			
			static System.Text.StringBuilder _logLineSB = new System.Text.StringBuilder();
			public static string FormatLogLine(string logName, LogLevel level, object message, Exception exception)
			{
				_logLineSB.Length = 0;
				if( _logLineSB.Capacity > 2048 )
					_logLineSB.Capacity = 2048;
				
				// TODO: ToUpperInvariant() allocates and it shouldn't to avoid excessive GC...
				
				if( message != null )
				{
					if( exception != null )
						_logLineSB.AppendFormat("{0} [{1}] {2}: {3}", logName, level.ToString().ToUpperInvariant(), message.ToString(), exception.ToString());
					else
						_logLineSB.AppendFormat("{0} [{1}] {2}", logName, level.ToString().ToUpperInvariant(), message.ToString());
				}
				else if( exception != null )
				{
					_logLineSB.AppendFormat("{0} [{1}] {2}", logName, level.ToString().ToUpperInvariant(), exception.ToString());
				}
				return _logLineSB.ToString();
			}

			public bool IsEnabledFor(LogLevel level)
			{
				return Logger.IsLogging && EffectiveLevel <= level;
			}
			
			public bool IsDebugEnabled { get { return IsEnabledFor(LogLevel.Debug); } } 
			public bool IsInfoEnabled { get { return IsEnabledFor(LogLevel.Info); } }
			public bool IsWarnEnabled { get { return IsEnabledFor(LogLevel.Warn); } }
			public bool IsErrorEnabled { get { return IsEnabledFor(LogLevel.Error); } }
		}
		
		
		
	}
}