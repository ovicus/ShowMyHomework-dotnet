using Newtonsoft.Json;
using System;

namespace Ovicus.ShowMyHomework
{
    public class Todo
    {
        public int Id { get; set; }

        [JsonProperty("teacher_name")]
        public string Teacher { get; set; }
        
        [JsonProperty("subject")]
        public string Subject { get; set; }
        
        [JsonProperty("class_task_title")]
        public string Title { get; set; }

        [JsonProperty("class_task_type")]
        public string Type { get; set; }

        [JsonProperty("class_task_description")]
        public string Description { get; set; }

        [JsonProperty("completed")]
        public bool Completed { get; set; }

        [JsonProperty("due_on")]
        public DateTimeOffset DueOn { get; set; }

        [JsonProperty("issued_on")]
        public DateTimeOffset IssuedOn { get; set; }
    }
}
