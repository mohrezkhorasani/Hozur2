using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NajmBridge.Presenter
{
    public sealed class JsonContentResult : ContentResult
    {

        public object obj { get; set; }
        public string Serialize(object obj)
        {
            this.obj = obj;
            return JsonConvert.SerializeObject(obj,
                Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
        }
    }
}
