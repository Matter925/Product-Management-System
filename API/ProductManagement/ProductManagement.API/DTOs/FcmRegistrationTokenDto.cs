using System.ComponentModel.DataAnnotations;

namespace ProductManagement.API.DTOs;

public class FcmRegistrationTokenCreateDto
{
    public string UserId { get; set; } = null!;

    [StringLength(200)]
    public string FcmToken { get; set; } = null!;
}
