using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobTrackr.Domain.Entities
{
    public class User
    {
        /*
         * - `Id`
- `FullName`
- `Email`
- `CreatedAtUtc`
         * 
         * */

        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
