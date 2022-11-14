using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudyBuddy.Models
{
    public class AzureStorageConfig
    {
        public string CONTAINER_CONNECTION_STRING { get; set; }
        public string CONTAINER_NAME { get; set; }

        public string STORAGE_ACCOUNT { get; set; }
    }
}