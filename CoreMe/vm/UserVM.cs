using System.ComponentModel.DataAnnotations;

namespace CoreMe.VM
{
    public class UserMainVM
    {
        public string? UUID { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? NationalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ExtId { get; set; }
        public string? Email { get; set; }
        public string? AllowedGUID { get; set; }
        public string? Role { get; set; }
    }
    public class NewUserViewModel : UserMainVM, IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(UUID) &&
                string.IsNullOrWhiteSpace(NationalCode) &&
                string.IsNullOrWhiteSpace(PhoneNumber) &&
                string.IsNullOrWhiteSpace(Email))
            {
                yield return new ValidationResult(
                    "At least one of UUID,  NationalCode, PhoneNumber or Email must be provided.",
                    new[] { nameof(UUID), nameof(Firstname), nameof(Lastname), nameof(NationalCode), nameof(PhoneNumber), nameof(ExtId), nameof(Email) }
                );
            }
        }
    }


}