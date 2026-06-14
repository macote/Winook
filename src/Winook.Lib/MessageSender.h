#pragma once

#include <Windows.h>

#include <array>
#include <chrono>
#include <condition_variable>
#include <cstddef>
#include <deque>
#include <mutex>
#include <string>
#include <thread>

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
    MessageSender() = default;
    ~MessageSender();

    MessageSender(const MessageSender&) = delete;
    MessageSender& operator=(const MessageSender&) = delete;

    void Connect(const std::wstring& pipeName);
    void Stop(bool waitForWorker = true);
    void SendMessage(const void* data, size_t bytecount, bool droppable = false);

private:
    static constexpr size_t kMaxMessageSize = 40;
    static constexpr size_t kMaxQueuedMessages = 256;
    static constexpr int kWorkerIdleTimeoutInMilliseconds = 1000;

    struct QueuedMessage
    {
        std::array<BYTE, kMaxMessageSize> bytes{};
        size_t bytecount{};
        bool droppable{};
    };

    static void WorkerThreadEntry(MessageSender* sender, HMODULE module);
    void WorkerLoop();
    bool TryDropQueuedDroppableMessage();

    HANDLE pipe_{ INVALID_HANDLE_VALUE };
    std::thread worker_;
    std::mutex mutex_;
    std::condition_variable hasMessage_;
    std::deque<QueuedMessage> queue_;
    bool stopping_{ false };
    bool connected_{ false };
    bool workerStarted_{ false };
};

inline MessageSender::~MessageSender()
{
    Stop(false);
}

inline void MessageSender::Connect(const std::wstring& pipeName)
{
    Stop();

    const auto pipePath = std::wstring(TEXT("\\\\.\\pipe\\")) + pipeName;
    pipe_ = CreateFile(
        pipePath.c_str(),
        GENERIC_WRITE,
        0,
        NULL,
        OPEN_EXISTING,
        FILE_ATTRIBUTE_NORMAL,
        NULL);

    if (pipe_ == INVALID_HANDLE_VALUE)
    {
        return;
    }

    {
        std::lock_guard<std::mutex> lock(mutex_);
        stopping_ = false;
        connected_ = true;
        workerStarted_ = false;
    }
}

inline void MessageSender::Stop(bool waitForWorker)
{
    bool shouldClosePipe = false;
    {
        std::lock_guard<std::mutex> lock(mutex_);
        stopping_ = true;
        connected_ = false;
        queue_.clear();
        shouldClosePipe = !workerStarted_;
    }

    hasMessage_.notify_one();

    if (worker_.joinable())
    {
        CancelSynchronousIo(reinterpret_cast<HANDLE>(worker_.native_handle()));
        if (waitForWorker)
        {
            worker_.join();
        }
        else
        {
            worker_.detach();
        }
    }

    if (shouldClosePipe && pipe_ != INVALID_HANDLE_VALUE)
    {
        CloseHandle(pipe_);
        pipe_ = INVALID_HANDLE_VALUE;
    }
}

inline void MessageSender::SendMessage(const void* data, size_t bytecount, bool droppable)
{
    if (data == nullptr || bytecount == 0 || bytecount > kMaxMessageSize)
    {
        return;
    }

    {
        std::lock_guard<std::mutex> lock(mutex_);
        if (!connected_ || stopping_)
        {
            return;
        }

        if (!workerStarted_)
        {
            if (worker_.joinable())
            {
                worker_.join();
            }

            HMODULE module{};
            GetModuleHandleEx(
                GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS,
                reinterpret_cast<LPCTSTR>(&MessageSender::WorkerThreadEntry),
                &module);

            worker_ = std::thread(&MessageSender::WorkerThreadEntry, this, module);
            workerStarted_ = true;
        }

        if (queue_.size() >= kMaxQueuedMessages)
        {
            if (droppable || !TryDropQueuedDroppableMessage())
            {
                return;
            }
        }

        QueuedMessage message;
        CopyMemory(message.bytes.data(), data, bytecount);
        message.bytecount = bytecount;
        message.droppable = droppable;
        queue_.push_back(message);
    }

    hasMessage_.notify_one();
}

inline bool MessageSender::TryDropQueuedDroppableMessage()
{
    for (auto iter = queue_.begin(); iter != queue_.end(); ++iter)
    {
        if (iter->droppable)
        {
            queue_.erase(iter);
            return true;
        }
    }

    return false;
}

inline void MessageSender::WorkerThreadEntry(MessageSender* sender, HMODULE module)
{
    sender->WorkerLoop();

    if (module != NULL)
    {
        FreeLibraryAndExitThread(module, 0);
    }
}

inline void MessageSender::WorkerLoop()
{
    HANDLE pipe = INVALID_HANDLE_VALUE;
    {
        std::lock_guard<std::mutex> lock(mutex_);
        pipe = pipe_;
        pipe_ = INVALID_HANDLE_VALUE;
    }

    while (true)
    {
        QueuedMessage message;
        {
            std::unique_lock<std::mutex> lock(mutex_);
            const auto hasWork = hasMessage_.wait_for(
                lock,
                std::chrono::milliseconds(kWorkerIdleTimeoutInMilliseconds),
                [this]
            {
                return stopping_ || !queue_.empty();
            });

            if (!hasWork)
            {
                workerStarted_ = false;
                pipe_ = pipe;
                return;
            }

            if (queue_.empty())
            {
                if (stopping_)
                {
                    workerStarted_ = false;
                    queue_.clear();
                    if (pipe != INVALID_HANDLE_VALUE)
                    {
                        CloseHandle(pipe);
                    }
                    return;
                }

                continue;
            }

            message = queue_.front();
            queue_.pop_front();
        }

        DWORD bytesWritten{};
        if (!WriteFile(pipe, message.bytes.data(), static_cast<DWORD>(message.bytecount), &bytesWritten, NULL)
            || bytesWritten != message.bytecount)
        {
            std::lock_guard<std::mutex> lock(mutex_);
            connected_ = false;
            stopping_ = true;
            workerStarted_ = false;
            queue_.clear();
            if (pipe != INVALID_HANDLE_VALUE)
            {
                CloseHandle(pipe);
            }
            return;
        }
    }
}
