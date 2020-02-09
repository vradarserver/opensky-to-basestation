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
using System.Linq;
using System.Text;
using OpenSkyToBaseStation.OpenSky;

namespace OpenSkyToBaseStation
{
    class AircraftList
    {
        private Dictionary<string, Aircraft> _Icao24ToAircraftMap = new Dictionary<string, Aircraft>(StringComparer.OrdinalIgnoreCase);
        private object _SyncLock = new object();

        private long _Version;
        public long Version => _Version;

        public void ApplyStateVectors(IEnumerable<StateVector> stateVectors)
        {
            var noLongerTracked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            lock(_SyncLock) {
                ++_Version;

                foreach(var key in _Icao24ToAircraftMap.Keys) {
                    noLongerTracked.Add(key);
                }

                foreach(var stateVector in stateVectors.Where(r => !String.IsNullOrEmpty(r.Icao24))) {
                    if(noLongerTracked.Contains(stateVector.Icao24)) {
                        noLongerTracked.Remove(stateVector.Icao24);
                    }

                    if(!_Icao24ToAircraftMap.TryGetValue(stateVector.Icao24, out var aircraft)) {
                        aircraft = new Aircraft(stateVector.Icao24);
                        _Icao24ToAircraftMap.Add(stateVector.Icao24, aircraft);
                    }
                    aircraft.ApplyStateVector(stateVector, Version);
                }

                foreach(var missingIcao24 in noLongerTracked) {
                    _Icao24ToAircraftMap.Remove(missingIcao24);
                }
            }
        }

        public IList<Aircraft> GetCloneSnapshot()
        {
            var result = new List<Aircraft>();

            lock(_SyncLock) {
                foreach(var aircraft in _Icao24ToAircraftMap.Values) {
                    result.Add(
                        new Aircraft(aircraft)
                    );
                }
            }

            return result;
        }
    }
}
