using HakatonApplication.Context;
using HakatonApplication.DTO;
using HakatonApplication.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HakatonApplication.Service
{
    public interface IHakatonService
    {
        Task<List<HakatonListItemDto>> GetAllHakatonsAsync(string? searchText = null);
        Task<HakatonDetailsDto> GetHakatonDetailsAsync(int id);
    }

    public class HakatonService : IHakatonService
    {
        private readonly HakatonDbContext _context;

        public HakatonService(HakatonDbContext context)
        {
            _context = context;
        }

        public async Task<List<HakatonListItemDto>> GetAllHakatonsAsync(string? searchText = null)
        {
            var query = _context.Hakatons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchText))
                query = query.Where(h => h.Name.Contains(searchText) || h.Description.Contains(searchText));

            var mainData = await query
                .Select(h => new
                {
                    h.Id,
                    h.Name,
                    h.Description,
                    StartDate = h.Stages.Min(s => s.StartDate),
                    EndDate = h.Stages.Max(s => s.EndDate),
                    TotalTeams = h.Teams.Count,
                    TotalParticipants = h.HakatonRegistrations.Count(r => r.RoleId == 1),
                    TotalExperts = h.HakatonRegistrations.Count(r => r.RoleId == 2),
                    TotalPrizeFund = h.SponsorContributions.Sum(sc => sc.Money)
                })
                .ToListAsync();

            var ids = mainData.Select(m => m.Id).ToList();

            var tags = await _context.Hakatons
                .Where(h => ids.Contains(h.Id))
                .SelectMany(h => h.HakatonTypes.Select(ht => new { HakatonId = h.Id, Tag = ht.TypeName ?? "" }))
                .Distinct()
                .ToListAsync();

            var sponsors = await _context.SponsorContributions
                .Where(sc => ids.Contains(sc.HakatonId))
                .Select(sc => new { sc.HakatonId, SponsorName = sc.Sponsor.Name ?? "" })
                .Distinct()
                .ToListAsync();

            var experts = await _context.HakatonRegistrations
                .Where(r => ids.Contains(r.HakatonId) && r.RoleId == 2 && r.User != null)
                .Select(r => new { r.HakatonId, ExpertName = (r.User.LastName + " " + r.User.FirstName).Trim() })
                .Distinct()
                .ToListAsync();

            int currentUserId = AppState.CurrentUserId;
            Dictionary<int, int> userRoles = new Dictionary<int, int>();
            if (currentUserId > 0)
            {
                var registrations = await _context.HakatonRegistrations
                    .Where(r => ids.Contains(r.HakatonId) && r.UserId == currentUserId)
                    .Select(r => new { r.HakatonId, r.RoleId })
                    .ToListAsync();
                userRoles = registrations
                    .Where(r => r.RoleId.HasValue)
                    .ToDictionary(r => r.HakatonId, r => r.RoleId.Value);
            }

            Dictionary<int, string> roleNames = new Dictionary<int, string>();
            if (userRoles.Values.Any())
            {
                var distinctRoleIds = userRoles.Values.Distinct();
                var roles = await _context.Roles
                    .Where(r => distinctRoleIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.Name })
                    .ToListAsync();
                roleNames = roles.ToDictionary(r => r.Id, r => r.Name ?? "");
            }

            var result = mainData.Select(h => new HakatonListItemDto
            {
                Id = h.Id,
                Name = h.Name ?? "",
                Description = h.Description ?? "",
                StartDate = h.StartDate ?? DateTime.MinValue,
                EndDate = h.EndDate ?? DateTime.MinValue,
                TotalTeams = h.TotalTeams,
                TotalParticipants = h.TotalParticipants,
                TotalExperts = h.TotalExperts,
                TotalPrizeFund = h.TotalPrizeFund ?? 0,
                Tags = tags.Where(t => t.HakatonId == h.Id).Select(t => t.Tag).ToList(),
                Sponsors = sponsors.Where(s => s.HakatonId == h.Id).Select(s => s.SponsorName).Distinct().ToList(),
                Experts = experts.Where(e => e.HakatonId == h.Id).Select(e => e.ExpertName).Distinct().ToList(),
                CurrentUserRoleId = userRoles.ContainsKey(h.Id) ? userRoles[h.Id] : 0,
                CurrentUserRoleName = userRoles.ContainsKey(h.Id) && roleNames.ContainsKey(userRoles[h.Id])
                    ? roleNames[userRoles[h.Id]]
                    : "Не зарегистрирован"

            }).ToList();

            return result;
        }
        public async Task<HakatonDetailsDto> GetHakatonDetailsAsync(int id)
        {
            var hakaton = await _context.Hakatons
                .Where(h => h.Id == id)
                .Select(h => new HakatonDetailsDto
                {
                    Id = h.Id,
                    Name = h.Name ?? "",
                    Description = h.Description ?? "",
                    Stages = h.Stages.OrderBy(s => s.OrderNumber).Select(s => new StageViewModel
                    {
                        Description = s.Description,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        Tasks = s.Tasks.Select(t => new TaskViewModel
                        {
                            Description = t.Description,
                            IsSolutionsPublic = t.IsSolutionsPublic == 1,
                            Criteria = t.TaskCriteria.Select(tc => new CriteriaViewModel
                            {
                                Name = tc.Criteria.Name,
                                Description = tc.Description,
                                MaxMark = tc.MaxMark
                            }).ToList()
                        }).ToList()
                    }).ToList(),
                    Teams = h.Teams.Select(t => new TeamViewModel
                    {
                        Name = t.Name,
                        Members = t.Registrations.Select(r => $"{r.User.LastName} {r.User.FirstName}").ToList()
                    }).ToList(),
                    SponsorContributions = h.SponsorContributions.Select(sc => new SponsorContributionViewModel
                    {
                        SponsorName = sc.Sponsor.Name,
                        Money = sc.Money,
                        Description = sc.Description
                    }).ToList(),
                    PrizeFunds = h.HakatonNominations.SelectMany(hn => hn.PrizeFunds.Select(pf => new PrizeFundViewModel
                    {
                        NominationName = hn.Nomination.Name,
                        Place = pf.Place,
                        Amount = pf.Contributions.Sum(c => c.Money),
                        WinnerTeamName = null  
                    })).ToList()
                })
                .FirstOrDefaultAsync();

            return hakaton ?? new HakatonDetailsDto();
        }
    }
}
  