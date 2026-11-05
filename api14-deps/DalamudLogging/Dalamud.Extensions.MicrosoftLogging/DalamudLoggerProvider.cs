using System;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Dalamud.Extensions.MicrosoftLogging;

public sealed class DalamudLoggerProvider : ILoggerProvider, IDisposable, ISupportExternalScope
{
	private readonly Assembly _assembly;

	private IExternalScopeProvider? _scopeProvider;

	public DalamudLoggerProvider(Assembly assembly)
	{
		_assembly = assembly;
	}

	ILogger ILoggerProvider.CreateLogger(string categoryName)
	{
		return CreateLoggerImpl(categoryName);
	}

	public ILogger CreateLogger(Type type)
	{
		return CreateLoggerImpl(type.FullName ?? type.ToString());
	}

	public ILogger CreateLogger<T>()
	{
		return CreateLoggerImpl(typeof(T).FullName ?? typeof(T).ToString());
	}

	private ILogger CreateLoggerImpl(string categoryName)
	{
		return new DalamudLogger(categoryName, _assembly, _scopeProvider);
	}

	public void SetScopeProvider(IExternalScopeProvider scopeProvider)
	{
		_scopeProvider = scopeProvider;
	}

	public void Dispose()
	{
	}
}
