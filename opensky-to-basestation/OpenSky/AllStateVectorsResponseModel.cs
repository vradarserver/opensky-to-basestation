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
using System.Runtime.Serialization;

namespace OpenSkyToBaseStation.OpenSky
{
    [DataContract]
    class AllStateVectorsResponseModel
    {
        [DataMember(Name = "time")]
        public double TimeAsSecondsSinceUnixEpoch { get; set; }

        public DateTime Time => Moments.UnixEpoch.AddSeconds(TimeAsSecondsSinceUnixEpoch);

        [DataMember(Name = "states")]
        public object[][] StateVectorValueArrays { get; set; }

        private List<StateVector> _StateVectors;
        public IList<StateVector> StateVectors
        {
            get {
                if(_StateVectors == null) {
                    _StateVectors = new List<StateVector>();
                    for(var rowIdx = 0;rowIdx < StateVectorValueArrays.Length;++rowIdx) {
                        _StateVectors.Add(
                            ConvertStateVectorValueArray(
                                StateVectorValueArrays[rowIdx]
                            )
                        );
                    }
                }
                return _StateVectors;
            }
        }

        private const int IdxIcao24 =           0;
        private const int IdxCallsign =         1;
        private const int IdxOriginCountry =    2;
        private const int IdxTimePosition =     3;
        private const int IdxLastContact =      4;
        private const int IdxLongitude =        5;
        private const int IdxLatitude =         6;
        private const int IdxBaroAltitude =     7;
        private const int IdxOnGround =         8;
        private const int IdxVelocity =         9;
        private const int IdxTrueTrack =        10;
        private const int IdxVerticalRate =     11;
        private const int IdxSensors =          12;
        private const int IdxGeoAltitude =      13;
        private const int IdxSquawk =           14;
        private const int IdxSpi =              15;
        private const int IdxPositionSource =   16;
        private const int CountFields =         17;

        private StateVector ConvertStateVectorValueArray(object[] values)
        {
            if(values.Length < CountFields) {
                throw new InvalidOperationException($"Expected an array of at least {CountFields} values, saw {values.Length}");
            }
            return new StateVector() {
                Icao24 =                            StateVectorConvert.ToString(values[IdxIcao24]).Trim().ToUpper(),
                Callsign =                          StateVectorConvert.ToString(values[IdxCallsign])?.Trim().ToUpper(),
                OriginCountry =                     StateVectorConvert.ToString(values[IdxOriginCountry]),
                UnixEpochSecondsOfLastPosition =    StateVectorConvert.ToLong(values[IdxTimePosition]),
                UnixEpochSecondsOfLastMessage =     StateVectorConvert.ToLong(values[IdxLastContact]).Value,
                Latitude =                          StateVectorConvert.ToDouble(values[IdxLatitude]),
                Longitude =                         StateVectorConvert.ToDouble(values[IdxLongitude]),
                BarometricAltitudeMetres =          StateVectorConvert.ToFloat(values[IdxBaroAltitude]),
                OnGround =                          StateVectorConvert.ToBool(values[IdxOnGround]).Value,
                GroundSpeedMetresPerSecond =        StateVectorConvert.ToFloat(values[IdxVelocity]),
                Track =                             StateVectorConvert.ToFloat(values[IdxTrueTrack]),
                VerticalRateMetresPerSecond =       StateVectorConvert.ToFloat(values[IdxVerticalRate]),
                // No sensor parsing. If someone wants this then feel free to submit a patch :)
                GeometricAltitudeMetres =           StateVectorConvert.ToFloat(values[IdxGeoAltitude]),
                Squawk =                            StateVectorConvert.ToString(values[IdxSquawk]),
                SpecialPurposeIndicator =           StateVectorConvert.ToBool(values[IdxSpi]).Value,
                PositionSource =                    (PositionSource)StateVectorConvert.ToInt(values[IdxPositionSource]),
            };
        }
    }
}
