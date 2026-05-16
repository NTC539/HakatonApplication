using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakatonApplication.DTO
{
    public class HakatonListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalTeams { get; set; }
        public int TotalParticipants { get; set; }
        public int TotalExperts { get; set; }
        public decimal TotalPrizeFund { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> Sponsors { get; set; } = new();
        public List<string> Experts { get; set; } = new();
        public int CurrentUserRoleId { get; set; } = 0;
        public string CurrentUserRoleName { get; set; } = "Не зарегистрирован";
    }
}
