/// Clippy - File: "AsyncPlipeClient.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using Clippy.Common;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Clippy.Functionality
{
    /// <summary>
    /// Named pipe client for communication between multiple instances
    /// </summary>
    class AsyncPlipeClient
    {
        public void SendMessage(string Message, string PipeName, int TimeOut = 1000)
        {
            try
            {
                NamedPipeClientStream clippyClientStream = new NamedPipeClientStream(
                    ".", 
                    PipeName, 
                    PipeDirection.Out, 
                    PipeOptions.Asynchronous);

                clippyClientStream.Connect(TimeOut);
                Debug.WriteLine("[ClippyClient] Pipe connection established");

                byte[] messageBuffer = Encoding.UTF8.GetBytes(Message);
                clippyClientStream.BeginWrite(messageBuffer, 0, messageBuffer.Length, AsyncSend, clippyClientStream);
            }
            catch (TimeoutException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void AsyncSend(IAsyncResult asyncResult)
        {
            try
            {
                NamedPipeClientStream pipeStream = (NamedPipeClientStream)asyncResult.AsyncState;
                pipeStream.EndWrite(asyncResult);
                pipeStream.Flush();
                pipeStream.Close();
                pipeStream.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
