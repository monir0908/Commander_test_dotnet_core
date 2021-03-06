using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Commander.Models;

namespace Commander.Models{

    public class BatchHost : NumberEntityField
    {
        [ForeignKey("ProjectId")]
        public virtual Project Project { get; set; }
        public long ProjectId { get; set; }

        [ForeignKey("BatchId")]
        public virtual Batch Batch { get; set; }
        public long BatchId { get; set; }

        [ForeignKey("HostId")]
        public virtual ApplicationUser Host { get; set; }
        public string HostId { get; set; }

    }
}