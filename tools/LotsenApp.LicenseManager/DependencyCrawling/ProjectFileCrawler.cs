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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LotsenApp.LicenseManager.DependencyCrawling
{
    public class ProjectFileCrawler
    {
        private readonly string[] _excludedFolders = {"node_modules", ".git", ".idea", "bin", "obj", ".vs", ".vscode"};
        private readonly IProjectCrawler[] _crawlers;

        public ProjectFileCrawler(IEnumerable<IProjectCrawler> crawlers)
        {
            _crawlers = crawlers.ToArray();
        }

        public async Task<IEnumerable<DependencyInformation>> Crawl(string initialDirectory)
        {
            var directory = new DirectoryInfo(initialDirectory);
            var projectFiles = directory.GetFiles()
                .Where(f => _crawlers.Any(c => f.Name.EndsWith(c.ApplicableFileExtension)));
            var dependencyInformation = projectFiles.Select(async p =>
            {
                var crawler = _crawlers.First(c => p.Name.EndsWith(c.ApplicableFileExtension));
                return await crawler.GetInformation(p.FullName);
            });
            var finishedDependencyInformation = (await Task.WhenAll(dependencyInformation))
                .SelectMany(d => d);
            var subdirectoryDependencies = directory.GetDirectories()
                .Where(d => !_excludedFolders.Contains(d.Name))
                .Select(async d => await Crawl(d.FullName));
            var finishedSubdirectories = (await Task.WhenAll(subdirectoryDependencies))
                .SelectMany(d => d);
            return finishedDependencyInformation.Concat(finishedSubdirectories);
        }
    }
}