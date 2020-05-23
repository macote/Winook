#define STRICT
#define WIN32_LEAN_AND_MEAN

#include "Winook.h"

#include <Windows.h>
#include <shellapi.h>

#include <string>

#if !defined(__MINGW32__)
#pragma comment (lib, "shlwapi.lib")
#endif

#define LOGWINOOKLIBHOST 1
#if _DEBUG && LOGWINOOKLIBHOST
#define LOGWINOOKLIBHOSTPATH TEXT("C:\\Temp\\WinookLibHost_")
#include "DebugHelper.h"
TimestampLogger Logger(LOGWINOOKLIBHOSTPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{	
    try
    {
        INT argscount;
        const auto args = CommandLineToArgvW(GetCommandLine(), &argscount);

        if (argscount != 5)
        {
            return EXIT_FAILURE;
        }

        const auto hooktype = std::stoi(args[1]);
        const auto port = std::wstring(args[2]);
        const auto processid = std::stoi(args[3]);
        const auto mutexguid = std::wstring(args[4]);

        Winook winook(hooktype, processid, port, mutexguid);

        LocalFree(args);

        if (winook.IsBitnessMatch())
        {
            winook.Hook();
        }

        return EXIT_SUCCESS;
    }
    catch (const std::exception&)
    {
        return EXIT_FAILURE;
    }
}
