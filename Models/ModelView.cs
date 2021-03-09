using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commander{

    public class DateTimeParams
    {        
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}