#pragma once

#include <Windows.h>

//#include <codecvt>
#include <iomanip>
//#include <filesystem>
#include <fstream>
#include <sstream>
#include <string>

class TimestampLogger
{
public:
    TimestampLogger(const std::wstring& filepath)
        : TimestampLogger(filepath, FALSE) { }
    TimestampLogger(const std::wstring& filepath, BOOL autoflush)
        //: logfile_(std::wofstream(std::filesystem::path(filepath)))
        : logfile_(std::wofstream(filepath.c_str()))
    {
        //logfile_.imbue(std::locale(std::locale::empty(), new std::codecvt_utf8<wchar_t>));
        pwritecriticalsection_ = new CRITICAL_SECTION();
        if (!InitializeCriticalSectionAndSpinCount(pwritecriticalsection_, 0x00000400))
        {
            // TODO: handle error
        }
    }
    ~TimestampLogger()
    {
        if (pwritecriticalsection_ != nullptr)
        {
            DeleteCriticalSection(pwritecriticalsection_);
            delete pwritecriticalsection_;
        }

        Close();
    }
    void WriteLine(const std::string& line);
    void WriteLine(const std::wstring& line);
    void Close()
    {
        logfile_.close();
    }
    static std::wstring GetTimestampString();
    static std::wstring GetTimestampString(BOOL asvalidfilename);
private:
    std::wofstream logfile_;
    PCRITICAL_SECTION pwritecriticalsection_{ nullptr };
};

inline void TimestampLogger::WriteLine(const std::string& line)
{
    std::wstring wline(line.begin(), line.end());
    WriteLine(wline);
}

inline void TimestampLogger::WriteLine(const std::wstring& line)
{
    EnterCriticalSection(pwritecriticalsection_);

    SYSTEMTIME filetime;
    GetLocalTime(&filetime);
    std::wstringstream wss;
    wss << TEXT("[") << GetTimestampString() << TEXT("] ");
    logfile_ << wss.str() << line;

    LeaveCriticalSection(pwritecriticalsection_);
}

inline std::wstring TimestampLogger::GetTimestampString()
{
    return GetTimestampString(FALSE);
}

inline std::wstring TimestampLogger::GetTimestampString(BOOL asvalidfilename)
{
    SYSTEMTIME filetime;
    GetLocalTime(&filetime);
    std::wstringstream wss;
    const auto timeseparator = asvalidfilename ? TEXT(".") : TEXT(":");
    wss << filetime.wYear << TEXT("-");
    wss << std::setw(2) << std::setfill(TEXT('0')) << filetime.wMonth << TEXT("-");
    wss << std::setw(2) << std::setfill(TEXT('0')) << filetime.wDay << TEXT("T");
    wss << std::setw(2) << std::setfill(TEXT('0')) << filetime.wHour << timeseparator;
    wss << std::setw(2) << std::setfill(TEXT('0')) << filetime.wMinute << timeseparator;
    wss << std::setw(2) << std::setfill(TEXT('0')) << filetime.wSecond << TEXT(".");
    wss << std::setw(3) << std::setfill(TEXT('0')) << filetime.wMilliseconds;

    return wss.str();
}