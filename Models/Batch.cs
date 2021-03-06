using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Commander.Models{

    public class Batch : NumberEntityField
    {


        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        public long ProjectId { get; set; }


        [Required, StringLength(750)]
        public string BatchName { get; set; }

        public DateTime? CreatedDateTime { get; set; }
    }
}