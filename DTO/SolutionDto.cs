using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakatonApplication.DTO
{
    public class SolutionDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime DeliveryDate { get; set; }
        public string Source { get; set; } = ""; 
        public string TeamName { get; set; } = "";
        public int TeamId { get; set; }
    }
}
