﻿using LotsenApp.Client.Configuration.Api;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json;

namespace LotsenApp.Client.Configuration.Database
{
    public class GlobalConfigurationEntry
    {
        public string ConfigurationId { get; set; }
        public string Configuration { get; set; }
        
        
    }
}