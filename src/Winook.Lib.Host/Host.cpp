#define STRICT
#define WIN32_LEAN_AND_MEAN

#include "Winook.h"
#include "MainWindowFinder.h"
#include "StreamLineWriter.h"

#include <Windows.h>
#include <shellapi.h>

#include <string>

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

#define LOGWINOOKLIBHOST 1
#if _DEBUG && LOGWINOOKLIBHOST
#define LOGWINOOKLIBHOSTPATH TEXT("C:\\Temp\\WinookLibHost_")
#include "DebugHelper.h"
TimestampLogger Logger(LOGWINOOKLIBHOSTPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

#define PROCESS_WAITFORINPUTIDLE_TIMEOUT_INTERVAL_IN_MILLISECONDS 2000

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{	
    INT argscount;
    const auto args = CommandLineToArgvW(GetCommandLine(), &argscount);

    const auto hooktype = std::stoi(args[1]);
    const auto port = std::wstring(args[2]);
    const auto processid = std::stoi(args[3]);
    const auto mutexguid = std::wstring(args[4]);

    LocalFree(args);

    // Get process full path

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("OpenProcess()"));
#endif
    const auto process = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processid);
    if (process == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("OpenProcess() failed."));
#endif
        return EXIT_FAILURE;
    }

    BOOL isx64os{};
    SYSTEM_INFO systeminfo;
    GetNativeSystemInfo(&systeminfo);
    if (systeminfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_AMD64)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("PROCESSOR_ARCHITECTURE_AMD64"));
#endif
        isx64os = TRUE;
    }
    else if (systeminfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("PROCESSOR_ARCHITECTURE_INTEL"));
#endif
    }
    else
    {
        return EXIT_FAILURE;
    }

    BOOL iswow64process;
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("IsWow64Process()"));
#endif
    if (!IsWow64Process(process, &iswow64process))
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("IsWow64Process() failed."));
#endif
        return EXIT_FAILURE;
    }

#if defined WINOOK64
    const auto isx64binary = TRUE;
#else
    const auto isx64binary = FALSE;
#endif

    if (isx64binary)
    {
        if (iswow64process)
        {
            // Let the 32-bit version handle the hook
#if _DEBUG && LOGWINOOKLIBHOST
            Logger.WriteLine(TEXT("isx64binary && iswow64process"));
#endif
            return EXIT_SUCCESS;
        }
    }
    else if (isx64os && !iswow64process)
    {
        // Let the 64-bit version handle the hook
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("!isx64binary && isx64os && !iswow64process"));
#endif
        return EXIT_SUCCESS;
    }
 
    TCHAR processfullpath[kPathBufferSize];
    DWORD processfullpathsize = sizeof(processfullpath);
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("QueryFullProcessImageName()"));
#endif
    if (!QueryFullProcessImageName(process, 0, processfullpath, &processfullpathsize))
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("QueryFullProcessImageName() failed."));
#endif
        return EXIT_FAILURE;
    }

    // Ensure its main window is ready

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("WaitForInputIdle()"));
#endif
    const auto waitresult = WaitForInputIdle(process, PROCESS_WAITFORINPUTIDLE_TIMEOUT_INTERVAL_IN_MILLISECONDS);
    CloseHandle(process);

    if (waitresult == WAIT_FAILED)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("WaitForInputIdle() failed."));
#endif
        return EXIT_FAILURE;
    }

    // Find process main window thread id

    MainWindowFinder mainwindowfinder(processid);
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("FindMainWindow()"));
#endif
    const auto mainwindowhandle = mainwindowfinder.FindMainWindow();
    if (mainwindowhandle == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("FindMainWindow() failed."));
#endif
        return EXIT_FAILURE;
    }

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("GetWindowThreadProcessId()"));
#endif
    const auto threadid = GetWindowThreadProcessId(mainwindowhandle, NULL);
    if (threadid == 0)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("GetWindowThreadProcessId() failed."));
#endif
        return EXIT_FAILURE;
    }

    // Get lib path

    TCHAR modulepath[kPathBufferSize];
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("GetModuleFileName()"));
#endif
    if (!GetModuleFileName(NULL, modulepath, kPathBufferSize))
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("GetModuleFileName() failed."));
#endif
        return EXIT_FAILURE;
    }

    const auto modulepathtmp = std::wstring(modulepath);
    const auto modulefolder = modulepathtmp.substr(0, modulepathtmp.find_last_of(TEXT("\\")) + 1);
    TCHAR hooklibpath[kPathBufferSize];
    std::wstring hooklibname;
    if (hooktype == WH_KEYBOARD)
    {
        hooklibname = kKeyboardHookLibName;
    }
    else if (hooktype == WH_MOUSE)
    {
        hooklibname = kMouseHookLibName;
    }
    else
    {
        return EXIT_FAILURE;
    }

    swprintf(hooklibpath, sizeof(hooklibpath), TEXT("%ls%ls"),
        modulefolder.c_str(), hooklibname.c_str());

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("hooklibpath: ") + std::wstring(hooklibpath));
#endif

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("LoadLibrary()"));
#endif
    const auto hooklib = LoadLibrary(hooklibpath);
    if (hooklib == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("LoadLibrary() failed."));
#endif
        return EXIT_FAILURE;
    }

    // Write configuration file

    const auto configfilepath = Winook::GetConfigFilePath(processfullpath, processid, threadid, hooktype);
    StreamLineWriter configfile(configfilepath, false);
    configfile.WriteLine(port);
    configfile.Close();
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("configfilepath: ") + configfilepath);
#endif

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
   
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("GetProcAddress()"));
#endif
    const auto hookproc = (HOOKPROC)GetProcAddress(hooklib, hookprocname.c_str());
    if (hookproc == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("GetProcAddress() failed."));
#endif
        return EXIT_FAILURE;
    }

#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("SetWindowsHookEx()"));
#endif
    const auto hook = SetWindowsHookEx(hooktype, hookproc, hooklib, threadid);
    if (hook == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("SetWindowsHookEx() failed."));
#endif
        return EXIT_FAILURE;
    }

    // Wait on host mutex

    TCHAR mutexname[256];
    swprintf(mutexname, sizeof(mutexname), TEXT("Global\\%ls"), mutexguid.c_str());
#if _DEBUG && LOGWINOOKLIBHOST
    Logger.WriteLine(TEXT("OpenMutex()"));
#endif
    const auto mutex = OpenMutex(SYNCHRONIZE, FALSE, mutexname);
    if (mutex == NULL)
    {
#if _DEBUG && LOGWINOOKLIBHOST
        Logger.WriteLine(TEXT("OpenMutex() failed."));
#endif
        return EXIT_FAILURE;
    }

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
