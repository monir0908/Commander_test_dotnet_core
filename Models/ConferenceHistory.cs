using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commander.Models{

    public class ConferenceHistory : NumberEntityField
    {
        [Required, StringLength(750)]
        public string ConferenceId { get; set; }

        [StringLength(750)]
        public string RoomId { get; set; }


        [ForeignKey("HostId")]
        public virtual ApplicationUser Host { get; set; }
        public string HostId { get; set; }

        [ForeignKey("ParticipantId")]
        public virtual ApplicationUser Participant { get; set; }
        public string ParticipantId { get; set; }

        [MaxLength(150)]
        public string SocketId { get; set;}

        public DateTime? JoineDateTime { get; set; }
        public DateTime? LeaveDateTime { get; set; }

    }
}