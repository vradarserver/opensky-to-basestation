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

namespace OpenSkyToBaseStation
{
    class Options
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsAnonymous => String.IsNullOrWhiteSpace(UserName);

        public string AnonRootUrl { get; set; } = "https://opensky-network.org/api";

        public string UserRootUrl { get; set; } = "https://{user}:{password}@opensky-network.org/api";

        public string RootUrl => IsAnonymous ? AnonRootUrl : UserRootUrl.Replace("{user}", UserName).Replace("{password}", Password);

        public string ObsfucatedRootUrl => IsAnonymous ? AnonRootUrl : UserRootUrl.Replace("{user}", UserName).Replace("{password}", new String('*', Password?.Length ?? 0));

        public int AnonymousIntervalSeconds { get; set; } = 10;

        public int UserIntervalSeconds { get; set; } = 5;

        public int IntervalSeconds => IsAnonymous ? AnonymousIntervalSeconds : UserIntervalSeconds;

        public Command Command { get; set; }

        public List<string> Icao24s { get; } = new List<string>();

        public double? LatitudeHigh { get; set; }

        public double? LatitudeLow { get; set; }

        public double? LongitudeHigh { get; set; }

        public double? LongitudeLow { get; set; }

        public bool HasBoundingBox => LatitudeHigh != null && LatitudeLow != null && LongitudeHigh != null && LongitudeLow != null;

        public string BoundsDescription => !HasBoundingBox ? "" : $"lamin {LatitudeLow} lomin {LongitudeLow} lamax {LatitudeHigh} lomax {LongitudeHigh}";
    }
}
