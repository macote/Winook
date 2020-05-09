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

const std::string kKeyboardHookProcName = std::string("KeyboardHookProc");
const std::string kMouseHookProcName = std::string("MouseHookProc");

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

    const auto hooktype = std::stoi(args[1]);
    const auto port = std::wstring(args[2]);
    const auto processid = std::stoi(args[3]);
    const auto mutexguid = std::wstring(args[4]);

    LocalFree(args);

    // Find process main window thread id

    MainWindowFinder mainwindowfinder(processid);
    const auto mainwindowhandle = mainwindowfinder.FindMainWindow();
    if (mainwindowhandle == NULL)
    {
        return EXIT_FAILURE;
    }

    const auto threadid = GetWindowThreadProcessId(mainwindowhandle, NULL);
    if (threadid == 0)
    {
        return EXIT_FAILURE;
    }

    // Get process full path

    const auto process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processid);
    if (process == NULL)        
    { 
        return EXIT_FAILURE;
    }

    TCHAR processfullpath[PATH_BUFFER_SIZE];
    DWORD processfullpathsize = sizeof(processfullpath);
    if (!QueryFullProcessImageName(process, 0, processfullpath, &processfullpathsize))
    {
        return EXIT_FAILURE;
    }

    CloseHandle(process);

    // Determine dll path - dll is in same folder as self

    TCHAR modulepath[PATH_BUFFER_SIZE];
    if (!GetModuleFileName(NULL, modulepath, PATH_BUFFER_SIZE))
    {
        return EXIT_FAILURE;
    }

    const auto modulepathtmp = std::wstring(modulepath);
    const auto modulefolder = modulepathtmp.substr(0, modulepathtmp.find_last_of(TEXT("\\")) + 1);
    TCHAR hooklibpath[PATH_BUFFER_SIZE];
    swprintf(hooklibpath, sizeof(hooklibpath), TEXT("%ls%ls"),
        modulefolder.c_str(), WINOOKLIBNAME);

    // Load dll

    const auto hooklib = LoadLibrary(hooklibpath);
    if (hooklib == NULL)
    {
        return EXIT_FAILURE;
    }

    // Build configuration file path

    const auto configfilepath = ConfigHelper::GetConfigFilePath(processfullpath, hooklib, processid, threadid);

    // Write configuration file

    StreamLineWriter configfile(configfilepath, false);
    configfile.WriteLine(port);
    configfile.Close();

    // Setup hook

    std::string hookprocname;
    if (hooktype == WH_KEYBOARD)
    {
        hookprocname = kKeyboardHookProcName;
    }
    else if (hooktype == WH_MOUSE)
    {
        hookprocname = kMouseHookProcName;
    }
    else
    {
        return EXIT_FAILURE;
    }
   
    const auto hookproc = (HOOKPROC)GetProcAddress(hooklib, hookprocname.c_str());
    if (hookproc == NULL)
    {
        return EXIT_FAILURE;
    }

    const auto hook = SetWindowsHookEx(hooktype, hookproc, hooklib, threadid);
    if (hook == NULL)
    {
        return EXIT_FAILURE;
    }

    // Wait on host mutex

    TCHAR mutexname[256];
    swprintf(mutexname, sizeof(mutexname), TEXT("Global\\%ls"), mutexguid.c_str());
    const auto mutex = OpenMutex(SYNCHRONIZE, FALSE, mutexname);
    auto event = WaitForSingleObject(mutex, INFINITE);
    if (event == WAIT_FAILED)
    {
        return EXIT_FAILURE;
    }
    
    CloseHandle(mutex);

    // Unhook

    if (hook != NULL)
    {
        if (!UnhookWindowsHookEx(hook))
        {
            return EXIT_FAILURE;
        }
    }

    if (hooklib != NULL)
    {
        if (!FreeLibrary(hooklib))
        {
            return EXIT_FAILURE;
        }
    }

    return EXIT_SUCCESS;
}
