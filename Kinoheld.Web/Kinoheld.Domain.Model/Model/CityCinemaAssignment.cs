using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kinoheld.Domain.Model.Model
{
    public class CityCinemaAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Cinema { get; set; }
        
        public long UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual KinoheldUser User { get; set; }
    }
}