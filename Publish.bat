cls
@echo **** 6.1.0 : UPDATED THE VERSION NUMBER IN THE PROJECT *AND* BATCH FILE? ****
pause

cls
@call BuildAndTest.bat

@echo ======================

set /p ShouldPublish=Publish 6.1.0 [yes]?
@if "%ShouldPublish%" == "yes" (
	@echo PUBLISHING
	dotnet nuget push .\Source\Lib\Fluxor\bin\Release\Fluxor.6.1.0.nupkg -k %MORRIS.NUGET.KEY% -s https://api.nuget.org/v3/index.json
	dotnet nuget push .\Source\Lib\Fluxor.Blazor.Web\bin\Release\Fluxor.Blazor.Web.6.1.0.nupkg -k %MORRIS.NUGET.KEY% -s https://api.nuget.org/v3/index.json
	dotnet nuget push .\Source\Lib\Fluxor.Blazor.Web.ReduxDevTools\bin\Release\Fluxor.Blazor.Web.ReduxDevTools.6.1.0.nupkg -k %MORRIS.NUGET.KEY% -s https://api.nuget.org/v3/index.json
)

