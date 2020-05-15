CC = g++

SYS = ${MSYSTEM}
DUMPMACHINE = ${shell ${CC} -dumpmachine}

WARNS = -Wall

ifeq (${DUMPMACHINE},x86_64-w64-mingw32) 
	DLL = Winook.Lib.x64.dll
else
	DLL = Winook.Lib.x86.dll
endif

WINDRES = windres

OBJECTS = obj/Lib.o obj/FileStream.o obj/Winook.Lib.o

ifeq (${DUMPMACHINE},x86_64-w64-mingw32) 
	CFLAGS = -m64 -std=c++17 -I../Winook.Common -I../../3rd/asio/asio/include -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -DWINOOK64 -D_FORTIFY_SOURCE=2 -D_GLIBCXX_ASSERTIONS -DASIO_STANDALONE -O2 ${WARNS} -fmessage-length=0 -fasynchronous-unwind-tables
else
	CFLAGS = -std=c++17 -I../Winook.Common -I../../3rd/asio/asio/include -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -D_FORTIFY_SOURCE=2 -D_GLIBCXX_ASSERTIONS -DASIO_STANDALONE -O2 ${WARNS} -fmessage-length=0 -fasynchronous-unwind-tables
endif

ifneq (${config},release)
	CFLAGS += -D_DEBUG -g
	LDFLAGS = -shared -static -Wl,--kill-at,--subsystem,windows -lws2_32
else
	LDFLAGS = -s -shared -static -Wl,--kill-at,--subsystem,windows -lws2_32
endif

.PHONY: all clean

all: bin/${DLL}

clean:
ifeq (${SYS}, MINGW64)
	@if [ -d "bin" ]; then rm -r bin; fi
	@if [ -d "obj" ]; then rm -r obj; fi
else
	@if exist bin\* del /f /s /q bin 1>nul & rd /s /q bin
	@if exist obj\* del /f /s /q obj 1>nul & rd /s /q obj
endif

bin obj:
ifeq (${SYS}, MINGW64)
	@if [ ! -d "bin" ]; then mkdir bin; fi
	@if [ ! -d "obj" ]; then mkdir obj; fi
else
	@if not exist "$@" mkdir "$@"
endif

obj/Winook.Lib.o: Winook.Lib.rc
	${WINDRES} --language 0x0409 Winook.Lib.rc -o obj/Winook.Lib.o

obj/FileStream.o: ../Winook.Common/FileStream.cpp | obj
	${CC} ${CFLAGS} -c ../Winook.Common/FileStream.cpp -o obj/FileStream.o

obj/%.o: %.cpp | obj
	${CC} ${CFLAGS} -c "$<" -o "$@"

bin/${DLL}: ${OBJECTS} | bin
	${CC} -o "$@" ${OBJECTS} ${LDFLAGS}

