# Release Note

## 0.5.0

* Remove WebAPI dependencies. The only external dependencies are JSON.NET.
* Remove dependencies on System.ServiceModel, so that we can target the library to dotnet core in future.
* Re-target from .NET Framework 4.6.1 to 4.5 on Windows.
* Caching uri template while adding handler to make the library faster.