@pushd %~dp0
@dotnet run --project "./tools/SmartBackup.Build/SmartBackup.Build.csproj" -- %*
@popd