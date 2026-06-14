namespace Winook
{
    using System;
    using System.IO;
    using System.IO.Pipes;
    using System.Threading;
    using System.Threading.Tasks;

    internal class MessageReceiver : IDisposable
    {
        #region Fields

        private readonly int _messageByteSize;
        private readonly string _pipeName;

        private NamedPipeServerStream _pipeServerStream;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _listeningTask;

        private bool _disposed;

        #endregion

        #region Constructors

        public MessageReceiver(int messageByteSize, string pipeName)
        {
            _messageByteSize = messageByteSize;
            _pipeName = pipeName;
        }

        #endregion

        #region Events

        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Properties

        public string PipeName => _pipeName;

        public bool IsListening => _listeningTask != null && !_listeningTask.IsCompleted;

        #endregion

        #region Methods

        public void StartListening()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _pipeServerStream = new NamedPipeServerStream(
                _pipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous);

            _listeningTask = Task.Run(() =>
            {
                try
                {
                    _pipeServerStream.WaitForConnection();

                    var bytes = new byte[_messageByteSize];
                    int bytecount, offset = 0;
                    while ((bytecount = _pipeServerStream.Read(bytes, offset, bytes.Length - offset)) != 0)
                    {
                        offset += bytecount;
                        if (offset == _messageByteSize)
                        {
                            offset = 0;
                            var messageBytes = new byte[_messageByteSize];
                            Buffer.BlockCopy(bytes, 0, messageBytes, 0, _messageByteSize);
                            MessageReceived(this, new MessageEventArgs { Bytes = messageBytes });
                        }

                        if (_cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                    }
                }
                catch (IOException)
                {
                    // The pipe is closed when the hook is uninstalled or the hooked process exits.
                }
                catch (ObjectDisposedException)
                {
                    // Stop() closes the pipe to unblock WaitForConnection() or Read().
                }
            });
        }

        public void Stop()
        {
            if (_pipeServerStream != null)
            {
                _cancellationTokenSource.Cancel();
                _pipeServerStream.Dispose();
                _pipeServerStream = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (IsListening)
                    {
                        Stop();
                        _listeningTask.Wait();
                    }

                    _listeningTask?.Dispose();
                    _cancellationTokenSource?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
