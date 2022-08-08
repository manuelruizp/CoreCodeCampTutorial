using System.ComponentModel.DataAnnotations;

namespace CoreCodeCamp.Models
{
    // Created new "Target" model to avoid overbinding with the SpeakerModel
    public class SpeakerTargetModel
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }
 
        [Required]
        [StringLength(50)]
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Company { get; set; }
        public string CompanyUrl { get; set; }
        public string BlogUrl { get; set; }
        public string Twitter { get; set; }
        public string GitHub { get; set; }
    }
}