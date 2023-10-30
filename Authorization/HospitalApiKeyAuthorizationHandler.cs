using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SecureAggregationAPI.Services;

namespace SecureAggregationAPI.Authorization
{
    public class HospitalApiKeyAuthorizationHandler : AuthorizationHandler<HospitalApiKeyAuthorizationRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HospitalApiKeyAuthorizationRequirement requirement)
        {
            if (context.Resource is Microsoft.AspNetCore.Mvc.Filters.AuthorizationFilterContext authorizationContext)
            {
                var apiKey = authorizationContext.HttpContext.Request.Headers["ApiKey"];
                var hospitalService = authorizationContext.HttpContext.RequestServices.GetRequiredService<HospitalService>();

                if (string.IsNullOrWhiteSpace(apiKey) || !hospitalService.ValidateHospitalApiKey(apiKey))
                {
                    context.Fail();
                }
                else
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
