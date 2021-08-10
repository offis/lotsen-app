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

using Newtonsoft.Json;

namespace LotsenApp.LicenseManager.DependencyCrawling.Nuget
{
    public class NugetCatalogEntry
    {
        [JsonProperty("@id")]
        public string IdPackage { get; set; }
        [JsonProperty("@type")]
        public string[] Type { get; set; }
        public string Authors { get; set; }
        // CommitId omitted
        // Commit Timestamp omitted
        public string Copyright { get; set; }
        public string Created { get; set; }
        public string Description { get; set; }
        public string IconFile { get; set; }
        public string Id { get; set; }
        public bool IsPrerelease { get; set; }
        public string LastEdited { get; set; }
        public string LicenseExpression { get; set; }
        public string LicenseUrl { get; set; }
        public bool Listed { get; set; }
        public string MinClientVersion { get; set; }
        public string PackageHash { get; set; }
        public string PackageHashAlgorithm { get; set; }
        public int PackageSize { get; set; }
        public string ProjectUrl { get; set; }
        public string Published { get; set; }
        public string Repository { get; set; }
        public bool RequireLicenseAcceptance { get; set; }
        public string Title { get; set; }
        public string VerbatimVersion { get; set; }
        public string Version { get; set; }

        public DependencyGroup[] DependencyGroups { get; set; } = new DependencyGroup[0];
        // Package Entries omitted
        // Context omitted
    }
}