/// Clippy - File: "AsyncPipeServer.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Clippy.Functionality
{
    // Delegate for passing received message back to caller
    public delegate void ServerMessage(string content);

    /// <summary>
    /// Named pipe server for communication between multiple instances
    /// </summary>
    class AsyncPipeServer
    {
        public event ServerMessage MessageRecieved;
        string m_pipeName;
        NamedPipeServerStream m_clippyServerStream;
        bool m_shutdownRequested;

        public void Listen(string PipeName)
        {
            m_shutdownRequested = false;

            try
            {
                m_pipeName = PipeName;
                m_clippyServerStream = new NamedPipeServerStream(
                    PipeName, 
                    PipeDirection.In, 
                    1, 
                    PipeTransmissionMode.Byte, 
                    PipeOptions.Asynchronous);

                // Wait for a connection
                m_clippyServerStream.BeginWaitForConnection(
                    new AsyncCallback(WaitForConnectionCallBack), m_clippyServerStream);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public void Shutdown()
        {
            m_shutdownRequested = true;
            if (m_clippyServerStream == null) return;

            if (m_clippyServerStream.IsConnected)
            {
                m_clippyServerStream.Disconnect();
            }

            m_clippyServerStream.Close();
            m_clippyServerStream.Dispose();
        }

        private void WaitForConnectionCallBack(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)asyncResult.AsyncState;
                pipeServer.EndWaitForConnection(asyncResult);
                byte[] messageBuffer = new byte[2048];

                // Read the incoming message
                pipeServer.Read(messageBuffer, 0, 2048);
                string stringData = Encoding.UTF8.GetString(messageBuffer, 0, messageBuffer.Length).Trim('\0');
                MessageRecieved.Invoke(stringData);

                // Close original sever and create new instance
                pipeServer.Close();
                pipeServer = null;

                if (m_shutdownRequested)
                {
                    return;
                }

                pipeServer = new NamedPipeServerStream(
                    m_pipeName, 
                    PipeDirection.In, 
                    1, 
                    PipeTransmissionMode.Byte, 
                    PipeOptions.Asynchronous);

                // Recursively wait for the next connection
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch
            {
                return;
            }
        }
    }
}
