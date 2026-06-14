CC = g++

SYS = $(MSYSTEM)
DUMPMACHINE = $(shell $(CC) -dumpmachine)
MSYS2_SHELL = $(filter MINGW% UCRT% CLANG%,$(SYS))

WARNS = -Wall

ifeq ($(DUMPMACHINE),x86_64-w64-mingw32) 
	DLL = Winook.Lib.Keyboard.x64.dll Winook.Lib.Mouse.x64.dll
else
	DLL = Winook.Lib.Keyboard.x86.dll Winook.Lib.Mouse.x86.dll
endif
TARGETS = $(addprefix bin/,$(DLL))

WINDRES = windres

OBJDIR = obj/$(DUMPMACHINE)
OBJECTS = $(OBJDIR)/Lib.o $(OBJDIR)/Winook.Lib.o

ifneq (,$(MSYS2_SHELL))
ifneq ($(filter /%,$(CC)),)
	CCENV = PATH="$(patsubst %/,%,$(dir $(CC))):$$PATH"
endif
ifneq ($(filter /%,$(WINDRES)),)
	WINDRESENV = PATH="$(patsubst %/,%,$(dir $(WINDRES))):$$PATH"
endif
endif

ifeq ($(DUMPMACHINE),x86_64-w64-mingw32) 
	CFLAGS = -m64 -std=c++17 -I../Winook.Common -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -DWINOOK64 -D_GLIBCXX_ASSERTIONS -O2 $(WARNS) -fmessage-length=0 -fasynchronous-unwind-tables
else
	CFLAGS = -std=c++17 -I../Winook.Common -DWINVER=0x0603 -D_WIN32_WINNT=0x0603 -DUNICODE -D_UNICODE -D_GLIBCXX_ASSERTIONS -O2 $(WARNS) -fmessage-length=0 -fasynchronous-unwind-tables
endif

ifneq ($(config),release)
	CFLAGS += -D_DEBUG -g
	LDFLAGS = -shared -static -Wl,--kill-at,--subsystem,windows
else
	LDFLAGS = -s -shared -static -Wl,--kill-at,--subsystem,windows
endif

.PHONY: all clean

all: $(TARGETS)

clean:
ifneq (,$(MSYS2_SHELL))
	@if [ -d "bin" ]; then rm -r bin; fi
	@if [ -d "obj" ]; then rm -r obj; fi
else
	@if exist bin\* del /f /s /q bin 1>nul & rd /s /q bin
	@if exist obj\* del /f /s /q obj 1>nul & rd /s /q obj
endif

bin obj $(OBJDIR):
ifneq (,$(MSYS2_SHELL))
	@if [ ! -d "$@" ]; then mkdir -p "$@"; fi
else
	@if not exist "$@" mkdir "$@"
endif

$(OBJDIR)/Winook.Lib.o: Winook.Lib.rc | $(OBJDIR)
	$(WINDRESENV) $(WINDRES) --language 0x0409 Winook.Lib.rc -o "$@"

$(OBJDIR)/%.o: %.cpp | $(OBJDIR)
	$(CCENV) $(CC) $(CFLAGS) -c "$<" -o "$@"

$(TARGETS): $(OBJECTS) | bin
	$(CCENV) $(CC) -o "$@" $(OBJECTS) $(LDFLAGS)
