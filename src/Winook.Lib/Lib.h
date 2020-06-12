#pragma once

#define STRICT
#define WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <Shlwapi.h>

#include <string>

#define BIT_SET(a,b) ((a) |= (1ULL<<(b)))
#define BIT_CLEAR(a,b) ((a) &= ~(1ULL<<(b)))
#define BIT_FLIP(a,b) ((a) ^= (1ULL<<(b)))
#define BIT_CHECK(a,b) (!!((a) & (1ULL<<(b))))

#define BITMASK_SET(x,y) ((x) |= (y))
#define BITMASK_CLEAR(x,y) ((x) &= (~(y)))
#define BITMASK_FLIP(x,y) ((x) ^= (y))
#define BITMASK_CHECK_ALL(x,y) (((x) & (y)) == (y)) // warning: evaluates y twice
#define BITMASK_CHECK_ANY(x,y) ((x) & (y))

constexpr auto kLeftShiftBitIndex = 8;
constexpr auto kLeftControlBitIndex = 7;
constexpr auto kLeftAltBitIndex = 6;
constexpr auto kRightShiftBitIndex = 5;
constexpr auto kRightControlBitIndex = 4;
constexpr auto kRightAltBitIndex = 3;
constexpr auto kShiftBitIndex = 2;
constexpr auto kControlBitIndex = 1;
constexpr auto kAltBitIndex = 0;

struct HookMouseMessage
{
    DWORD messageCode;
    DWORD pointX;
    DWORD pointY;
    DWORD hwnd;
    DWORD hitTestCode;
    DWORD zDelta;
};

struct HookKeyboardMessage
{
    WORD keyCode;
    WORD modifiers;
    DWORD flags;
};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpReserved);

BOOL Initialize(HINSTANCE hinst);
WORD UpdateShiftCtrlAltState(WORD current, INT bitindex, INT down);

#if _DEBUG
void LogDll(std::string message);
void LogDll(std::wstring message);
void LogDllMain(HINSTANCE hinst, std::wstring reason);
#endif

extern "C" __declspec(dllexport) LRESULT CALLBACK KeyboardHookProc(int code, WPARAM wParam, LPARAM lParam);

extern "C" __declspec(dllexport) LRESULT CALLBACK MouseHookProc(int code, WPARAM wParam, LPARAM lParam);