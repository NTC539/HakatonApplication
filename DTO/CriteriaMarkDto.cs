using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakatonApplication.DTO
{
    public class CriteriaMarkDto
    {
        public int CriteriaId { get; set; }
        public string Name { get; set; } = "";
        public decimal MaxMark { get; set; }
        public decimal Mark { get; set; }
        public string Comment { get; set; } = "";
    }
}
