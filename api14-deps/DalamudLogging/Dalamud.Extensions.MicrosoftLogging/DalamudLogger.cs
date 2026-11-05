using ILogger = Microsoft.Extensions.Logging.ILogger;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Dalamud.Extensions.MicrosoftLogging;

public sealed class DalamudLogger : ILogger
{
	private sealed class NullScope : IDisposable
	{
		public static NullScope Instance { get; } = new NullScope();

		private NullScope()
		{
		}

		public void Dispose()
		{
		}
	}

	private readonly string _name;

	private readonly string _assemblyName;

	private readonly IExternalScopeProvider? _scopeProvider;

	private readonly Serilog.ILogger _pluginLogDelegate;

	public DalamudLogger(string name, Assembly assembly, IExternalScopeProvider? scopeProvider)
	{
		_name = name;
		_assemblyName = assembly.GetName().Name ?? typeof(DalamudLogger).Assembly.GetName().Name ?? "{unknown DalamudLogger}";
		_scopeProvider = scopeProvider;
		_pluginLogDelegate = Serilog.Log.ForContext("SourceContext", (object)_assemblyName, false);
	}

	public IDisposable BeginScope<TState>(TState state) where TState : notnull
	{
		return _scopeProvider?.Push(state) ?? NullScope.Instance;
	}

	public bool IsEnabled(LogLevel logLevel)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (logLevel != LogLevel.None)
		{
			return IsEnabled(ToSerilogLevel(logLevel));
		}
		return false;
	}

	private bool IsEnabled(LogEventLevel logEventLevel)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return _pluginLogDelegate.IsEnabled(logEventLevel);
	}

	public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		if (logLevel == LogLevel.None)
		{
			return;
		}
		LogEventLevel val = ToSerilogLevel(logLevel);
		if (!IsEnabled(val))
		{
			return;
		}
		if (formatter == null)
		{
			throw new ArgumentNullException("formatter");
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[').Append(_assemblyName).Append("] ");
		_scopeProvider?.ForEachScope(delegate(object? scope, StringBuilder builder)
		{
			if (scope is IEnumerable<KeyValuePair<string, object>> enumerable)
			{
				{
					foreach (KeyValuePair<string, object> item in enumerable)
					{
						builder.Append('<').Append(item.Key).Append('=')
							.Append(item.Value)
							.Append("> ");
					}
					return;
				}
			}
			if (scope != null)
			{
				builder.Append('<').Append(scope).Append("> ");
			}
		}, stringBuilder);
		stringBuilder.Append(_name).Append(": ").Append(formatter(state, null));
		_pluginLogDelegate.Write(val, exception, stringBuilder.ToString());
	}

	private LogEventLevel ToSerilogLevel(LogLevel logLevel)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		return (LogEventLevel)(logLevel switch
		{
			LogLevel.Critical => 5,
			LogLevel.Error => 4,
			LogLevel.Warning => 3,
			LogLevel.Information => 2,
			LogLevel.Debug => 1,
			LogLevel.Trace => 0,
			_ => throw new ArgumentOutOfRangeException("logLevel", logLevel, null),
		});
	}
}
