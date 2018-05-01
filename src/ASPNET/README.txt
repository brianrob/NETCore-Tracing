To enable automatic injection of this component, perform the following steps:

1. dotnet build
2. export ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=NETCore.Tracing.Service.ASPNET
3. export DOTNET_ADDITIONAL_DEPS=~/.dotnet/x64/additionalDeps/NETCore.Tracing.Service.ASPNET/
4. Run the application (e.g. dotnet path/to/dll)
