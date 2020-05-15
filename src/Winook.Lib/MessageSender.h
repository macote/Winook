#pragma once

#include <Windows.h>

#include "asio.hpp"

#define LOGWINOOKMESSAGESENDER 1
#if _DEBUG && LOGWINOOKMESSAGESENDER
#define LOGWINOOKMESSAGESENDERPATH TEXT("C:\\Temp\\WinookMessageSender_")
#include "DebugHelper.h"
TimestampLogger MessageSenderLogger = TimestampLogger(LOGWINOOKMESSAGESENDERPATH + TimestampLogger::GetTimestampString(TRUE) + TEXT(".log"), TRUE);
#endif

using asio::ip::tcp;

class MessageSender
{
public:
    MessageSender(asio::io_context& io_context) 
        : io_context_(io_context), socket_(tcp::socket(io_context))
    {
    }
    ~MessageSender()
    {
        io_context_.run();
    }
    void Connect(std::string port);
    void SendMessage(void* data, size_t bytecount);
private:
    asio::io_context& io_context_;
    tcp::socket socket_;
    asio::error_code ignorederror_;
};

inline void MessageSender::Connect(std::string port)
{
    tcp::resolver resolver(io_context_);
    auto endpoints = resolver.resolve("127.0.0.1", port);
    std::error_code error;
    asio::connect(
        socket_, 
        endpoints, 
        error);
    if (error)
    {
        // TODO: handle error
    }
}

inline void MessageSender::SendMessage(void* data, size_t bytecount)
{
    asio::async_write(
        socket_, 
        asio::buffer(data, bytecount), 
        [this](std::error_code ec, std::size_t)
        {
            if (ec)
            {
                socket_.close();
            }
        });
}