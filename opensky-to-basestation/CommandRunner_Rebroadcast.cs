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
using System.Net.Http;
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
        private string                      _RequestUrl;
        private NetworkListener             _NetworkListener;
        private AircraftList                _AircraftList = new AircraftList();
        private BaseStationMessageGenerator _MessageGenerator = new BaseStationMessageGenerator();

        public override bool Run()
        {
            Console.WriteLine($"Rebroadcast OpenSky state");
            Console.WriteLine($"User:       {(Options.IsAnonymous ? "Anonymous" : Options.UserName)}");
            Console.WriteLine($"Root URL:   {Options.ObsfucatedRootUrl}");
            Console.WriteLine($"Interval:   {Options.IntervalSeconds} seconds");
            Console.WriteLine($"Icao24s:    {(Options.Icao24s.Count == 0 ? "all" : String.Join("-", Options.Icao24s))}");
            Console.WriteLine($"Bounds:     {(!Options.HasBoundingBox ? "entire earth" : Options.BoundsDescription)}");
            Console.WriteLine($"Listen on:  {Options.Port}");

            if(!String.IsNullOrEmpty(Options.OpenSkyJsonFileName)) {
                Console.WriteLine($"JSON save:  {Options.OpenSkyJsonFileName}");
                CreateDiagnosticJsonSaveFolder();
            }

            StartNetworkListener();
            StartCallingOpenSkyApi();
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
            _RequestUrl = BuildRequestUrl();
            _CallOpenSkyTimer = new Timer() {
                AutoReset = false,
                Enabled = false,
                Interval = 1,
            };
            _CallOpenSkyTimer.Elapsed += CallOpenSkyTimer_Elapsed;
            _CallOpenSkyTimer.Start();
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
                Console.WriteLine($"Caught exception in fetching OpenSky: {ex}");
            } finally {
                var timer = _CallOpenSkyTimer;
                if(timer != null) {
                    timer.Interval = Options.IntervalSeconds * 1000;
                    timer.Start();
                }
            }
        }

        private async Task FetchFromOpenSky()
        {
            var response = await _HttpClient.GetAsync(_RequestUrl);
            if(!response.IsSuccessStatusCode) {
                Console.WriteLine($"Warning: OpenSky fetch failed, status {response.StatusCode}");
            } else {
                var jsonText = await response.Content.ReadAsStringAsync();
                if(jsonText != null) {
                    ShowOpenSkyStateFetched();
                    SaveJsonToDiagnosticFile(jsonText);

                    var allStateVectors = JsonConvert.DeserializeObject<AllStateVectorsResponseModel>(jsonText);
                    var versionBeforeChanges = _AircraftList.Version;
                    _AircraftList.ApplyStateVectors(allStateVectors.StateVectors);
                    var aircraftList = _AircraftList.GetCloneSnapshot();
                    _NetworkListener.SendBytes(
                        _MessageGenerator.GenerateMessageBytes(aircraftList, versionBeforeChanges)
                    );
                }
            }
        }

        private void SaveJsonToDiagnosticFile(string jsonText)
        {
            if(!String.IsNullOrEmpty(Options.OpenSkyJsonFileName)) {
                try {
                    File.WriteAllText(Options.OpenSkyJsonFileName, jsonText);
                } catch(Exception ex) {
                    Console.WriteLine($"Could not save JSON to {Options.OpenSkyJsonFileName}: {ex.Message}");
                }
            }
        }

        private int _IndicatorPhase;

        private void ShowOpenSkyStateFetched()
        {
            var ch = '\0';
            switch(_IndicatorPhase) {
                case 0: ch = '|'; break;
                case 1: ch = '/'; break;
                case 2: ch = '-'; break;
                case 3: ch = '\\'; break;
            }
            Console.Write(ch);
            Console.CursorLeft--;
            _IndicatorPhase = ++_IndicatorPhase % 4;
        }
    }
}
