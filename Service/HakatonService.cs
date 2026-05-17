using HakatonApplication.Context;
using HakatonApplication.DTO;
using HakatonApplication.Models;
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
        Task<int> CreateEmptyHakatonAsync(int creatorUserId);
        Task UpdateHakatonAsync(int id, string name, string description);

        Task<Stage?> GetStageByIdAsync(int id);
        Task AddStageAsync(Stage stage);
        Task UpdateStageAsync(Stage stage);
        Task DeleteStageAsync(int stageId);

        Task<StageTask?> GetTaskByIdAsync(int id);
        Task AddTaskAsync(StageTask task);
        Task UpdateTaskAsync(StageTask task);
        Task DeleteTaskAsync(int taskId);

        Task<TaskCriterion?> GetCriteriaByIdAsync(int id);
        Task AddCriteriaAsync(TaskCriterion criteria);
        Task UpdateCriteriaAsync(TaskCriterion criteria);
        Task DeleteCriteriaAsync(int criteriaId);

        Task<List<Criterion>> GetAllCriteriaAsync();

        Task<bool> IsUserRegisteredOnHakatonAsync(int hakatonId, int userId);
        Task RegisterUserOnHakatonAsync(int hakatonId, int userId, int roleId = 1); 
        Task<List<UserInviteDto>> GetAvailableUsersForHakatonAsync(int hakatonId);
        Task<List<Role>> GetAllRolesAsync();

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
                        Id = s.Id,                      
                        Description = s.Description,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate,
                        OrderNumber = s.OrderNumber,
                        LocationId = s.LocationId,
                        StageTypeId = s.StageTypeId,
                        Tasks = s.Tasks.Select(t => new TaskViewModel
                        {
                            Id = t.Id,
                            Description = t.Description,
                            IsSolutionsPublic = t.IsSolutionsPublic == 1,
                            Criteria = t.TaskCriteria.Select(tc => new CriteriaViewModel
                            {
                                Id = tc.Id,                
                                Name = tc.Criteria.Name,
                                Description = tc.Description,
                                MaxMark = tc.MaxMark
                            }).ToList()
                        }).ToList()
                    }).ToList(),
                    Teams = h.Teams.Select(t => new TeamViewModel
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Members = t.Registrations.Select(r => $"{r.User.LastName} {r.User.FirstName}").ToList()
                    }).ToList(),
                    SponsorContributions = h.SponsorContributions.Select(sc => new SponsorContributionViewModel
                    {
                        Id = sc.Id,
                        SponsorName = sc.Sponsor.Name,
                        Money = sc.Money,
                        Description = sc.Description
                    }).ToList(),
                    PrizeFunds = h.HakatonNominations.SelectMany(hn => hn.PrizeFunds.Select(pf => new PrizeFundViewModel
                    {
                        Id = pf.Id,
                        NominationName = hn.Nomination.Name,
                        Place = pf.Place,
                        Amount = pf.Contributions.Sum(c => c.Money),
                        WinnerTeamName = null
                    })).ToList(),
                    CurrentUserRoleId = _context.HakatonRegistrations
                        .Where(r => r.HakatonId == id && r.UserId == AppState.CurrentUserId)
                        .Select(r => r.RoleId ?? 0)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            return hakaton ?? new HakatonDetailsDto();
        }

        public async Task<int> CreateEmptyHakatonAsync(int creatorUserId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var hakaton = new Hakaton
                {
                    Name = "Новый хакатон",
                    Description = ""
                };
                _context.Hakatons.Add(hakaton);
                await _context.SaveChangesAsync();

                var registration = new HakatonRegistration
                {
                    HakatonId = hakaton.Id,
                    UserId = creatorUserId,
                    RoleId = 3, 
                    RegistrationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified).AddHours(3)
                };
                _context.HakatonRegistrations.Add(registration);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return hakaton.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Stage?> GetStageByIdAsync(int id)
        {
            return await _context.Stages.FindAsync(id);
        }

        public async Task<StageTask?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TaskCriterion?> GetCriteriaByIdAsync(int id)
        {
            return await _context.TaskCriteria.FindAsync(id);
        }

        public async Task UpdateHakatonAsync(int id, string name, string description)
        {
            var hakaton = await _context.Hakatons.FindAsync(id);
            if (hakaton != null)
            {
                hakaton.Name = name;
                hakaton.Description = description;
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddStageAsync(Stage stage)
        {
            _context.Stages.Add(stage);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStageAsync(Stage stage)
        {
            var existing = await _context.Stages.FindAsync(stage.Id);
            if (existing != null)
            {
                existing.Description = stage.Description;
                existing.StartDate = stage.StartDate;
                existing.EndDate = stage.EndDate;
                existing.OrderNumber = stage.OrderNumber;
                existing.LocationId = stage.LocationId;
                existing.StageTypeId = stage.StageTypeId;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteStageAsync(int stageId)
        {
            var stage = await _context.Stages.FindAsync(stageId);
            if (stage != null)
            {
                _context.Stages.Remove(stage);
                await _context.SaveChangesAsync();
            }
        }

        // Task (задания)
        public async Task AddTaskAsync(StageTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTaskAsync(StageTask task)
        {
            var existing = await _context.Tasks.FindAsync(task.Id);
            if (existing != null)
            {
                existing.Description = task.Description;
                existing.IsSolutionsPublic = task.IsSolutionsPublic;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }

        // Criteria
        public async Task AddCriteriaAsync(TaskCriterion criteria)
        {
            _context.TaskCriteria.Add(criteria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCriteriaAsync(TaskCriterion criteria)
        {
            var existing = await _context.TaskCriteria.FindAsync(criteria.Id);
            if (existing != null)
            {
                existing.Description = criteria.Description;
                existing.MaxMark = criteria.MaxMark;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteCriteriaAsync(int criteriaId)
        {
            var criteria = await _context.TaskCriteria.FindAsync(criteriaId);
            if (criteria != null)
            {
                _context.TaskCriteria.Remove(criteria);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Criterion>> GetAllCriteriaAsync()
        {
            return await _context.Criteria.ToListAsync();
        }

        public async Task<bool> IsUserRegisteredOnHakatonAsync(int hakatonId, int userId)
        {
            return await _context.HakatonRegistrations
                .AnyAsync(r => r.HakatonId == hakatonId && r.UserId == userId);
        }

        public async Task RegisterUserOnHakatonAsync(int hakatonId, int userId, int roleId = 1)
        {
            var existing = await _context.HakatonRegistrations
                .FirstOrDefaultAsync(r => r.HakatonId == hakatonId && r.UserId == userId);
            if (existing != null) return;

            var registration = new HakatonRegistration
            {
                HakatonId = hakatonId,
                UserId = userId,
                RoleId = roleId,
                RegistrationDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };
            _context.HakatonRegistrations.Add(registration);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserInviteDto>> GetAvailableUsersForHakatonAsync(int hakatonId)
        {
            // Все пользователи, у которых ещё нет регистрации на этот хакатон
            var registeredUserIds = await _context.HakatonRegistrations
                .Where(r => r.HakatonId == hakatonId)
                .Select(r => r.UserId)
                .ToListAsync();

            var users = await _context.Users
                .Where(u => !registeredUserIds.Contains(u.Id))
                .Select(u => new UserInviteDto
                {
                    Id = u.Id,
                    FullName = (u.LastName + " " + u.FirstName + " " + u.Patronymic).Trim(),
                    Email = u.Contact != null ? u.Contact.Email : ""
                })
                .ToListAsync();
            return users;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.ToListAsync();
        }
    }
}
  