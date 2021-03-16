using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commander{

    public class DateTimeParams
    {        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }

    public class CallDurationHistory
    {        
        public TimeSpan? HostCallDuration { get; set; }
        public TimeSpan? ParticipantsCallDuration { get; set; }
        public TimeSpan? EmptySlotDuration { get; set; }
        public TimeSpan? ActualCallDuration { get; set; }
        public int ParticipantJoined { get; set; }
        public int UniqueParticipantCounts { get; set; }
    }

    public class ParticipantList
    {        

        public string Id{ get; set; }
    }
}