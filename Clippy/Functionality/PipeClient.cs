/// Clippy - File: "PipeClient.cs"
/// Copyright © 2018 by Tobias Zorn
/// Licensed under GNU GENERAL PUBLIC LICENSE

using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Clippy.Functionality
{
    class PipeClient
    {
        public void SendMessage(string MessageText, string PipeName, int TimeOut = 1000)
        {
            try
            {
                NamedPipeClientStream clientPipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);

                clientPipeStream.Connect(TimeOut);
                Debug.WriteLine("[ClippyPipeClient] Connection established");

                byte[] messageBuffer = Encoding.UTF8.GetBytes(MessageText);
                clientPipeStream.BeginWrite(messageBuffer, 0, messageBuffer.Length, AsyncSend, clientPipeStream);
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
