namespace Winook
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class MessageReceiver : IDisposable
    {
        #region Fields

        private int _messageByteSize;
        private TcpListener _tcpListener;
        private CancellationTokenSource _cancellationTokenSource;
        private CancellationToken _cancellationToken;
        private Task _listeningTask;
        private ManualResetEvent _portSetEvent;

        private bool _disposed = false;

        #endregion

        #region Constructors

        public MessageReceiver(int messageByteSize)
        {
            _messageByteSize = messageByteSize;
        }

        #endregion

        #region Events

        public event EventHandler<MessageEventArgs> MessageReceived;

        #endregion

        #region Properties

        public int Port { get; private set; }

        public bool IsListening => _listeningTask != null && !_listeningTask.IsCompleted;

        #endregion

        #region Methods

        public void StartListening()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
            _portSetEvent = new ManualResetEvent(false);
            _listeningTask = Task.Run(() =>
            {
                _tcpListener = new TcpListener(IPAddress.Loopback, 0);
                _tcpListener.Start(1);
                Port = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
                _portSetEvent.Set();

                try
                {
                    var bytes = new byte[_messageByteSize];
                    using (var client = _tcpListener.AcceptTcpClient())
                    {
                        using (var stream = client.GetStream())
                        {
                            int bytecount, offset = 0;
                            while ((bytecount = stream.Read(bytes, offset, bytes.Length - offset)) != 0)
                            {
                                if (bytecount + offset == _messageByteSize)
                                {
                                    offset = 0;
                                    MessageReceived(this, new MessageEventArgs
                                    {
                                        Bytes = bytes
                                    });
                                }
                                else
                                {
                                    offset = bytecount;
                                }

                                if (_cancellationToken.IsCancellationRequested)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                catch (System.IO.IOException exception)
                {
                    if (exception.InnerException is SocketException socketException)
                    {
                        // Connection gets reset when the hooked client terminates
                        if (socketException.SocketErrorCode != SocketError.ConnectionReset)
                        {
                            throw;
                        }
                    }
                }
                catch (SocketException exception)
                {
                    switch (exception.ErrorCode)
                    {
                        case 10004: // WSACancelBlockingCall
                            break;
                        default:
                            throw;
                    }
                }
            });
            _portSetEvent.WaitOne();
        }

        public void Stop()
        {
            if (_tcpListener != null)
            {
                _cancellationTokenSource.Cancel();
                _tcpListener.Stop();
                _portSetEvent.Reset();
            }

            Port = 0;
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
                    _portSetEvent?.Dispose();
                    _cancellationTokenSource?.Dispose();
                }

                _disposed = true;
            }
        }

        #endregion
    }
}
