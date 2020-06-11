#include "Lib.h"
#include "Winook.h"
#include "MessageSender.h"

#if _DEBUG
#include <bitset>
#endif
#include <cinttypes>
//#include <filesystem>
#include <fstream>
#include <string>
#include <regex>

#if !defined(__MINGW32__)
#pragma comment (lib, "shlwapi.lib")
#endif

#define LOGWINOOKLIB 1
#if _DEBUG && LOGWINOOKLIB
#define LOGWINOOKLIBPATH TEXT("C:\\Temp\\WinookLibHookProc_")
#include "TimestampLogger.h"
#include "DebugHelper.h"
TimestampLogger Logger = TimestampLogger(LOGWINOOKLIBPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

asio::io_context io_context;
MessageSender messagesender(io_context);
WORD shiftctrlalt{};

BOOL WINAPI DllMain(HINSTANCE hinst, DWORD fdwReason, LPVOID lpReserved)
{
    switch (fdwReason)
    {
    case DLL_PROCESS_ATTACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_PROCESS_ATTACH"));
#endif
        if (!Initialize(hinst))
        {
            return FALSE;
        }

        break;

    case DLL_THREAD_ATTACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_THREAD_ATTACH"));
#endif
        break;

    case DLL_THREAD_DETACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_THREAD_DETACH"));
#endif
        break;

    case DLL_PROCESS_DETACH:
#if _DEBUG
        LogDllMain(hinst, TEXT("DLL_PROCESS_DETACH"));
#endif
        break;
    }

    return TRUE;
}

BOOL Initialize(HINSTANCE hinst)
{
    // Look for initialization file stored in %TEMP%

    HMODULE module;
    if (!GetModuleHandleEx(
        GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS | GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
        (LPCTSTR)Initialize,
        &module))
    {
        return FALSE;
    }

    TCHAR dllfilepath[kPathBufferSize];
    GetModuleFileName(module, dllfilepath, kPathBufferSize);
    int hooktype{};
    if (StrStrI(dllfilepath, kKeyboardHookLibName.c_str()) != NULL)
    {
        hooktype = WH_KEYBOARD;
        const auto lshift = GetKeyState(VK_LSHIFT);
        const auto lcontrol = GetKeyState(VK_LCONTROL);
        const auto lalt = GetKeyState(VK_LMENU);
        const auto rshift = GetKeyState(VK_RSHIFT);
        const auto rcontrol = GetKeyState(VK_RCONTROL);
        const auto ralt = GetKeyState(VK_RMENU);
#if _DEBUG
        std::bitset<16> shiftctrlaltbits1(shiftctrlalt);
        LogDll(std::string("shiftctrlalt init before: ") + shiftctrlaltbits1.to_string());
#endif
        shiftctrlalt = (lshift < 0) << kLeftShiftBitIndex
            | (lcontrol < 0) << kLeftControlBitIndex
            | (lalt < 0) << kLeftAltBitIndex
            | (rshift < 0) << kRightShiftBitIndex
            | (rcontrol < 0) << kRightControlBitIndex
            | (ralt < 0) << kRightAltBitIndex
            | (lshift < 0 || rshift < 0) << kShiftBitIndex
            | (lcontrol < 0 || rcontrol < 0) << kControlBitIndex
            | (lalt < 0 || ralt < 0) << kAltBitIndex;
#if _DEBUG
        std::bitset<16> shiftctrlaltbits2(shiftctrlalt);
        LogDll(std::string("shiftctrlalt init after: ") + shiftctrlaltbits2.to_string());
#endif
    }
    else if (StrStrI(dllfilepath, kMouseHookLibName.c_str()) != NULL)
    {
        hooktype = WH_MOUSE;
    }
    else
    {
        return FALSE; // Unsupported hook type
    }

    const auto configfilepath = Winook::FindConfigFilePath(hooktype);
    if (configfilepath.empty())
    {
        return TRUE; // Assume out-of-context initialization
    }

    std::ifstream configfile(configfilepath.c_str());
    //std::ifstream configfile(std::filesystem::path(configfilepath));
    std::string port;
    configfile >> port;
    configfile.close();
    DeleteFile(configfilepath.c_str());
    messagesender.Connect(port);

    return TRUE;
}

WORD UpdateShiftCtrlAltState(WORD current, INT bitindex, INT down)
{
#if _DEBUG
    std::bitset<16> shiftctrlaltbits1(current);
    LogDll(std::string("UpdateShiftCtrlAltState() before: ") + shiftctrlaltbits1.to_string());
    LogDll(std::string("bitindex: ") + std::to_string(bitindex));
    LogDll(std::string("down: ") + std::to_string(down));
#endif
    BITMASK_CLEAR(current, 7);
    const auto temp = down ? BIT_SET(current, bitindex) : BIT_CLEAR(current, bitindex);
#if _DEBUG
    const auto temp2 = temp
        | (BIT_CHECK(temp, kLeftShiftBitIndex) || BIT_CHECK(temp, kRightShiftBitIndex)) << kShiftBitIndex
        | (BIT_CHECK(temp, kLeftControlBitIndex) || BIT_CHECK(temp, kRightControlBitIndex)) << kControlBitIndex
        | (BIT_CHECK(temp, kLeftAltBitIndex) || BIT_CHECK(temp, kRightAltBitIndex)) << kAltBitIndex;
    std::bitset<16> shiftctrlaltbits2(temp2);
    LogDll(std::string("UpdateShiftCtrlAltState() after: ") + shiftctrlaltbits2.to_string());
    return temp2;
#else
    return temp
        | (BIT_CHECK(temp, kLeftShiftBitIndex) || BIT_CHECK(temp, kRightShiftBitIndex)) << kShiftBitIndex
        | (BIT_CHECK(temp, kLeftControlBitIndex) || BIT_CHECK(temp, kRightControlBitIndex)) << kControlBitIndex
        | (BIT_CHECK(temp, kLeftAltBitIndex) || BIT_CHECK(temp, kRightAltBitIndex)) << kAltBitIndex;
#endif
}

LRESULT CALLBACK KeyboardHookProc(int code, WPARAM wParam, LPARAM lParam)
{
#if _DEBUG
    LogDll(DebugHelper::FormatKeyboardHookMessage(code, wParam, lParam));
#endif
    if (code == HC_ACTION)
    {
        switch ((DWORD)wParam)
        {
        case VK_SHIFT:
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kLeftShiftBitIndex, GetKeyState(VK_LSHIFT) < 0);
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kRightShiftBitIndex, GetKeyState(VK_RSHIFT) < 0);
            break;
        case VK_CONTROL:
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kLeftControlBitIndex, GetKeyState(VK_LCONTROL) < 0);
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kRightControlBitIndex, GetKeyState(VK_RCONTROL) < 0);
            break;
        case VK_MENU:
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kLeftAltBitIndex, GetKeyState(VK_LMENU) < 0);
            shiftctrlalt = UpdateShiftCtrlAltState(shiftctrlalt, kRightAltBitIndex, GetKeyState(VK_RMENU) < 0);
            break;
        default:
            break;
        }

        HookKeyboardMessage hkm;
        hkm.keyCode = (WORD)wParam;
        hkm.modifiers = shiftctrlalt;
        hkm.flags = (DWORD)lParam;
        messagesender.SendMessage(&hkm, sizeof(HookKeyboardMessage));
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

