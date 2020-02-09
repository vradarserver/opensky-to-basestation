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
using System.Text;
using VirtualRadar.Interface.BaseStation;

namespace OpenSkyToBaseStation
{
    class BaseStationMessageGenerator
    {
        public byte[] GenerateMessageBytes(IList<Aircraft> aircraftList, long startVersionExclusive)
        {
            var result = new StringBuilder();

            var now = DateTime.Now;
            foreach(var aircraft in aircraftList) {
                var baseStationMessage = CreateBaseStationMessage(aircraft, startVersionExclusive, now);
                result.AppendLine(baseStationMessage.ToBaseStationString());
            }

            return Encoding.ASCII.GetBytes(
                result.ToString()
            );
        }

        private BaseStationMessage CreateBaseStationMessage(Aircraft aircraft, long startVersionExclusive, DateTime now)
        {
            var result = new BaseStationMessage() {
                Icao24 =            aircraft.Icao24,
                MessageGenerated =  now,
                MessageLogged =     now,
                MessageType =       BaseStationMessageType.Transmission,
                TransmissionType =  BaseStationTransmissionType.SurfacePosition,
                StatusCode =        BaseStationStatusCode.OK,
            };

            bool versionQualifies<T>(VersionedValue<T> value) => value.Version > startVersionExclusive && value.Value != null;

            if(versionQualifies(aircraft.AltitudeFeet))                 result.Altitude = (int)aircraft.AltitudeFeet;
            if(versionQualifies(aircraft.Callsign))                     result.Callsign = aircraft.Callsign;
            if(versionQualifies(aircraft.GroundSpeedKnots))             result.GroundSpeed = aircraft.GroundSpeedKnots;
            if(versionQualifies(aircraft.OnGround))                     result.OnGround = aircraft.OnGround;
            if(versionQualifies(aircraft.SpecialPurposeIndicator))      result.IdentActive = aircraft.SpecialPurposeIndicator;
            if(versionQualifies(aircraft.Squawk))                       result.Squawk = ConvertSquawk(aircraft.Squawk);
            if(versionQualifies(aircraft.Track))                        result.Track = aircraft.Track;
            if(versionQualifies(aircraft.VerticalRateFeetPerSecond))    result.VerticalRate = (int)aircraft.VerticalRateFeetPerSecond;

            if(versionQualifies(aircraft.Latitude) || versionQualifies(aircraft.Longitude)) {
                result.Latitude = aircraft.Latitude;
                result.Longitude = aircraft.Longitude;
            }

            return result;
        }

        private int? ConvertSquawk(string squawk)
        {
            int? result = null;

            if(!String.IsNullOrEmpty(squawk) && squawk.Length <= 4) {
                if(int.TryParse(squawk, NumberStyles.None, CultureInfo.InvariantCulture, out var squawkInt)) {
                    result = squawkInt;
                }
            }

            return result;
        }
    }
}
