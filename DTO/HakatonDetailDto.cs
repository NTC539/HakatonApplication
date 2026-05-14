using HakatonApplication.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HakatonApplication.DTO
{
    public class HakatonDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<StageViewModel> Stages { get; set; } = new();
        public List<TeamViewModel> Teams { get; set; } = new();
        public List<SponsorContributionViewModel> SponsorContributions { get; set; } = new();
        public List<PrizeFundViewModel> PrizeFunds { get; set; } = new();
    }
}
