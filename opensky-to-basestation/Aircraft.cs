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
using OpenSkyToBaseStation.OpenSky;

namespace OpenSkyToBaseStation
{
    class Aircraft
    {
        private object _SyncLock = new object();

        public string Icao24 { get; }

        public DateTime LastSeenInOpenSkyUtc { get; private set; }

        public long Version { get; private set; }

        public VersionedValue<string> Callsign { get; }

        public VersionedValue<string> OriginCountry { get; }

        public VersionedValue<DateTime?> LastPositionTime { get; }

        public VersionedValue<DateTime?> LastMessageTime { get; }

        public VersionedValue<double?> Latitude { get; }

        public VersionedValue<double?> Longitude { get; }

        public VersionedValue<float?> AltitudeFeet { get; }

        public VersionedValue<bool?> OnGround { get; }

        public VersionedValue<float?> GroundSpeedKnots { get; }

        public VersionedValue<float?> Track { get; }

        public VersionedValue<float?> VerticalRateFeetPerSecond { get; }

        public VersionedValue<string> Squawk { get; }

        public VersionedValue<bool?> SpecialPurposeIndicator { get; }

        public VersionedValue<PositionSource> PositionSource { get; }

        public Aircraft(string icao24)
        {
            Icao24 = icao24;
            Callsign =                  new VersionedValue<string>();
            OriginCountry =             new VersionedValue<string>();
            LastPositionTime =          new VersionedValue<DateTime?>();
            LastMessageTime =           new VersionedValue<DateTime?>();
            Latitude =                  new VersionedValue<double?>();
            Longitude =                 new VersionedValue<double?>();
            AltitudeFeet =              new VersionedValue<float?>();
            OnGround =                  new VersionedValue<bool?>();
            GroundSpeedKnots =          new VersionedValue<float?>();
            Track =                     new VersionedValue<float?>();
            VerticalRateFeetPerSecond = new VersionedValue<float?>();
            Squawk =                    new VersionedValue<string>();
            SpecialPurposeIndicator =   new VersionedValue<bool?>();
            PositionSource =            new VersionedValue<PositionSource>();
        }

        public Aircraft(Aircraft other)
        {
            lock(other._SyncLock) {
                Icao24 =                    other.Icao24;
                Version =                   other.Version;
                LastSeenInOpenSkyUtc =      other.LastSeenInOpenSkyUtc;

                Callsign =                  other.Callsign.Clone();
                OriginCountry =             other.OriginCountry.Clone();
                LastPositionTime =          other.LastPositionTime.Clone();
                LastMessageTime =           other.LastMessageTime.Clone();
                Latitude =                  other.Latitude.Clone();
                Longitude =                 other.Longitude.Clone();
                AltitudeFeet =              other.AltitudeFeet.Clone();
                OnGround =                  other.OnGround.Clone();
                GroundSpeedKnots =          other.GroundSpeedKnots.Clone();
                Track =                     other.Track.Clone();
                VerticalRateFeetPerSecond = other.VerticalRateFeetPerSecond.Clone();
                Squawk =                    other.Squawk.Clone();
                SpecialPurposeIndicator =   other.SpecialPurposeIndicator.Clone();
                PositionSource =            other.PositionSource.Clone();
            }
        }

        public void ApplyStateVector(StateVector stateVector, long version)
        {
            lock(_SyncLock) {
                LastSeenInOpenSkyUtc = DateTime.UtcNow;
                Version = Math.Max(Version, Callsign.UpdateValue(stateVector.Callsign, version));
                Version = Math.Max(Version, OriginCountry.UpdateValue(stateVector.OriginCountry, version));
                Version = Math.Max(Version, LastPositionTime.UpdateValue(stateVector.TimeOfLastPosition, version));
                Version = Math.Max(Version, LastMessageTime.UpdateValue(stateVector.TimeOfLastMessage, version));
                Version = Math.Max(Version, Latitude.UpdateValue(stateVector.Latitude, version));
                Version = Math.Max(Version, Longitude.UpdateValue(stateVector.Longitude, version));
                Version = Math.Max(Version, AltitudeFeet.UpdateValue(stateVector.AltitudeFeet, version));
                Version = Math.Max(Version, OnGround.UpdateValue(stateVector.OnGround, version));
                Version = Math.Max(Version, GroundSpeedKnots.UpdateValue(stateVector.GroundSpeedKnots, version));
                Version = Math.Max(Version, Track.UpdateValue(stateVector.Track, version));
                Version = Math.Max(Version, VerticalRateFeetPerSecond.UpdateValue(stateVector.VerticalRateFeetPerSecond, version));
                Version = Math.Max(Version, Squawk.UpdateValue(stateVector.Squawk, version));
                Version = Math.Max(Version, SpecialPurposeIndicator.UpdateValue(stateVector.SpecialPurposeIndicator, version));
                Version = Math.Max(Version, PositionSource.UpdateValue(stateVector.PositionSource, version));
            }
        }
    }
}
