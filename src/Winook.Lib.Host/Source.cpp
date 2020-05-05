#ifndef UNICODE
#define UNICODE
#endif
#ifndef _UNICODE
#define _UNICODE
#endif

#define STRICT
#define WIN32_LEAN_AND_MEAN

#define PATH_BUFFER_SIZE 1024

#define LOGWINOOKLIBHOST 0
#if _DEBUG && LOGWINOOKLIBHOST
#define LOGWINOOKLIBHOSTPATH TEXT("C:\\Temp\\WinookLibHost_")
#include "DebugHelper.h"
#endif

#include "ConfigHelper.h"
#include "MainWindowFinder.h"
#include "StreamLineWriter.h"

#include <Windows.h>
#include <shellapi.h>

#include <string>

#if defined(WINOOKLIBHOST64)
#define WINOOKLIBNAME TEXT("Winook.Lib.x64.dll")
#else
#define WINOOKLIBNAME TEXT("Winook.Lib.x86.dll")
#endif

#if _DEBUG
#define _CRTDBG_MAP_ALLOC
#include <stdlib.h>
#include <crtdbg.h>
#endif

#if _MSC_VER
#pragma warning(suppress: 28251)
#ifndef _iob_defined
#define _iob_defined
FILE _iob[] = { *stdin, *stdout, *stderr };
extern "C" FILE * __cdecl __iob_func(void) { return _iob; }
#endif
#endif

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{
	
#if _DEBUG && LOGWINOOKLIBHOST
    TimestampLogger logger(LOGWINOOKLIBHOSTPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

    INT argscount;
    const auto args = CommandLineToArgvW(GetCommandLine(), &argscount);

    auto port = std::wstring(args[1]);
    auto processid = std::stoi(args[2]);
    auto mutexguid = std::wstring(args[3]);

    LocalFree(args);

    // Find process main window thread id

    MainWindowFinder mainwindowfinder(processid);
    auto mainwindowhandle = mainwindowfinder.FindMainWindow();
    if (mainwindowhandle == NULL)
    {
        return EXIT_FAILURE;
    }

    auto threadid = GetWindowThreadProcessId(mainwindowhandle, NULL);
    if (threadid == 0)
    {
        return EXIT_FAILURE;
    }

    // Get process full path

    auto process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processid);
    TCHAR processfullpath[PATH_BUFFER_SIZE];
    DWORD processfullpathsize = sizeof(processfullpath);
    QueryFullProcessImageName(process, 0, processfullpath, &processfullpathsize);
    CloseHandle(process);

    // Determine dll path - dll is in same folder as self

    TCHAR modulepath[PATH_BUFFER_SIZE];
    GetModuleFileName(NULL, modulepath, PATH_BUFFER_SIZE);
    auto modulepathtmp = std::wstring(modulepath);
    auto modulefolder = modulepathtmp.substr(0, modulepathtmp.find_last_of(TEXT("\\")) + 1);
    TCHAR hooklibpath[PATH_BUFFER_SIZE];
    swprintf(hooklibpath, sizeof(hooklibpath), TEXT("%ls%ls"),
        modulefolder.c_str(), WINOOKLIBNAME);

    // Load dll

    auto hooklib = LoadLibrary(hooklibpath);

    // Build configuration file path

    auto configfilepath = ConfigHelper::GetConfigFilePath(processfullpath, hooklib, processid, threadid);

    // Write configuration file

    StreamLineWriter configfile(configfilepath, false);
    configfile.WriteLine(port);
    configfile.Close();

    // Setup hook

    auto hookproc = (HOOKPROC)GetProcAddress(hooklib, "MouseHookProc");
    auto mousehook = SetWindowsHookEx(WH_MOUSE, hookproc, hooklib, threadid);

    // Wait on host mutex

    TCHAR mutexname[256];
    swprintf(mutexname, sizeof(mutexname), TEXT("Global\\%ls"), mutexguid.c_str());
    auto mutex = OpenMutex(SYNCHRONIZE, FALSE, mutexname);
    WaitForSingleObject(mutex, INFINITE);
    CloseHandle(mutex);

    // Unhook

    if (mousehook != NULL)
    {
        UnhookWindowsHookEx(mousehook);
    }

    if (hooklib != NULL)
    {
        FreeLibrary(hooklib);
    }

    return EXIT_SUCCESS;
}
