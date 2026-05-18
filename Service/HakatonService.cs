using HakatonApplication.Context;
using HakatonApplication.DTO;
using HakatonApplication.Models;
using HakatonApplication.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        Task<List<UserDto>> GetAvailableUsersForTeamAsync(int hakatonId);
        Task<int> CreateTeamAsync(int hakatonId, string teamName);
        Task AddUserToTeamAsync(int userId, int teamId);
        Task RemoveUserFromTeamAsync(int userId, int teamId);
        Task DeleteTeamAsync(int teamId);

        Task<HakatonRegistration?> GetUserRegistrationOnHakatonAsync(int hakatonId, int userId);
        Task<bool> UserHasTeamOnHakatonAsync(int hakatonId, int userId);

        Task<List<SolutionDto>> GetSolutionsForTaskAsync(int taskId, int currentUserId, bool isOrganizerOrExpert);
        Task<int> AddOrUpdateSolutionAsync(SolutionEditDto dto);
        Task<SolutionEditDto?> GetSolutionForEditAsync(int solutionId, int userId);
        Task<bool> HasUserSolutionForTaskAsync(int taskId, int teamId);

        Task<List<CriteriaMarkDto>> GetCriteriaForTaskAsync(int taskId);
        Task<List<TeamForRatingDto>> GetTeamsWithSolutionForTaskAsync(int taskId);
        Task<List<CriteriaMarkDto>> GetMarksForTeamAndTaskAsync(int teamId, int taskId, int expertRegistrationId);
        Task SaveMarksAsync(int taskId, int teamId, int expertRegistrationId, List<CriteriaMarkDto> marks);

        Task<int?> GetUserRoleOnHakatonAsync(int hakatonId, int userId);
        Task<int?> GetUserTeamIdOnHakatonByTaskAsync(int taskId, int currentUserId);
        Task<int> GetUserRoleOnHakatonByTaskAsync(int taskId, int currentUserId);

        Task<List<CriteriaMarkDto>> GetDetailedMarksForTeamTaskAsync(int teamId, int taskId);
        Task<decimal?> GetAverageScoreForTeamTaskAsync(int teamId, int taskId);

        Task<List<Location>> GetAllLocationsAsync();
        Task AddLocationAsync(Location location);
        Task<List<StageType>> GetAllStageTypesAsync();
        Task AddStageTypeAsync(StageType stageType);
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
                        Members = new ObservableCollection<UserDto>(
                             t.Registrations.Select(r => new UserDto
                             {
                                 Id = r.UserId,
                                 FullName = (r.User.LastName + " " + r.User.FirstName + " " + r.User.Patronymic).Trim(),
                                 RegistrationId = r.Id
                             }))
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

        public async Task AddCriteriaAsync(TaskCriterion criteria)
        {
            if (criteria.Criteria != null && !string.IsNullOrEmpty(criteria.Criteria.Name))
            {
                var existing = await _context.Criteria
                    .FirstOrDefaultAsync(c => c.Name == criteria.Criteria.Name);
                if (existing != null)
                    criteria.CriteriaId = existing.Id;
                else
                {
                    _context.Criteria.Add(criteria.Criteria);
                    await _context.SaveChangesAsync(); 
                    criteria.CriteriaId = criteria.Criteria.Id;
                }
                criteria.Criteria = null; 
            }
            _context.TaskCriteria.Add(criteria);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCriteriaAsync(TaskCriterion criteria)
        {
            var existing = await _context.TaskCriteria
                .Include(tc => tc.Criteria)
                .FirstOrDefaultAsync(tc => tc.Id == criteria.Id);
            if (existing == null) return;

            existing.Description = criteria.Description;
            existing.MaxMark = criteria.MaxMark;

            if (criteria.Criteria != null && !string.IsNullOrEmpty(criteria.Criteria.Name))
            {
                var existingCriterion = await _context.Criteria
                    .FirstOrDefaultAsync(c => c.Name == criteria.Criteria.Name);
                if (existingCriterion != null)
                    existing.CriteriaId = existingCriterion.Id;
                else
                {
                    _context.Criteria.Add(criteria.Criteria);
                    await _context.SaveChangesAsync();
                    existing.CriteriaId = criteria.Criteria.Id;
                }
            }
            await _context.SaveChangesAsync();
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

        public async Task<int> CreateTeamAsync(int hakatonId, string teamName)
        {
            var team = new Team { HakatonId = hakatonId, Name = teamName };
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
            return team.Id;
        }

        public async Task<List<UserDto>> GetAvailableUsersForTeamAsync(int hakatonId)
        {
            var usersInTeams = await _context.Teams
                .Where(t => t.HakatonId == hakatonId)
                .SelectMany(t => t.Registrations.Select(r => r.Id))
                .ToListAsync();

            var available = await _context.HakatonRegistrations
                .Where(r => r.HakatonId == hakatonId && r.RoleId == 1 && !usersInTeams.Contains(r.Id))
                .Select(r => new UserDto
                {
                    Id = r.UserId,
                    FullName = (r.User.LastName + " " + r.User.FirstName + " " + r.User.Patronymic).Trim(),
                    RegistrationId = r.Id
                })
                .ToListAsync();
            return available;
        }


        public async Task AddUserToTeamAsync(int userId, int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) return;

            var registration = await _context.HakatonRegistrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.HakatonId == team.HakatonId && r.RoleId == 1); // только участники
            if (registration == null) return;

            if (!team.Registrations.Contains(registration))
            {
                team.Registrations.Add(registration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveUserFromTeamAsync(int userId, int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team == null) return;

            var registration = await _context.HakatonRegistrations
                .FirstOrDefaultAsync(r => r.UserId == userId && r.HakatonId == team.HakatonId);
            if (registration == null) return;

            if (team.Registrations.Contains(registration))
            {
                team.Registrations.Remove(registration);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTeamAsync(int teamId)
        {
            var team = await _context.Teams
                .Include(t => t.Registrations)
                .FirstOrDefaultAsync(t => t.Id == teamId);
            if (team != null)
            {
                team.Registrations.Clear(); 
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<HakatonRegistration?> GetUserRegistrationOnHakatonAsync(int hakatonId, int userId)
        {
            return await _context.HakatonRegistrations
                .FirstOrDefaultAsync(r => r.HakatonId == hakatonId && r.UserId == userId);
        }

        public async Task<bool> UserHasTeamOnHakatonAsync(int hakatonId, int userId)
        {
            var registration = await GetUserRegistrationOnHakatonAsync(hakatonId, userId);
            if (registration == null) return false;
            return await _context.Teams
                .Where(t => t.HakatonId == hakatonId)
                .AnyAsync(t => t.Registrations.Any(r => r.Id == registration.Id));
        }
        public async Task<List<SolutionDto>> GetSolutionsForTaskAsync(int taskId, int currentUserId, bool isOrganizerOrExpert)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            bool isPublic = task?.IsSolutionsPublic == 1;

            var query = _context.Solutions
                .Include(s => s.Team)
                .Where(s => s.Tasks.Any(t => t.Id == taskId));

            if (!isPublic && !isOrganizerOrExpert)
            {
                var userTeamId = await _context.Teams
                    .Where(t => t.Hakaton.Stages.Any(s => s.Tasks.Any(t2 => t2.Id == taskId))
                                && t.Registrations.Any(r => r.UserId == currentUserId))
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();

                if (userTeamId != 0)
                    query = query.Where(s => s.TeamId == userTeamId);
                else
                    return new List<SolutionDto>();
            }

            return await query.Select(s => new SolutionDto
            {
                Id = s.Id,
                Name = s.Name ?? "",
                Description = s.Description ?? "",
                DeliveryDate = s.DeliveryDate ?? DateTime.MinValue,
                Source = s.Source ?? "",
                TeamName = s.Team.Name ?? "",
                TeamId = s.TeamId
            }).ToListAsync();
        }

        public async Task<int> AddOrUpdateSolutionAsync(SolutionEditDto dto)
        {
            if (dto.Id == 0)
            {
                var solution = new Solution
                {
                    TeamId = dto.TeamId,
                    Name = dto.Name,
                    Description = dto.Description,
                    DeliveryDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    Source = dto.Source
                };
                _context.Solutions.Add(solution);
                var task = await _context.Tasks.FindAsync(dto.TaskId);
                if (task == null) throw new InvalidOperationException($"Task with id {dto.TaskId} not found");
                solution.Tasks.Add(task);
                await _context.SaveChangesAsync();
                return solution.Id;
            }
            else
            {
                var solution = await _context.Solutions.FindAsync(dto.Id);
                if (solution == null) return 0;
                solution.Name = dto.Name;
                solution.Description = dto.Description;
                solution.Source = dto.Source;
                await _context.SaveChangesAsync();
                return solution.Id;
            }
        
        }

        public async Task<SolutionEditDto?> GetSolutionForEditAsync(int solutionId, int userId)
        {
            var solution = await _context.Solutions
                .Include(s => s.Tasks)
                .FirstOrDefaultAsync(s => s.Id == solutionId && s.Team.Registrations.Any(r => r.UserId == userId));
            if (solution == null) return null;
            return new SolutionEditDto
            {
                Id = solution.Id,
                TeamId = solution.TeamId,
                Name = solution.Name ?? "",
                Description = solution.Description ?? "",
                Source = solution.Source ?? "",
                TaskId = solution.Tasks.First().Id
            };
        }

        public async Task<List<CriteriaMarkDto>> GetCriteriaForTaskAsync(int taskId)
        {
            return await _context.TaskCriteria
                .Where(tc => tc.TaskId == taskId)
                .Select(tc => new CriteriaMarkDto
                {
                    CriteriaId = tc.Id,
                    Name = tc.Criteria.Name,
                    MaxMark = tc.MaxMark ?? 0,
                    Mark = 0
                }).ToListAsync();
        }

        public async Task SaveMarksAsync(int taskId, int teamId, int expertRegistrationId, List<CriteriaMarkDto> marks)
        {
            foreach (var mark in marks)
            {
                var existing = await _context.Marks
                    .FirstOrDefaultAsync(m => m.TaskCriteriaId == mark.CriteriaId && m.TeamId == teamId && m.RegistrationId == expertRegistrationId);
                if (existing != null)
                {
                    existing.Mark1 = mark.Mark;
                    existing.Description = mark.Comment;
                }
                else
                {
                    _context.Marks.Add(new Mark
                    {
                        TaskCriteriaId = mark.CriteriaId,
                        TeamId = teamId,
                        RegistrationId = expertRegistrationId,
                        Mark1 = mark.Mark,
                        Description = mark.Comment
                    });
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserSolutionForTaskAsync(int taskId, int teamId)
        {
            return await _context.Solutions
                .AnyAsync(s => s.TeamId == teamId && s.Tasks.Any(t => t.Id == taskId));
        }

        public async Task<int?> GetUserRoleOnHakatonAsync(int hakatonId, int userId)
        {
            return await _context.HakatonRegistrations
                .Where(r => r.HakatonId == hakatonId && r.UserId == userId)
                .Select(r => r.RoleId)
                .FirstOrDefaultAsync();
        }

        public async Task<int?> GetUserTeamIdOnHakatonByTaskAsync(int taskId, int userId)
        {
            var hakatonId = await _context.Tasks
                .Where(t => t.Id == taskId)
                .Select(t => t.Stage.HakatonId)
                .FirstOrDefaultAsync();
            if (hakatonId == 0) return null;

            var team = await _context.Teams
                .Where(t => t.HakatonId == hakatonId && t.Registrations.Any(r => r.UserId == userId))
                .FirstOrDefaultAsync();
            return team?.Id;
        }
        public async Task<List<TeamForRatingDto>> GetTeamsWithSolutionForTaskAsync(int taskId)
        {
            return await _context.Solutions
                .Where(s => s.Tasks.Any(t => t.Id == taskId))
                .Select(s => new TeamForRatingDto
                {
                    TeamId = s.TeamId,
                    TeamName = s.Team.Name ?? ""
                })
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<CriteriaMarkDto>> GetMarksForTeamAndTaskAsync(int teamId, int taskId, int expertRegistrationId)
        {
            var criteria = await GetCriteriaForTaskAsync(taskId);
            var existingMarks = await _context.Marks
                .Where(m => m.TeamId == teamId && m.RegistrationId == expertRegistrationId && m.TaskCriteria.TaskId == taskId)
                .ToDictionaryAsync(m => m.TaskCriteriaId);
            foreach (var c in criteria)
            {
                if (existingMarks.TryGetValue(c.CriteriaId, out var mark))
                {
                    c.Mark = mark.Mark1 ?? 0;
                    c.Comment = mark.Description ?? "";
                }
            }
            return criteria;
        }
        public async Task<int> GetUserRoleOnHakatonByTaskAsync(int taskId, int currentUserId)
        {
            var hakatonId = await _context.Tasks
                .Where(t => t.Id == taskId)
                .Select(t => t.Stage.HakatonId)
                .FirstOrDefaultAsync();
            if (hakatonId == 0) return 0;
            var role = await GetUserRoleOnHakatonAsync(hakatonId, currentUserId);
            return role ?? 0;
        }

        public async Task<List<CriteriaMarkDto>> GetDetailedMarksForTeamTaskAsync(int teamId, int taskId)
        {
            var criteria = await GetCriteriaForTaskAsync(taskId);
            var marks = await _context.Marks
                .Where(m => m.TeamId == teamId && m.TaskCriteria.TaskId == taskId)
                .GroupBy(m => m.TaskCriteriaId)
                .Select(g => new { CriteriaId = g.Key, AvgMark = g.Average(m => m.Mark1) ?? 0 })
                .ToListAsync();
            foreach (var c in criteria)
            {
                var mark = marks.FirstOrDefault(m => m.CriteriaId == c.CriteriaId);
                c.Mark = mark?.AvgMark ?? 0;
            }
            return criteria;
        }

        public async Task<decimal?> GetAverageScoreForTeamTaskAsync(int teamId, int taskId)
        {
            return await _context.Marks
                .Where(m => m.TeamId == teamId && m.TaskCriteria.TaskId == taskId)
                .AverageAsync(m => m.Mark1);
        }

        public async Task<List<Location>> GetAllLocationsAsync() => await _context.Locations.ToListAsync();
        public async Task AddLocationAsync(Location location) { _context.Locations.Add(location); await _context.SaveChangesAsync(); }
        public async Task<List<StageType>> GetAllStageTypesAsync() => await _context.StageTypes.ToListAsync();
        public async Task AddStageTypeAsync(StageType stageType) { _context.StageTypes.Add(stageType); await _context.SaveChangesAsync(); }
    }
}
  