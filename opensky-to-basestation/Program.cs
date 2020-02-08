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
using System.Reflection;

namespace OpenSkyToBaseStation
{
    class Program
    {
        static void Main(string[] args)
        {
            var success = false;

            try {
                var version = Assembly.GetExecutingAssembly().GetName().Version;
                Console.WriteLine($"opensky-to-basestation v{version.Major}.{version.Minor}.{version.Build}");
                var options = OptionsParser.Parse(args);

                CommandRunner commandRunner = null;
                switch(options.Command) {
                    case Command.Rebroadcast:   commandRunner = new CommandRunner_Rebroadcast(); break;
                    case Command.None:          break;
                }

                if(commandRunner == null) {
                    OptionsParser.Usage("Missing command");
                } else {
                    commandRunner.Options = options;
                    success = commandRunner.Run();
                }
            } catch(Exception ex) {
                Console.WriteLine($"Caught unhandled exception: {ex}");
                Environment.Exit(2);
            }

            Environment.Exit(success ? 0 : 1);
        }
    }
}
