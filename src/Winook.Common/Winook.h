#pragma once

#include <Windows.h>

#include <string>
#include <regex>

constexpr auto kPathBufferSize = 1024;

#if defined(WINOOKLIBHOST64)
const std::wstring kKeyboardHookLibName = std::wstring(TEXT("Winook.Lib.x64.dll"));
const std::wstring kMouseHookLibName = std::wstring(TEXT("Winook.Lib.x64.dll"));
#else
const std::wstring kKeyboardHookLibName = std::wstring(TEXT("Winook.Lib.x86.dll"));
const std::wstring kMouseHookLibName = std::wstring(TEXT("Winook.Lib.x86.dll"));
#endif

const std::string kKeyboardHookProcName = std::string("KeyboardHookProc");
const std::string kMouseHookProcName = std::string("MouseHookProc");

class Winook
{
public:
    static std::wstring GetConfigFilePath(LPCTSTR libfullpath, HINSTANCE hooklib, DWORD processid, DWORD threadid);
};

inline std::wstring Winook::GetConfigFilePath(LPCTSTR libfullpath, HINSTANCE hooklib, DWORD processid, DWORD threadid)
{
    TCHAR temppath[kPathBufferSize];
    GetTempPath(kPathBufferSize, temppath);
    TCHAR configfilepath[kPathBufferSize];
    swprintf(configfilepath, sizeof(configfilepath), TEXT("%ls%ls%d%d%d"),
        temppath,
        std::regex_replace(libfullpath, std::wregex(TEXT("[\\\\]|[/]|[:]|[ ]")), TEXT("")).c_str(),
        PtrToInt(hooklib),
        processid,
        threadid);

    return std::wstring(configfilepath);
}
