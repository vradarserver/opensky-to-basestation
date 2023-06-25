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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;
using OpenSkyToBaseStation.OpenSky;

namespace OpenSkyToBaseStation
{
    class CommandRunner_Rebroadcast : CommandRunner
    {
        private HttpClient                  _HttpClient;
        private Timer                       _CallOpenSkyTimer;
        private Timer                       _TickleAircraftListTimer;
        private string                      _RequestUrl;
        private NetworkListener             _NetworkListener;
        private AircraftList                _AircraftList = new AircraftList();
        private BaseStationMessageGenerator _MessageGenerator = new BaseStationMessageGenerator();
        private DateTime                    _AircraftListLastSentUtc;

        public override bool Run()
        {
            Console.WriteLine($"Rebroadcast OpenSky state");
            Console.WriteLine($"User:       {(Options.IsAnonymous ? "Anonymous" : Options.UserName)}");
            Console.WriteLine($"Root URL:   {Options.ObsfucatedRootUrl}");
            Console.WriteLine($"Interval:   Every {Options.IntervalSeconds} seconds");
            Console.WriteLine($"Icao24s:    {(Options.Icao24s.Count == 0 ? "all" : String.Join("-", Options.Icao24s))}");
            Console.WriteLine($"Bounds:     {(!Options.HasBoundingBox ? "World" : Options.BoundsDescription)}");
            Console.WriteLine($"Listen on:  {Options.Port}");
            Console.WriteLine($"Tickle:     Every {Options.TickleIntervalSeconds} seconds");

            if(!String.IsNullOrEmpty(Options.OpenSkyJsonFileName)) {
                Console.WriteLine($"JSON save:  {Options.OpenSkyJsonFileName}");
                CreateDiagnosticJsonSaveFolder();
            }

            StartNetworkListener();
            StartCallingOpenSkyApi();
            if(Options.TickleIntervalSeconds > 0) {
                StartTickleTimer();
            }
            SuspendThreadUntilUserWantsToQuit();
            Cleanup();

            return true;
        }

        private void StartNetworkListener()
        {
            _NetworkListener = new NetworkListener() {
                Port = Options.Port,
            };
            Task.Run(() => {
                _NetworkListener
                    .AcceptConnections()
                    .ContinueWith(NetworkListener_Error, TaskContinuationOptions.OnlyOnFaulted);
            });
        }

        private void NetworkListener_Error(Task task)
        {
            Console.WriteLine($"Network listener faulted: {task?.Exception}");
        }

