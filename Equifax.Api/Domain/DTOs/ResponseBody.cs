namespace Equifax.Api.Domain.DTOs
{
    public class ResponseBody
    {
        public bool status { get; set; }
        public string message { get; set; }
        public dynamic? data { get; set; }
    }
}
