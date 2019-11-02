using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ovicus.ShowMyHomework.Auth
{
    public class School
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Address { get; set; }

        public string Town { get; set; }

        [JsonProperty("post_code")]
        public string PostCode { get; set; }

        [JsonProperty("is_active")]
        public bool IsActive { get; set; }
    }
}
