using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobTrackr.Domain.Entities
{
    public class JobTask
    {
        /*
         * 
- `Id`
- `Title`
- `Description`
- `IsCompleted`
- `CreatedAtUtc`

         * 
         * 
         * 
         */
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    }
}
