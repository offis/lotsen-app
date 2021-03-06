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

using System;
using System.IO;
using System.Linq;

namespace LotsenApp.LicenseManager.Configuration
{
    public class LicenseManagerConfiguration
    {
        private string _directoryRoot;
        public string LotsenAppRepositoryRoot
        {
            get
            {
                if (_directoryRoot != null)
                {
                    return _directoryRoot;
                }
                var currentDirectory = Environment.CurrentDirectory;
                var currentDirectoryInfo = new DirectoryInfo(currentDirectory);
                while (currentDirectoryInfo?.GetFiles().All(f => f.Name != "LotsenApp.Client.sln") ?? false)
                {
                    currentDirectoryInfo = currentDirectoryInfo.Parent;
                }

                _directoryRoot = currentDirectoryInfo?.FullName;

                return currentDirectoryInfo?.FullName ?? currentDirectory;
            }
        }

        public string CacheFolder => Path.Join(LotsenAppRepositoryRoot, "tools/LotsenApp.LicenseManager/Assets");
    }
}