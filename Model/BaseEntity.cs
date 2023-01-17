using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthTracker.api.Model
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public int status { get; set; } = 1;

        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDateTime { get; set; }
    }

}
