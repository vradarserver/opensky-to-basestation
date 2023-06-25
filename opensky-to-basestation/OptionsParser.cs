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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSkyToBaseStation
{
    static class OptionsParser
    {
        /// <summary>
        /// Parses the command-line options passed across.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Options Parse(string[] args)
        {
            var result = new Options();

            if(args.Length == 0) {
                Usage(null);
            }

            for(var i = 0;i < args.Length;++i) {
                var arg = (args[i] ?? "");
                var normalisedArg = arg.ToLower();
                var nextArg = i + 1 < args.Length ? args[i + 1] : null;

                switch(normalisedArg) {
                    case "/?":
                    case "-help":
                    case "-?":
                    case "--help":
                    case "--?":
                        Usage(null);
                        break;
                    case "-icao24":
                        result
                            .Icao24s.AddRange(
                                UseNextArg(arg, nextArg, ref i)
                                .Split('-', ',', ';')
                                .Select(r => r.Trim())
                                .Where(r => !String.IsNullOrEmpty(r))
                            );
                        break;
                    case "-lamax":
                        result.LatitudeHigh = ParseDouble(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-lamin":
                        result.LatitudeLow = ParseDouble(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-lomax":
                        result.LongitudeHigh = ParseDouble(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-lomin":
                        result.LongitudeLow = ParseDouble(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-jsonfilename":
                        result.OpenSkyJsonFileName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-password":
                        result.Password = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-port":
                        result.Port = ParseInt(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-rebroadcast":
                        result.Command = ParseCommand(result, Command.Rebroadcast);
                        break;
                    case "-rooturl":
                        result.RootUrl = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-ticklesecs":
                        result.TickleIntervalSeconds = ParseInt(UseNextArg(arg, nextArg, ref i));
                        break;
                    case "-user":
                        result.UserName = UseNextArg(arg, nextArg, ref i);
                        break;
                    case "-userinterval":
                        result.UserIntervalSeconds = ParseInt(UseNextArg(arg, nextArg, ref i));
                        break;
                    default:
                        Usage($"Invalid argument {arg}");
                        break;
                }
            }

            return result;
        }

        private static Command ParseCommand(Options options, Command command)
        {
            if(options.Command != Command.None) {
                Usage($"Cannot specify both {options.Command} and {command} commands");
            }

            return command;
        }

        private static string UseNextArg(string arg, string nextArg, ref int argIndex)
        {
            if(String.IsNullOrWhiteSpace(nextArg)) {
                Usage($"{arg} argument missing");
            }
            ++argIndex;

            return nextArg;
        }

        private static T ParseEnum<T>(string arg)
        {
            try {
                return (T)Enum.Parse(typeof(T), arg ?? "", ignoreCase: true);
            } catch(ArgumentException) {
                Usage($"{arg} is not a recognised {typeof(T).Name} value");
                throw;
            }
        }

        private static int ParseInt(string arg)
        {
            if(!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)) {
                Usage($"{arg} is not an integer");
            }

            return result;
        }

        private static double ParseDouble(string arg)
        {
            if(!double.TryParse(arg, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)) {
                Usage($"{arg} is not a floating point value (remember to use . for the decimal point)");
            }

            return result;
        }

        public static void Usage(string message)
        {
            var defaults = new Options();

            Console.WriteLine($"usage: <command> [options]");
            Console.WriteLine($"COMMANDS");
            Console.WriteLine($"  -rebroadcast  Periodically fetch OpenSky state and rebroadcast locally");
            Console.WriteLine();
            Console.WriteLine($"OPENSKY PARAMETERS");
            Console.WriteLine($"  -user         <text>     OpenSky network username [{defaults.UserName}]");
            Console.WriteLine($"  -password     <text>     OpenSky network password [{defaults.Password}]");
            Console.WriteLine($"  -rootUrl      <url>      Root URL for OpenSky API calls [{defaults.RootUrl}]");
            Console.WriteLine($"  -anonInterval <secs>     Seconds between fetches for anonymous users [{defaults.AnonIntervalSeconds}]");
            Console.WriteLine($"  -userInterval <secs>     Seconds between fetches for logged-in users [{defaults.UserIntervalSeconds}]");
            Console.WriteLine($"  -icao24       <hex-list> Hyphen-separated ICAOs to fetch from OpenSky [{String.Join("-", defaults.Icao24s)}]");
            Console.WriteLine($"  -lamin        <float>    Lower bound for latitude [{defaults.LatitudeHigh}]");
            Console.WriteLine($"  -lamax        <float>    Upper bound for latitude [{defaults.LatitudeLow}]");
            Console.WriteLine($"  -lomin        <float>    Lower bound for longitude [{defaults.LongitudeHigh}]");
            Console.WriteLine($"  -lomax        <float>    Upper bound for longitude [{defaults.LongitudeLow}]");
            Console.WriteLine();
            Console.WriteLine($"REBROADCAST SERVER");
            Console.WriteLine($"  -port         <1-65535>  The port to listen to for incoming connections [{defaults.Port}]");
            Console.WriteLine($"  -tickleSecs   <secs>     Seconds between aircraft list tickles, 0 to switch off [{defaults.TickleIntervalSeconds}]");
            Console.WriteLine();
            Console.WriteLine($"DIAGNOSTICS");
            Console.WriteLine($"  -jsonFileName <filename> The full path to a file that the OpenSky JSON will be saved to [{defaults.OpenSkyJsonFileName}]");

            if(!String.IsNullOrEmpty(message)) {
                Console.WriteLine();
                Console.WriteLine(message);
            }

            Environment.Exit(1);
        }
    }
}
