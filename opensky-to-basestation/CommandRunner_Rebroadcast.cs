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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OpenSkyToBaseStation
{
    class CommandRunner_Rebroadcast : CommandRunner
    {
        private HttpClient _HttpClient;
        private Timer      _Timer;
        private string     _RequestUrl;

        public override bool Run()
        {
            _HttpClient = new HttpClient();

            var queryString = new StringBuilder();
            foreach(var icao24 in Options.Icao24s) {
                UrlHelper.AppendQueryString(queryString, "icao24", icao24);
            }
            if(Options.HasBoundingBox) {
                UrlHelper.AppendQueryString(queryString, "lamin", Options.LatitudeLow.Value.ToString(CultureInfo.InvariantCulture));
                UrlHelper.AppendQueryString(queryString, "lomin", Options.LongitudeLow.Value.ToString(CultureInfo.InvariantCulture));
                UrlHelper.AppendQueryString(queryString, "lamax", Options.LatitudeHigh.Value.ToString(CultureInfo.InvariantCulture));
                UrlHelper.AppendQueryString(queryString, "lomax", Options.LongitudeHigh.Value.ToString(CultureInfo.InvariantCulture));
            }
            _RequestUrl = $"/states/all{queryString}";

            Console.WriteLine($"Rebroadcast OpenSky state");
            Console.WriteLine($"User:       {(Options.IsAnonymous ? "Anonymous" : Options.UserName)}");
            Console.WriteLine($"Root URL:   {Options.ObsfucatedRootUrl}");
            Console.WriteLine($"Endpoint:   {_RequestUrl}");
            Console.WriteLine($"Interval:   {Options.IntervalSeconds} seconds");
            Console.WriteLine($"Icao24s:    {(Options.Icao24s.Count == 0 ? "all" : String.Join("-", Options.Icao24s))}");
            Console.WriteLine($"Bounds:     {(!Options.HasBoundingBox ? "entire earth" : Options.BoundsDescription)}");

            _Timer = new Timer() {
                AutoReset = false,
                Enabled =   false,
                Interval =  1,
            };
            _Timer.Elapsed += Timer_Elapsed;
            _Timer.Start();

            Console.WriteLine();
            Console.WriteLine("Press Q to quit:");
            while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                ;
            }

            _Timer.Enabled = false;
            _Timer = null;

            return true;
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                await FetchFromOpenSky();
            } catch(Exception ex) {
                Console.WriteLine($"Caught exception in fetching OpenSky: {ex}");
            } finally {
                var timer = _Timer;
                if(timer != null) {
                    timer.Interval = Options.IntervalSeconds * 1000;
                    timer.Start();
                }
            }
        }

        private int _IndicatorPhase;

        private async Task FetchFromOpenSky()
        {
            var response = await _HttpClient.GetAsync(Options.RootUrl + _RequestUrl);
            if(!response.IsSuccessStatusCode) {
                Console.WriteLine($"Warning: OpenSky fetch failed, status {response.StatusCode}");
            } else {
                var jsonText = await response.Content.ReadAsStringAsync();
                if(jsonText != null) {
                    var ch = '\0';
                    switch(_IndicatorPhase) {
                        case 0:
                        case 4: ch = '|'; break;
                        case 1:
                        case 5: ch = '/'; break;
                        case 2:
                        case 6: ch = '-'; break;
                        case 3:
                        case 7: ch = '\\'; break;
                    }
                    Console.Write(ch);
                    Console.CursorLeft--;
                    _IndicatorPhase = ++_IndicatorPhase % 8;
                }
            }
        }
    }
}
