using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobTrackr.Application.Tasks
{
    public  class TaskResponse
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public bool IsCompleted     { get; set; }

        public DateTime CreatedAtUtc { get; set; }


    }
}
