@echo off
chcp 65001 >nul
cd /d "%~dp0"
echo ============================================
echo   GitLume 一键启动
echo   首次运行会自动编译，请稍候...
echo ============================================
where git >nul 2>nul
if errorlevel 1 (
    echo [错误] 未检测到 git，请先安装 Git for Windows: https://git-scm.com/download/win
    pause
    exit /b 1
)

rem 优先使用用户目录下的 dotnet（系统 C:\Program Files\dotnet 若损坏则不影响本项目）
set "DOTNET_ROOT=%USERPROFILE%\.dotnet"
set "PATH=%USERPROFILE%\.dotnet;%PATH%"

if not exist "GitLume\bin\Debug\net8.0-windows\GitLume.exe" (
    dotnet build GitLume\GitLume.csproj -c Debug -v q
    if errorlevel 1 (
        echo [错误] 编译失败，请确认已安装 .NET 8/9 SDK，或用 Visual Studio 2022 打开 GitLume.sln 编译。
        pause
        exit /b 1
    )
)
start "" "GitLume\bin\Debug\net8.0-windows\GitLume.exe"
exit /b 0
