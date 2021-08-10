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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LotsenApp.LicenseManager.LicenseCreation;
using Xunit;

namespace LotsenApp.LicenseManager.Test
{
    [ExcludeFromCodeCoverage]
    public class LicenseHeaderFormatterTest
    {
        [Fact]
        public void TestHeaderInsertion()
        {
            const string content = "<?xml version=\"1\" encoding=\"UTF-8\" ?>\n" +
                                   "<some-parent>\n" +
                                   "  <some-child></some-child>\n" +
                                   "</some-parent>\n";
            const string expectedResult = "<?xml version=\"1\" encoding=\"UTF-8\" ?>\n" +
                                          "<!-- test-header: SOME_AUTHOR -->\n" +
                                          "<!-- multiple-lines2020 -->\n\n\n" +
                                          "<some-parent>\n" +
                                          "  <some-child></some-child>\n" +
                                          "</some-parent>\n";
            var xmlFormatter = new XmlLicenseHeaderFormatter();
            var formatter = new LicenseHeaderFormatter();
            var newContent = formatter.SetOrUpdateHeader(content, new Dictionary<string, string>
            {
                {"author", "SOME_AUTHOR"},
                {"year", "2020"}
            }, "test-header: {{author}}\nmultiple-lines{{year}}", xmlFormatter);
            
            Assert.Equal(expectedResult, newContent);
        }
        
        [Fact]
        public void TestHeaderReplacementWithRealLicense()
        {
            var content = File.ReadAllText("../../../LicenseHeaderFormatterTest.cs").Replace("\r\n", "\n");
            var xmlFormatter = new CSharpLicenseHeaderFormatter();
            var formatter = new LicenseHeaderFormatter();
            var newContent = formatter.SetOrUpdateHeader(content, new Dictionary<string, string>
            {
                {"company", "OFFIS e.V."},
                {"year", "2021"}
            }, "Copyright (c) {{year}} {{company}}. All rights reserved.\n\nRedistribution and use in source and binary forms, with or without\nmodification, are permitted provided that the following conditions are met:\n\n1. Redistributions of source code must retain the above copyright notice, this\n   list of conditions and the following disclaimer.\n   \n2. Redistributions in binary form must reproduce the above copyright notice,\n   this list of conditions and the following disclaimer in the documentation\n   and/or other materials provided with the distribution.\n   \n3. Neither the name of the copyright holder nor the names of its contributors\n   may be used to endorse or promote products derived from this software without\n   specific prior written permission.\n   \nTHIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS \"AS IS\" AND\nANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED\nWARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE\nDISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE\nFOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL\nDAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR\nSERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER\nCAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,\nOR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE\nOF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.", xmlFormatter);
            Assert.Equal(content, newContent);
        }
        
        [Fact]
        public void TestHeaderReplacement()
        {
            const string content = "<?xml version=\"1\" encoding=\"UTF-8\" ?>\n" +
                                   "<!-- test-header: SOME_AUTHOR -->\n" +
                                   "<!-- multiple-lines2020 -->\n\n" +
                                   "<some-parent>\n" +
                                   "  <some-child></some-child>\n" +
                                   "</some-parent>\n";
            const string expectedResult = "<?xml version=\"1\" encoding=\"UTF-8\" ?>\n" +
                                          "<!-- test-header: SOME_AUTHOR -->\n" +
                                          "<!-- multiple-lines2021 -->\n\n\n" +
                                          "<some-parent>\n" +
                                          "  <some-child></some-child>\n" +
                                          "</some-parent>\n";
            var xmlFormatter = new XmlLicenseHeaderFormatter();
            var formatter = new LicenseHeaderFormatter();
            var newContent = formatter.SetOrUpdateHeader(content, new Dictionary<string, string>
            {
                {"author", "SOME_AUTHOR"},
                {"year", "2021"}
            }, "test-header: {{author}}\nmultiple-lines{{year}}", xmlFormatter);
            
            Assert.Equal(expectedResult, newContent);
        }
    }
}