LRESULT CALLBACK MouseHookProc(int code, WPARAM wParam, LPARAM lParam) 
{
#if _DEBUG
    LogDll(DebugHelper::FormatMouseHookMessage(code, wParam, lParam));
#endif
    if (code == HC_ACTION)
    {
        HookMouseMessage hmm{};
        hmm.messageCode = (DWORD)wParam;
        if (wParam == WM_MOUSEWHEEL)
        {
            auto pmhsx = (PMOUSEHOOKSTRUCTEX)lParam;
            hmm.pointX = pmhsx->pt.x;
            hmm.pointY = pmhsx->pt.y;
            hmm.hwnd = (DWORD)PtrToInt(pmhsx->hwnd);
            hmm.hitTestCode = (DWORD)pmhsx->wHitTestCode;
            hmm.zDelta = HIWORD(pmhsx->mouseData);
        }
        else
        {
            auto pmhs = (PMOUSEHOOKSTRUCT)lParam;
            hmm.pointX = pmhs->pt.x;
            hmm.pointY = pmhs->pt.y;
            hmm.hwnd = (DWORD)PtrToInt(pmhs->hwnd);
            hmm.hitTestCode = (DWORD)pmhs->wHitTestCode;
        }

        messagesender.SendMessage(&hmm, sizeof(HookMouseMessage));
    }

    return CallNextHookEx(NULL, code, wParam, lParam);
}

#if _DEBUG
void LogDll(std::string message)
{
#if LOGWINOOKLIB
    Logger.WriteLine(message);
#endif
}

void LogDll(std::wstring message)
{
#if LOGWINOOKLIB
    Logger.WriteLine(message);
#endif
}

void LogDllMain(HINSTANCE hinst, std::wstring reason)
{
#if LOGWINOOKLIB
    std::wstringstream wss;
    wss << std::setw(16) << std::setfill(L'0') << std::hex << hinst;
    TCHAR procInfo[256];
    swprintf(procInfo, sizeof(procInfo), TEXT("Instance: %lx; Reason: %ls; ProcessId: %d; ThreadId: %d"),
        PtrToLong(hinst), reason.c_str(), GetCurrentProcessId(), GetThreadId(GetCurrentThread()));
    Logger.WriteLine(procInfo);
#endif
}
#endif