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

namespace OpenSkyToBaseStation.OpenSky
{
    /// <summary>
    /// Parses semi-typed objects out of the state vector array into fully-typed objects.
    /// </summary>
    /// <remarks>
    /// The JSON parser does not have enough information to reliably figure out the types for
    /// the values in a state vector array. This takes the objects in the state vector array
    /// and forces them into their correct type.
    /// </remarks>
    static class StateVectorConvert
    {
        public static bool? ToBool(object rawValue) => rawValue == null ? (bool?)null : (bool)Convert.ChangeType(rawValue, typeof(bool));

        public static double? ToDouble(object rawValue) => rawValue == null ? (double?)null : (double)Convert.ChangeType(rawValue, typeof(double));

        public static float? ToFloat(object rawValue) => rawValue == null ? (float?)null : (float)Convert.ChangeType(rawValue, typeof(float));

        public static int? ToInt(object rawValue) => rawValue == null ? (int?)null : (int)Convert.ChangeType(rawValue, typeof(int));

        public static long? ToLong(object rawValue) => rawValue == null ? (long?)null : (long)Convert.ChangeType(rawValue, typeof(long));

        public static string ToString(object rawValue) => rawValue == null ? null : (string)Convert.ChangeType(rawValue, typeof(string));
    }
}
