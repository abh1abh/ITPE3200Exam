using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace HomecareAppointmentManagement.Models;

public class AvailableSlot : IValidatableObject
{
    public int Id { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Healthcare worker is required.")]
    public int HealthcareWorkerId { get; set; }

    [ValidateNever]
    public virtual HealthcareWorker HealthcareWorker { get; set; } = default!;

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime Start { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTime End { get; set; }

    public bool IsBooked { get; set; }

    public virtual Appointment? Appointment { get; set; } // Navigation property to Appointment    

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (End <= Start)
        {
            yield return new ValidationResult(
                "End time must be after start time.",
                new[] { nameof(Start), nameof(End) });
        }

        var duration = End - Start;
        if (duration != TimeSpan.FromHours(1))
        {
            yield return new ValidationResult(
                "Each slot must be exactly 1 hour long.",
                new[] { nameof(Start), nameof(End) });
        }
    }
}

