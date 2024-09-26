using System.ComponentModel.DataAnnotations;

namespace Equifax.Api.Domain.DTOs
{
    public class PayloadRequestDto
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 8, ErrorMessage = "Password must be at least 8 to 20 characters long.")]
        public string Password { get; set; }
    }
}
