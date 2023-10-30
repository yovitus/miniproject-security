using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureAggregationAPI.Services
{
    public class HospitalService
    {
        // Store the Hospital's API key
        private string hospitalApiKey = "YourHospitalApiKey";

        public bool ValidateHospitalApiKey(string apiKey)
        {
            return apiKey == hospitalApiKey;
        }
    }
}
