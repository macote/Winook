CC = g++

SYS = ${MSYSTEM}
DUMPMACHINE = ${shell ${CC} -dumpmachine}

WARNS = -Wall

ifeq (${DUMPMACHINE},x86_64-w64-mingw32) 
	EXE = Winook.Lib.Host.x64.exe
else
	EXE = Winook.Lib.Host.x86.exe
endif

WINDRES = windres

OBJECTS = obj/Host.o obj/FileStream.o obj/Winook.Lib.Host.o

ifeq (${DUMPMACHINE},x86_64-w64-mingw32) 
	CFLAGS = -m64 -std=c++17 -I../Winook.Common -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -DWINOOK64 -D_FORTIFY_SOURCE=2 -D_GLIBCXX_ASSERTIONS -O2 ${WARNS} -fmessage-length=0 -fasynchronous-unwind-tables
else
	CFLAGS = -std=c++17 -I../Winook.Common -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -D_FORTIFY_SOURCE=2 -D_GLIBCXX_ASSERTIONS -O2 ${WARNS} -fmessage-length=0 -fasynchronous-unwind-tables
endif

ifneq (${config},release)
	CFLAGS += -D_DEBUG -g
	LDFLAGS = -static -mwindows
else
	LDFLAGS = -s -static -mwindows
endif
    
.PHONY: all clean

all: bin/${EXE}

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

obj/Winook.Lib.Host.o: Winook.Lib.Host.rc
	${WINDRES} --language 0x0409 Winook.Lib.Host.rc -o obj/Winook.Lib.Host.o

obj/FileStream.o: ../Winook.Common/FileStream.cpp | obj
	${CC} ${CFLAGS} -c ../Winook.Common/FileStream.cpp -o obj/FileStream.o

obj/%.o: %.cpp | obj
	${CC} ${CFLAGS} -c "$<" -o "$@"

bin/${EXE}: ${OBJECTS} | bin
	${CC} ${LDFLAGS} -o "$@" ${OBJECTS}

