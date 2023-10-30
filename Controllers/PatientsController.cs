using Microsoft.AspNetCore.Mvc;
using SecureAggregationAPI.Authorization;
using SecureAggregationAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureAggregationAPI.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientsController : ControllerBase
    {
        private readonly SecretSharingService secretSharingService;

        static List<int> aliceShares = new List<int>();
        static List<int> bobShares = new List<int>();
        static List<int> charlieShares = new List<int>();

        static List<int> aliceCollectedShares = new List<int>();
        static List<int> bobCollectedShares = new List<int>();
        static List<int> charlieCollectedShares = new List<int>();


        public PatientsController(SecretSharingService secretSharingService)
        {
            this.secretSharingService = secretSharingService;
        }

        [HttpGet("hello")]
        public string GetHelloFromPatient()
        {
            return "Hello from Patient!";
        }

        [HttpPost("submit-share")]
        public IActionResult SubmitShare([FromBody] int data, [FromQuery] string patientName)
        {
            if (data < 0)
            {
                return BadRequest("Data cannot be negative.");
            }

            List<int> randomShares = new List<int>(); // Initialize the list of random shares

            switch (patientName.ToLower())
            {
                case "alice":
                    aliceShares.Add(data);
                    randomShares = secretSharingService.Split(data, 3); // Split into 3 random shares
                    
                    bobCollectedShares.Add(randomShares.ElementAt(0));
                    charlieCollectedShares.Add(randomShares.ElementAt(1));
                    aliceCollectedShares.Add(randomShares.ElementAt(2));
                    break;
                case "bob":
                    bobShares.Add(data);
                    randomShares = secretSharingService.Split(data, 3); // Split into 3 random shares
                   
                    charlieCollectedShares.Add(randomShares.ElementAt(0));
                    aliceCollectedShares.Add(randomShares.ElementAt(1));
                    bobCollectedShares.Add(randomShares.ElementAt(2));
                    break;
                case "charlie":
                    charlieShares.Add(data);
                    randomShares = secretSharingService.Split(data, 3); // Split into 3 random shares

                    aliceCollectedShares.Add(randomShares.ElementAt(0));
                    bobCollectedShares.Add(randomShares.ElementAt(1));
                    charlieCollectedShares.Add(randomShares.ElementAt(2));
                    break;
                default:
                    return BadRequest("Invalid patient name.");
            }

            // Return the response with random shares
            return Ok(new
            {
                Patient = patientName,
                Data = data,
                Message = "Data submitted.",
                aliceCollectedShares,
                bobCollectedShares,
                charlieCollectedShares,
            });
        }
        [HttpGet("see-shares")]
        [HospitalApiKeyAuthorization]
        public IActionResult checkValueHospitalGet() {
            return Ok(new {
                aliceShares,
                bobShares,
                charlieShares,
                aliceCollectedShares,
                bobCollectedShares,
                charlieCollectedShares,
            });
        }

        [HttpGet("get-aggregate-value")]
        [HospitalApiKeyAuthorization]
        public IActionResult GetAggregateValue()
        {
            //check if empty, else return error
            if (aliceCollectedShares.Count != 3 || bobCollectedShares.Count != 3 || charlieCollectedShares.Count != 3)
            {
                return BadRequest("Not all patients have submitted their shares.");
            }
            else
            {
                // Combine the shares from all patients
                var combinedShares = aliceCollectedShares.Take(aliceCollectedShares.Count).Sum() +
                                    bobCollectedShares.Take(bobCollectedShares.Count).Sum() +
                                    charlieCollectedShares.Take(charlieCollectedShares.Count).Sum();

                
                // Clear the shares for the next round
                aliceShares.Clear();
                bobShares.Clear();
                charlieShares.Clear();
                aliceCollectedShares.Clear();
                bobCollectedShares.Clear();
                charlieCollectedShares.Clear();

                return Ok($"Aggregate Value: {combinedShares}");
            }
        }
    }
}
