#pragma once

#include <Windows.h>

#include <winsock2.h>
#include <ws2tcpip.h>

#if !defined(__MINGW32__)
#pragma comment (lib, "ws2_32.lib")
#endif

#define LOGWINOOKMESSAGESENDER 1
#if _DEBUG && LOGWINOOKMESSAGESENDER
#define LOGWINOOKMESSAGESENDERPATH TEXT("C:\\Temp\\WinookMessageSender_")
#include "DebugHelper.h"
#include "TimestampLogger.h"
TimestampLogger MessageSenderLogger = TimestampLogger(LOGWINOOKMESSAGESENDERPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

class MessageSender
{
public:
    MessageSender()
#if _DEBUG && LOGWINOOKMESSAGESENDER
        : logger_(TimestampLogger(LOGWINOOKMESSAGESENDERPATH + TimestampLogger::GetTimestampString(TRUE) + L".log", TRUE))
#endif
    {
        WSADATA wsadata;
        if (WSAStartup(MAKEWORD(2, 2), &wsadata))
        {
            throw std::runtime_error("WSAStartup() failed");
        }
    }
    ~MessageSender()
    {
        if (addrinfo_ != NULL)
        {
            freeaddrinfo(addrinfo_);
        }

        if (socket_ != INVALID_SOCKET)
        {
            closesocket(socket_);
        }

        if (!WSACleanup())
        {
            lasterror_ = WSAGetLastError();
        }
    }
    void Connect(std::string port);
    void SendMessage(void* data, size_t bytecount);
private:
    SOCKET socket_{ INVALID_SOCKET };
    int lasterror_{};
    struct addrinfo* addrinfo_{ NULL };
#if _DEBUG && LOGWINOOK
    TimestampLogger logger_;
#endif
};

inline void MessageSender::Connect(std::string port)
{
    struct addrinfo hints{};
    int result;

    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    result = getaddrinfo("127.0.0.1", port.c_str(), &hints, &addrinfo_);
    if (result != 0) {
        lasterror_ = result;
        throw std::runtime_error("getaddrinfo() failed");
    }

    socket_ = socket(addrinfo_->ai_family, addrinfo_->ai_socktype, addrinfo_->ai_protocol);
    if (socket_ == INVALID_SOCKET) {
        lasterror_ = WSAGetLastError();
        throw std::runtime_error("socket() failed");
    }

    result = connect(socket_, addrinfo_->ai_addr, (int)addrinfo_->ai_addrlen);
    if (result == SOCKET_ERROR)
    {
        lasterror_ = WSAGetLastError();
        throw std::runtime_error("Failed to connect to server");
    }
}

inline void MessageSender::SendMessage(void* data, size_t bytecount)
{
    send(socket_, reinterpret_cast<const char*>(data), static_cast<int>(bytecount), 0);
}