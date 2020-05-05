#pragma once

#define STRICT
#define WIN32_LEAN_AND_MEAN

#include <Windows.h>

#include <string>

struct HookMouseMessage
{
    DWORD messageCode;
    DWORD pointX;
    DWORD pointY;
    DWORD hwnd;
    DWORD hitTestCode;
};

struct HookKeyboardMessage
{
    DWORD virtualKeyCode;
    DWORD flags;
};

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD fdwReason, LPVOID lpReserved);

void Initialize(HINSTANCE hinst);

#if _DEBUG
void LogDllMain(HINSTANCE hinst, std::wstring reason);
#endif

extern "C" __declspec(dllexport) LRESULT CALLBACK KeyboardHookProc(int code, WPARAM wParam, LPARAM lParam);

extern "C" __declspec(dllexport) LRESULT CALLBACK MouseHookProc(int code, WPARAM wParam, LPARAM lParam);