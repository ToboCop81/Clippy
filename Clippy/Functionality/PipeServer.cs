/// Clippy - File: "PipeServer.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Clippy.Functionality
{
    // Delegate for passing received message back to caller
    public delegate void DelegateMessage(string Reply);

    class PipeServer
    {
        public event DelegateMessage MessageRecieved;
        string m_pipeName;

        public bool ShutdownRequested { get; set; }

        public PipeServer()
        {
            ShutdownRequested = false;
        }

        public void Listen(string PipeName)
        {
            try
            {
                m_pipeName = PipeName;

                NamedPipeServerStream pipeServer = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void WaitForConnectionCallBack(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)asyncResult.AsyncState;
                pipeServer.EndWaitForConnection(asyncResult);

                byte[] messageBuffer = new byte[1024];
                pipeServer.Read(messageBuffer, 0, 1024);
                string message = Encoding.UTF8.GetString(messageBuffer, 0, messageBuffer.Length);
                Debug.WriteLine(message + Environment.NewLine);

                MessageRecieved.Invoke(message);

                pipeServer.Close();
                pipeServer = null;
                pipeServer = new NamedPipeServerStream(m_pipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

                if (ShutdownRequested) return;

                // Recursively wait for the connection again
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch
            {
                return;
            }
        }
    }
}
