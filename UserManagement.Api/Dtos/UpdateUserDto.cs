using System.ComponentModel.DataAnnotations;

namespace UserManagement.Api.Dtos
{
    /// <summary>
    /// Data transfer object containing only properties allowed to be updated.
    /// Email is excluded as it cannot be modified post-creation.
    /// </summary>
    public class UpdateUserDto
    {
        /// <summary>
        /// Gets or sets the updated first name.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the updated last name.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
    }
}