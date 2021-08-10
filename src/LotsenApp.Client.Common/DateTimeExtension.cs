// Copyright (c) 2021 OFFIS e.V.. All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
// 1. Redistributions of source code must retain the above copyright notice, this
//    list of conditions and the following disclaimer.
//    
// 2. Redistributions in binary form must reproduce the above copyright notice,
//    this list of conditions and the following disclaimer in the documentation
//    and/or other materials provided with the distribution.
//    
// 3. Neither the name of the copyright holder nor the names of its contributors
//    may be used to endorse or promote products derived from this software without
//    specific prior written permission.
//    
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System.Globalization;

namespace System
{
    public static class DateTimeExtension
    {
        public static TimeSpan? ToTimeSpan(this DateTime dt)
        {
            return dt - dt.Date;
        }

        public static DateTime? ToDateTime(this TimeSpan ts)
        {
            DateTime? fResult = null;

            var year = string.Format("{0:0000}", DateTime.MinValue.Date.Year);
            var month = string.Format("{0:00}", DateTime.MinValue.Date.Month);
            var day = string.Format("{0:00}", DateTime.MinValue.Date.Day);

            var hours = string.Format("{0:00}", ts.Hours);
            var minutes = string.Format("{0:00}", ts.Minutes);
            var seconds = string.Format("{0:00}", ts.Seconds);

            var dSep = "-";
            var tSep = ":";
            var dtSep = "T";

            // yyyy-mm-ddTHH:mm:ss
            var dtStr = string.Concat(year, dSep, month, dSep, day, dtSep, hours, tSep, minutes, tSep, seconds);

            if (DateTime.TryParseExact(dtStr, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out var dt))
            {
                fResult = dt;
            }

            return fResult;
        }

        public static DateTimeOffset? ToDateTimeOffset(this DateTime dt)
        {
            return new DateTimeOffset(dt);
        }

        public static DateTime? ToDateTime(this DateTimeOffset dto)
        {
            return dto.DateTime;
        }
    }
}