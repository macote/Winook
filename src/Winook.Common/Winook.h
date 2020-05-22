#pragma once

#include "MainWindowFinder.h"
#include "StreamLineWriter.h"

#include <Windows.h>

#include <string>
#include <regex>

#if defined(__MINGW32__) && !defined(PROCESSOR_ARCHITECTURE_ARM64)
#define PROCESSOR_ARCHITECTURE_ARM64            12
#endif

constexpr auto kProcessWaitForInputIdleTimeoutIntervalInMilliseconds = 2000;
constexpr auto kPathBufferSize = 1024;

#if defined(WINOOK64)
const std::wstring kKeyboardHookLibName = std::wstring(TEXT("Winook.Lib.Keyboard.x64.dll"));
const std::wstring kMouseHookLibName = std::wstring(TEXT("Winook.Lib.Mouse.x64.dll"));
#else
const std::wstring kKeyboardHookLibName = std::wstring(TEXT("Winook.Lib.Keyboard.x86.dll"));
const std::wstring kMouseHookLibName = std::wstring(TEXT("Winook.Lib.Mouse.x86.dll"));
#endif

const std::string kKeyboardHookProcName = std::string("KeyboardHookProc");
const std::string kMouseHookProcName = std::string("MouseHookProc");

class Winook
{
public:
    static std::wstring GetConfigFilePath(LPCTSTR libfullpath, DWORD processid, DWORD threadid, INT hooktype);
    static std::wstring FindConfigFilePath(int hooktype);
public:
    Winook(INT hooktype, INT processid, std::wstring port, std::wstring mutexguid)
        : hooktype_(hooktype), processid_(processid), port_(port), mutexguid_(mutexguid)
    {
    }
    virtual ~Winook()
    {
        CleanUp();
    }
public:
    BOOL IsBitnessMatch();
    void Hook();
private:
    void GetProcessFullPath();
    void WaitForProcess();
    void FindProcessMainWindowThreadId();
    void GetLibPath();
    void LoadLib();
    void WriteConfigurationFile();
    void SetupHook();
    void WaitOnHostMutex();
    void LogError(std::string errormessage);
    void HandleError(std::string errormessage);
    void HandleError(std::string errormessage, DWORD errorcode);
    void CleanUp();
private:
    INT hooktype_;
    INT processid_;
    std::wstring port_;
    std::wstring mutexguid_;
    HANDLE process_{ NULL };
    std::wstring processfullpath_;
    DWORD threadid_{};
    std::wstring hooklibpath_;
    HHOOK hook_{ NULL };
    HMODULE hooklib_{ NULL };
    HANDLE mutex_{ NULL };
};

inline std::wstring Winook::GetConfigFilePath(LPCTSTR libfullpath, DWORD processid, DWORD threadid, INT hooktype)
{
    TCHAR temppath[kPathBufferSize];
    GetTempPath(kPathBufferSize, temppath);
    TCHAR configfilepath[kPathBufferSize];
    swprintf(configfilepath, sizeof(configfilepath), TEXT("%ls%ls%d%d%d"),
        temppath,
        std::regex_replace(libfullpath, std::wregex(TEXT("[\\\\]|[/]|[:]|[ ]")), TEXT("")).c_str(),
        processid,
        threadid,
        hooktype);

    return std::wstring(configfilepath);
}

inline std::wstring Winook::FindConfigFilePath(int hooktype)
{
    TCHAR modulepath[kPathBufferSize];
    GetModuleFileName(NULL, modulepath, kPathBufferSize);
    const auto configfilepath = GetConfigFilePath(modulepath, GetCurrentProcessId(), GetThreadId(GetCurrentThread()), hooktype);
    WIN32_FIND_DATA findfiledata;
    HANDLE find = FindFirstFile(configfilepath.c_str(), &findfiledata);
    if (find != INVALID_HANDLE_VALUE)
    {
        FindClose(find);
        return configfilepath;
    }

    return std::wstring();
}

inline void Winook::LogError(std::string errormessage)
{
    TCHAR temppath[kPathBufferSize];
    GetTempPath(kPathBufferSize, temppath);
    TCHAR errorfilepath[kPathBufferSize];
    swprintf(errorfilepath, sizeof(errorfilepath), TEXT("%ls%ls"), temppath, mutexguid_.c_str());

    StreamLineWriter errorfile(errorfilepath, false);
    errorfile.WriteLine(errormessage);
    errorfile.Close();
}

inline BOOL Winook::IsBitnessMatch()
{
    process_ = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, processid_);
    if (process_ == NULL)
    {
        HandleError("OpenProcess() failed", GetLastError());
    }

    BOOL isx64os{};
    SYSTEM_INFO systeminfo;
    GetNativeSystemInfo(&systeminfo);
    switch (systeminfo.wProcessorArchitecture)
    {
    case PROCESSOR_ARCHITECTURE_AMD64:
    case PROCESSOR_ARCHITECTURE_ARM64:
        isx64os = TRUE;
        break;
    case PROCESSOR_ARCHITECTURE_ARM:
    case PROCESSOR_ARCHITECTURE_INTEL:
        break;
    default:
        HandleError("Unsupported processor architecture");
        break;
    }

    BOOL iswow64process;
    if (!IsWow64Process(process_, &iswow64process))
    {
        HandleError("IsWow64Process() failed", GetLastError());
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
            return FALSE;
        }
    }
    else if (isx64os && !iswow64process)
    {
        // Let the 64-bit version handle the hook
        return FALSE;
    }

    return TRUE;
}

