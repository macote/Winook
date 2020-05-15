#pragma once

#include <Windows.h>

#include <string>
#include <regex>

constexpr auto kPathBufferSize = 1024;

#if defined(WINOOKLIBHOST64)
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
