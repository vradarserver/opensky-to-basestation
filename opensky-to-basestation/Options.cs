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
    /// <summary>
    /// Command-line options.
    /// </summary>
    class Options
    {
        /// <summary>
        /// The optional user name to send to OpenSky.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The optional password to send to OpenSky.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// True if no credentials have been supplied.
        /// </summary>
        public bool IsAnonymous => String.IsNullOrWhiteSpace(UserName);

        /// <summary>
        /// The API root URL.
        /// </summary>
        public string RootUrl { get; set; } = "https://opensky-network.org/api";

        /// <summary>
        /// The number of seconds to wait between fetches when no credentials have been supplied. Do not set this lower
        /// than the OpenSky rate limit.
        /// </summary>
        public int AnonIntervalSeconds { get; set; } = 10;

        /// <summary>
        /// The number of seconds to wait between fetches when credentials have been supplied. Do not set this lower than
        /// the OpenSky rate limit.
        /// </summary>
        public int UserIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// <see cref="UserIntervalSeconds"/> if credentials have been supplied, otherwise <see cref="AnonIntervalSeconds"/>
        /// </summary>
        public int IntervalSeconds => IsAnonymous ? AnonIntervalSeconds : UserIntervalSeconds;

        /// <summary>
        /// The command that we are to carry out.
        /// </summary>
        public Command Command { get; set; }

        /// <summary>
        /// The ICAO24s that fetches should be constrained to.
        /// </summary>
        public List<string> Icao24s { get; } = new List<string>();

        /// <summary>
        /// The upper latitude bound.
        /// </summary>
        public double? LatitudeHigh { get; set; }

        /// <summary>
        /// The lower latitude bound.
        /// </summary>
        public double? LatitudeLow { get; set; }

        /// <summary>
        /// The upper longitude bound.
        /// </summary>
        public double? LongitudeHigh { get; set; }

        /// <summary>
        /// The lower longitude bound.
        /// </summary>
        public double? LongitudeLow { get; set; }

        /// <summary>
        /// True if bounds have been supplied.
        /// </summary>
        public bool HasBoundingBox => LatitudeHigh != null && LatitudeLow != null && LongitudeHigh != null && LongitudeLow != null;

        /// <summary>
        /// An English description of the bounds.
        /// </summary>
        public string BoundsDescription => !HasBoundingBox ? "" : $"lamin {LatitudeLow} lomin {LongitudeLow} lamax {LatitudeHigh} lomax {LongitudeHigh}";

        /// <summary>
        /// The port to rebroadcast messages on.
        /// </summary>
        public int Port { get; set; } = 20003;

        /// <summary>
        /// The optional filename of a file that is overwritten on every fetch with the JSON
        /// sent from OpenSky.
        /// </summary>
        public string OpenSkyJsonFileName  { get; set; }

        /// <summary>
        /// The number of seconds between tickles.
        /// </summary>
        /// <remarks><para>
        /// By default VRS will take aircraft off display if there are no messages from them for 30 seconds. The interval
        /// between OpenSky fetches is at most 10 seconds but when OpenSky is busy they can take more than 20 seconds to
        /// reply, which means that messages from the aircraft can be spaced more than 30 seconds apart.
        /// </para><para>
        /// If tickles are switched on then the program starts a timer when it rebroadcasts an aircraft list. If a fresh
        /// aircraft list is not sent by the time the tickle interval has elapsed then it sends a no-change message for
        /// every ICAO on the list. This keeps the aircraft on display in VRS without having to increase the global
        /// display timeout, which would affect all receivers.
        /// </para><para>
        /// A zero tickle turns the feature off.
        /// </para></remarks>
        public int TickleIntervalSeconds { get; set; } = 25;
    }
}