inline void Winook::Hook()
{
    GetProcessFullPath();
    WaitForProcess();
    FindProcessMainWindowThreadId();
    GetLibPath();
    LoadLib();
    WriteConfigurationFile();
    SetupHook();
    WaitOnHostMutex();
}

inline void Winook::GetProcessFullPath()
{
    TCHAR processfullpath[kPathBufferSize];
    DWORD processfullpathsize = sizeof(processfullpath);
    if (!QueryFullProcessImageName(process_, 0, processfullpath, &processfullpathsize))
    {
        HandleError("QueryFullProcessImageName() failed", GetLastError());
    }

    processfullpath_ = processfullpath;
}

inline void Winook::WaitForProcess()
{
    // Ensure target process' main window is ready
    const auto waitresult = WaitForInputIdle(process_, kProcessWaitForInputIdleTimeoutIntervalInMilliseconds);
    CloseHandle(process_);
    process_ = NULL;

    if (waitresult == WAIT_FAILED)
    {
        HandleError("WaitForInputIdle() failed", GetLastError());
    }
}

inline void Winook::FindProcessMainWindowThreadId()
{
    MainWindowFinder mainwindowfinder(processid_);
    const auto mainwindowhandle = mainwindowfinder.FindMainWindow();
    if (mainwindowhandle == NULL)
    {
        HandleError("FindMainWindow() failed", mainwindowfinder.lasterror());
    }

    threadid_ = GetWindowThreadProcessId(mainwindowhandle, NULL);
    if (threadid_ == 0)
    {
        HandleError("GetWindowThreadProcessId() failed", GetLastError());
    }
}

inline void Winook::GetLibPath()
{
    TCHAR modulepath[kPathBufferSize];
    if (!GetModuleFileName(NULL, modulepath, kPathBufferSize))
    {
        HandleError("GetModuleFileName() failed", GetLastError());
    }

    const auto modulepathtmp = std::wstring(modulepath);
    const auto modulefolder = modulepathtmp.substr(0, modulepathtmp.find_last_of(TEXT("\\")) + 1);
    TCHAR libpath[kPathBufferSize];
    std::wstring libname;
    if (hooktype_ == WH_KEYBOARD)
    {
        libname = kKeyboardHookLibName;
    }
    else if (hooktype_ == WH_MOUSE)
    {
        libname = kMouseHookLibName;
    }
    else
    {
        HandleError("Invalid hook type");
    }

    swprintf(libpath, sizeof(libpath), TEXT("%ls%ls"), modulefolder.c_str(), libname.c_str());
    
    hooklibpath_ = libpath;
}

inline void Winook::LoadLib()
{
    hooklib_ = LoadLibrary(hooklibpath_.c_str());
    if (hooklib_ == NULL)
    {
        HandleError("LoadLibrary() failed", GetLastError());
    }    
}

inline void Winook::WriteConfigurationFile()
{
    const auto configfilepath = Winook::GetConfigFilePath(processfullpath_.c_str(), processid_, threadid_, hooktype_);
    StreamLineWriter configfile(configfilepath, false);
    configfile.WriteLine(port_);
    configfile.Close();
}

inline void Winook::SetupHook()
{
    std::string hookprocname;
    if (hooktype_ == WH_KEYBOARD)
    {
        hookprocname = kKeyboardHookProcName;
    }
    else if (hooktype_ == WH_MOUSE)
    {
        hookprocname = kMouseHookProcName;
    }
    else
    {
        HandleError("Unsupported hook type");
    }

    const auto hookproc = (HOOKPROC)GetProcAddress(hooklib_, hookprocname.c_str());
    if (hookproc == NULL)
    {
        HandleError("GetProcAddress() failed", GetLastError());
    }

    hook_ = SetWindowsHookEx(hooktype_, hookproc, hooklib_, threadid_);
    if (hook_ == NULL)
    {
        HandleError("SetWindowsHookEx() failed", GetLastError());
    }
}

inline void Winook::WaitOnHostMutex()
{
    TCHAR mutexname[256];
    swprintf(mutexname, sizeof(mutexname), TEXT("Global\\%ls"), mutexguid_.c_str());
    mutex_ = OpenMutex(SYNCHRONIZE, FALSE, mutexname);
    if (mutex_ == NULL)
    {
        HandleError("OpenMutex() failed", GetLastError());
    }

    auto event = WaitForSingleObject(mutex_, INFINITE);
    if (event == WAIT_FAILED)
    {
        HandleError("WaitForSingleObject() failed", GetLastError());
    }
}

inline void Winook::HandleError(std::string errormessage)
{
    HandleError(errormessage, 0);
}

inline void Winook::HandleError(std::string errormessage, DWORD errorcode)
{
    std::stringstream ss;
    ss << errormessage;
    if (errorcode > 0)
    {
        ss << " (0x" << std::hex << std::uppercase << errorcode << ")";
    }

    const auto formattederrormessage = ss.str();
    LogError(formattederrormessage);

    throw std::runtime_error(formattederrormessage);
}

inline void Winook::CleanUp()
{
    if (process_ != NULL)
    {
        CloseHandle(process_);
    }

    if (mutex_ != NULL)
    {
        CloseHandle(mutex_);
    }

    if (hook_ != NULL)
    {
        if (!UnhookWindowsHookEx(hook_))
        {
            HandleError("UnhookWindowsHookEx() failed", GetLastError());
        }
    }

    if (hooklib_ != NULL)
    {
        if (!FreeLibrary(hooklib_))
        {
            HandleError("FreeLibrary() failed", GetLastError());
        }
    }
}