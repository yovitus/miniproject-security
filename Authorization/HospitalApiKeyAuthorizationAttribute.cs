using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SecureAggregationAPI.Services;

namespace SecureAggregationAPI.Authorization
{
    public class HospitalApiKeyAuthorizationAttribute : TypeFilterAttribute
    {
        public HospitalApiKeyAuthorizationAttribute() : base(typeof(HospitalApiKeyAuthorizationFilter))
        {
        }
    }

    public class HospitalApiKeyAuthorizationFilter : IAuthorizationFilter
    {
        private readonly HospitalService hospitalService;

        public HospitalApiKeyAuthorizationFilter(HospitalService service)
        {
            hospitalService = service;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var apiKey = context.HttpContext.Request.Headers["ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey) || !hospitalService.ValidateHospitalApiKey(apiKey))
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
                                    