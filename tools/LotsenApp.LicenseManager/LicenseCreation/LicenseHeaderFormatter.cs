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

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LotsenApp.LicenseManager.LicenseCreation
{
    public class LicenseHeaderFormatter
    {
        
        public void SetOrUpdateHeader(string file, string header, IDictionary<string, string> interpolation, ILicenseHeaderFormatter formatter)
        {
            var content = File.ReadAllText(file);
            var newContent = SetOrUpdateHeader(content, interpolation, header, formatter);
            var contentSplit = content.Split("\n");
            var newContentSplit = newContent.Split("\n");
            var traversedLength = 0;
            for (var i = 0; i < contentSplit.Length; ++i)
            {
                traversedLength = i;
                if (contentSplit[i].TrimEnd() != newContentSplit[i].TrimEnd())
                {
                    break;
                }
            }

            if (traversedLength == newContentSplit.Length - 1)
            {
                return;
            }
            File.WriteAllText(file, newContent);
        }

        public string SetOrUpdateHeader(string content, IDictionary<string, string> interpolation, string header, ILicenseHeaderFormatter formatter)
        {
            var hasWindowsLineEndings = content.Contains("\r\n");
            var formattedHeader = formatter.FormatHeader(header);
            var headerRegex = Regex.Escape(formattedHeader);
            headerRegex = headerRegex.Replace("\\n", "\\s*\\r?\\n");
            foreach (var interpolationKey in interpolation.Keys)
            {
                headerRegex = headerRegex.Replace(@"\{\{" + interpolationKey + "}}", ".+");
            }

            var regex = new Regex(headerRegex);

            var match = regex.Match(content);
            
            var headerExists = match.Success;
            var interpolatedHeader = InterpolateHeader(formattedHeader, interpolation);
            if (hasWindowsLineEndings)
            {
                interpolatedHeader = interpolatedHeader.Replace("\n", "\r\n");
            }
            if (headerExists)
            {
                content = ReplaceHeader(content, match.Index, match.Length, interpolatedHeader, formatter);
            }
            else
            {
                content = formatter.CreateHeader(content, interpolatedHeader);
            }

            return content;
        }
        
        public string InterpolateHeader(string formattedHeader, IDictionary<string, string> interpolation)
        {
            foreach (var interpolationKey in interpolation.Keys)
            {
                formattedHeader =
                    formattedHeader.Replace("{{" + interpolationKey + "}}", interpolation[interpolationKey]);
            }

            return formattedHeader + "\n\n";
        }
        
        public string ReplaceHeader(string content, int startIndex, int length, string interpolatedHeader, ILicenseHeaderFormatter formatter)
        {
            var offset = content.Contains("\r\n") ? 4 : 2;
            var originalContent = content.Remove(startIndex, length + offset);
            return formatter.CreateHeader(originalContent, interpolatedHeader);
        }
    }
}