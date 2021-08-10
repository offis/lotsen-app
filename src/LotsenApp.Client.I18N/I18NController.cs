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

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LotsenApp.Client.Configuration.Api;
using LotsenApp.Client.DataFormat;
using LotsenApp.Client.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LotsenApp.Client.I18N
{
    [Route("/api/i18n")]
    [ApiController]
    public class I18NController: ControllerBase
    {
        private readonly IDataFormatStorage _dataFormatStorage;
        private readonly ILogger<I18NController> _logger;
        private readonly IConfigurationStorage _configuration;

        public I18NController(IDataFormatStorage dataFormatStorage, ILogger<I18NController> logger, IConfigurationStorage configuration)
        {
            _dataFormatStorage = dataFormatStorage;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("{**slug}")]
        public async Task<string> GetI18N([FromRoute] string slug)
        {
            var locale = slug?.Replace(".json", "") ?? "";
            _logger.LogDebug("Requested locale: " + locale);
            if (locale == "")
            {
                locale = (await _configuration.GetGlobalConfiguration()).DefaultLanguage;
            }
            
            var projectI18N = await _dataFormatStorage.GetProjectI18N(locale);
            // Get Normal I18N
            var standardI18N = GetStandardI18N(locale);
            var objects = projectI18N.Select(JObject.Parse);
            var i18NAggregate = objects.Aggregate(standardI18N, (current, next) =>
            {
                current.Merge(next);
                return current;
            });
            return i18NAggregate.ToString(Formatting.Indented);
        }

        private JObject GetStandardI18N(string locale)
        {
            var localeStream = GetType().Assembly.GetManifestResourceStream($"LotsenApp.Client.I18N.Assets.{locale}.json");
            if (localeStream == null)
            {
                throw new NotFoundException();
            }
            using var reader = new StreamReader(localeStream);
            var standardI18N = reader.ReadToEnd();
            return JObject.Parse(standardI18N);
        }
    }
}