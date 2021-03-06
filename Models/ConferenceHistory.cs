using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commander.Models{

    public class ConferenceHistory : NumberEntityField
    {
        [Required, StringLength(750)]
        public string ConferenceId { get; set; }

        [MaxLength(150)]
        public string JoinedPersonName { get; set;}

        public DateTime? JoineDateTime { get; set; }
        public DateTime? LeaveDateTime { get; set; }

    }
}