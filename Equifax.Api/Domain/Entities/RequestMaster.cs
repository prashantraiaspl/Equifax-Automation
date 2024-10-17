using Equifax.Api.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Equifax.Api.Domain.Models
{
    public class RequestMaster
    {
        [Key]
        public int RequestId { get; set; }
        public string user_name { get; set; }
        public string user_password { get; set; }
        public int client_id { get; set; }
        public string? creditor_name { get; set; }
        public string? open_date { get; set; }
        public RequestStatus request_status { get; set; }
        public string? confirmation_number { get; set; }
    }
}
