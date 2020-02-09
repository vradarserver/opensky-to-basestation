// Copyright © 2020 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OpenSkyToBaseStation
{
    /// <summary>
    /// Listens for incoming connections on a port and maintains a queue of bytes to send
    /// to each connection.
    /// </summary>
    class NetworkListener
    {
        private TcpListener _TcpListener;

        private List<ThreadSafeQueue> _SendQueues = new List<ThreadSafeQueue>();

        private object _SyncLock = new object();

        public int Port { get; set; }

        public async Task AcceptConnections()
        {
            _TcpListener = new TcpListener(IPAddress.Any, Port);
            _TcpListener.Start();

            do {
                try {
                    var socket = await _TcpListener.AcceptSocketAsync();

                    #pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => {
                        ServiceConnection(socket)
                        .ContinueWith(ServiceConnectionFailed, TaskContinuationOptions.OnlyOnFaulted);
                    });
                    #pragma warning restore CS4014
                } catch(Exception ex) {
                    Console.WriteLine($"Caught exception waiting for TCP clients to connect: {ex}");
                }
            } while(true);
        }

        public void SendBytes(byte[] bytes)
        {
            lock(_SyncLock) {
                foreach(var queue in _SendQueues) {
                    queue.Enqueue(bytes);
                }
            }
        }

        private async Task ServiceConnection(Socket socket)
        {
            var sendQueue = new ThreadSafeQueue();
            lock(_SyncLock) {
                _SendQueues.Add(sendQueue);
            }

            try {
                Console.WriteLine($"Client connected:    {socket.RemoteEndPoint}");
                while(socket.Connected) {
                    var sendBytes = sendQueue.Dequeue();
                    if(sendBytes == null || sendBytes.Length == 0) {
                        Thread.Sleep(1);
                    } else {
                        await socket.SendAsync(sendBytes, SocketFlags.None);
                    }
                }
            } catch(IOException) {
            } catch(SocketException) {
            } finally {
                lock(_SyncLock) {
                    _SendQueues.Remove(sendQueue);
                }
                try {
                    Console.WriteLine($"Client disconnected: {socket.RemoteEndPoint}");
                } catch {
                }
                try {
                    socket.Close();
                } catch {
                }
                try {
                    socket.Dispose();
                } catch {
                }
            }
        }

        private void ServiceConnectionFailed(Task task)
        {
            if(task.Exception != null) {
                Console.WriteLine($"Caught exception during processing of service connection: {task.Exception}");
            }
        }
    }
}
