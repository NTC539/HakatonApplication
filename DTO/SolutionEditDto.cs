using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakatonApplication.DTO
{
    public class SolutionEditDto
    {
        public int Id { get; set; }
        public int TaskId { get; set; }
        public int TeamId { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Source { get; set; } = "";
    }
}