        private void StartCallingOpenSkyApi()
        {
            _HttpClient = new HttpClient();
            if(!String.IsNullOrEmpty(Options.UserName) && !String.IsNullOrEmpty(Options.Password)) {
                _HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes($"{Options.UserName}:{Options.Password}")
                    )
                );
            }
            _RequestUrl = BuildRequestUrl();
            _CallOpenSkyTimer = new Timer() {
                AutoReset = false,
                Enabled = false,
                Interval = 1,
            };
            _CallOpenSkyTimer.Elapsed += CallOpenSkyTimer_Elapsed;
            _CallOpenSkyTimer.Start();
        }

        private void StartTickleTimer()
        {
            _TickleAircraftListTimer = new Timer() {
                AutoReset = false,
                Enabled = false,
                Interval = 500,
            };
            _TickleAircraftListTimer.Elapsed += TickleAircraftListTimer_Elapsed;
            _TickleAircraftListTimer.Start();
        }

        private static void SuspendThreadUntilUserWantsToQuit()
        {
            Console.WriteLine();
            Console.WriteLine("Press Q to quit");
            Console.WriteLine();
            while(Console.ReadKey(intercept: true).Key != ConsoleKey.Q) {
                ;
            }
        }

        private void Cleanup()
        {
            _CallOpenSkyTimer.Enabled = false;
            _CallOpenSkyTimer = null;
            _TickleAircraftListTimer = null;
        }

        private string BuildRequestUrl()
        {
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

            return Options.RootUrl + (
                !Options.RootUrl.EndsWith('/')
                ? $"/states/all{queryString}"
                :  $"states/all{queryString}"
            );
        }

        private void CreateDiagnosticJsonSaveFolder()
        {
            var folder = Path.GetDirectoryName(Options.OpenSkyJsonFileName);
            if(!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }
        }

        private async void CallOpenSkyTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try {
                await FetchFromOpenSky();
            } catch(Exception ex) {
                Console.WriteLine($"[{DateTime.Now}] Caught exception in fetching OpenSky: {ex.Message}");
            } finally {
                var timer = _CallOpenSkyTimer;
                if(timer != null) {
                    timer.Interval = Options.IntervalSeconds * 1000;
                    timer.Start();
                }
            }
        }

        private void TickleAircraftListTimer_Elapsed(object sender, ElapsedEventArgs e)
        {

            try {
                var lastSent = _AircraftListLastSentUtc;
                if(lastSent != default(DateTime) && lastSent.AddSeconds(Options.TickleIntervalSeconds) < DateTime.UtcNow) {
                    SendAircraftList(-1L);
                }
            } catch(Exception ex) {
                Console.WriteLine($"[{DateTime.Now}] Caught exception tickling aircraft list: {ex.Message}");
            } finally {
                var timer = _TickleAircraftListTimer;
                if(timer != null) {
                    timer.Start();
                }
            }
        }

        private async Task FetchFromOpenSky()
        {
            var response = await _HttpClient.GetAsync(_RequestUrl);
            if(!response.IsSuccessStatusCode) {
                Console.WriteLine($"[{DateTime.Now}] Warning: OpenSky fetch failed, status {(int)response.StatusCode} {response.StatusCode}");
            } else {
                var xRateLimit = response.Headers.TryGetValues("X-Rate-Limit-Remaining", out var values)
                    ? String.Join(", ", values)
                    : "";

                var jsonText = await response.Content.ReadAsStringAsync();
                if(jsonText != null) {
                    ShowOpenSkyStateFetched($"Credits remaining: {xRateLimit}");
                    SaveJsonToDiagnosticFile(jsonText);

                    var allStateVectors = JsonConvert.DeserializeObject<AllStateVectorsResponseModel>(jsonText);
                    var versionBeforeChanges = _AircraftList.Version;
                    _AircraftList.ApplyStateVectors(allStateVectors.StateVectors);
                    SendAircraftList(versionBeforeChanges);
                }
            }
        }

        private void SendAircraftList(long versionBeforeChanges)
        {
            var aircraftList = _AircraftList.GetCloneSnapshot();

            if(versionBeforeChanges == -1L) {
                versionBeforeChanges = aircraftList
                    .Select(r => r.Version)
                    .DefaultIfEmpty()
                    .Max();
            }

            _NetworkListener.SendBytes(
                _MessageGenerator.GenerateMessageBytes(aircraftList, versionBeforeChanges)
            );

            _AircraftListLastSentUtc = DateTime.UtcNow;
        }

        private void SaveJsonToDiagnosticFile(string jsonText)
        {
            if(!String.IsNullOrEmpty(Options.OpenSkyJsonFileName)) {
                try {
                    File.WriteAllText(Options.OpenSkyJsonFileName, jsonText);
                } catch(Exception ex) {
                    Console.WriteLine($"[{DateTime.Now}] Could not save JSON to {Options.OpenSkyJsonFileName}: {ex.Message}");
                }
            }
        }

        private int _IndicatorPhase;
        private int _PreviousSuffixLength;

        private void ShowOpenSkyStateFetched(string suffixText)
        {
            var ch = '\0';
            switch(_IndicatorPhase) {
                case 0: ch = '|'; break;
                case 1: ch = '/'; break;
                case 2: ch = '-'; break;
                case 3: ch = '\\'; break;
            }
            _IndicatorPhase = ++_IndicatorPhase % 4;

            suffixText ??= "";
            var clearPreviousSuffix = suffixText.Length >= _PreviousSuffixLength
                ? ""
                : new String(' ', _PreviousSuffixLength - suffixText.Length);
            _PreviousSuffixLength = suffixText.Length;

            Console.Write($"{ch} {suffixText}{clearPreviousSuffix}");
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}
