#pragma once

#include <Windows.h>

#include <string>
#include <regex>

#define PATH_BUFFER_SIZE 1024

class ConfigHelper
{
public:
    static std::wstring GetConfigFilePath(LPCTSTR libfullpath, HINSTANCE hooklib, DWORD processid, DWORD threadid);
};

inline std::wstring ConfigHelper::GetConfigFilePath(LPCTSTR libfullpath, HINSTANCE hooklib, DWORD processid, DWORD threadid)
{
    TCHAR temppath[PATH_BUFFER_SIZE];
    GetTempPath(PATH_BUFFER_SIZE, temppath);
    TCHAR configfilepath[PATH_BUFFER_SIZE];
    swprintf(configfilepath, sizeof(configfilepath), TEXT("%ls%ls%d%d%d"),
        temppath,
        std::regex_replace(libfullpath, std::wregex(TEXT("[\\\\]|[/]|[:]|[ ]")), TEXT("")).c_str(),
        PtrToInt(hooklib),
        processid,
        threadid);

    return std::wstring(configfilepath);
}
