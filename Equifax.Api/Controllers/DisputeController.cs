using Microsoft.AspNetCore.Mvc;

namespace Equifax.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisputeController : ControllerBase
    {
        public DisputeController()
        {
                
        }


        //[HttpPost("Dispute")]
        //public IActionResult ProcessDispute([FromBody] ProcessDisputeRequestDto request)
        //{
        //    if (request == null || request.EquifaxData == null)
        //    {
        //        return BadRequest("Invalid request data");
        //    }

        //    Define variables to hold the results
        //    Account? matchedAccount = null;

        //    Handle based on dispute type
        //    switch (request.DisputeType.ToLower())
        //    {
        //        case request.DisputeType is "Accounts":

        //            matchedAccount = FindAccountByCreditorAndDate(request.RequestData, request.RequestData.CreditorName, request.RequestData.OpenDate);
        //            break;

        //        case "collection":
        //            Handle logic for collection
        //            break;

        //        case "inquiries":
        //            Handle logic for inquiries
        //            break;

        //        default:
        //            return BadRequest("Invalid dispute type");
        //    }

        //    if (matchedAccount != null)
        //    {
        //        return Ok(matchedAccount);
        //    }

        //    return NotFound("No matching record found.");
        //}


        //Method to find unique account based on creditor_name and open_date
        //private Task FindAccountByCreditorAndDate(string disputeType, string creditorName, string openDate)
        //{
        //    DateTime targetOpenDate;
        //    bool isValidDate = DateTime.TryParse(openDate, out targetOpenDate);

        //    if (!isValidDate)
        //    {
        //        return null;
        //    }

        //    Find the matching account based on creditorName and openDate
        //    return accounts.FirstOrDefault(account =>
        //        account.CreditorName.Equals(creditorName, StringComparison.OrdinalIgnoreCase) &&
        //        DateTime.TryParse(account.OpenDate, out DateTime accountOpenDate) &&
        //        accountOpenDate == targetOpenDate);
        //}
    }
}
