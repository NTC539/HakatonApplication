using HakatonApplication.Context;
using HakatonApplication.DTO;
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
                Experts = experts.Where(e => e.HakatonId == h.Id).Select(e => e.ExpertName).Distinct().ToList()
            }).ToList();

            return result;
        } 
    }
}
  