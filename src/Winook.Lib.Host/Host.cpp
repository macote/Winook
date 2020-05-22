#define STRICT
#define WIN32_LEAN_AND_MEAN

#include "Winook.h"

#include <Windows.h>
#include <shellapi.h>

#include <string>

#pragma comment (lib, "shlwapi.lib")

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

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nShowCmd)
{	
    INT argscount;
    const auto args = CommandLineToArgvW(GetCommandLine(), &argscount);

    Winook winook(std::stoi(args[1]), std::stoi(args[3]), std::wstring(args[2]), std::wstring(args[4]));

    LocalFree(args);

    try
    {
        if (winook.IsBitnessMatch())
        {
            winook.Hook();
        }
    }
    catch (const std::exception&)
    {
        return EXIT_FAILURE;
    }

    return EXIT_SUCCESS;
}
