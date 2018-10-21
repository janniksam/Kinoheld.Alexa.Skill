using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinoheld.Domain.Model.Model
{
    public class KinoheldUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string AlexaId { get; set; }
      
        public string City { get; set; }

        public bool DisableEmails { get; set; }

        [InverseProperty(nameof(CityCinemaAssignment.User))]
        public virtual ICollection<CityCinemaAssignment> CityCinemaAssignments { get; } = new HashSet<CityCinemaAssignment>();
    }
}