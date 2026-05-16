using System;
using System.Collections.Generic;
using System.ComponentModel;
using HakatonApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace HakatonApplication.Context;

public partial class HakatonDbContext : DbContext
{
    public HakatonDbContext(DbContextOptions<HakatonDbContext> options) : base(options) { }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Criterion> Criteria { get; set; }

    public virtual DbSet<Hakaton> Hakatons { get; set; }

    public virtual DbSet<HakatonFeedback> HakatonFeedbacks { get; set; }

    public virtual DbSet<HakatonNomination> HakatonNominations { get; set; }

    public virtual DbSet<HakatonRegistration> HakatonRegistrations { get; set; }

    public virtual DbSet<HakatonSponsorsSummary> HakatonSponsorsSummaries { get; set; }

    public virtual DbSet<HakatonType> HakatonTypes { get; set; }

    public virtual DbSet<HakatonWinner> HakatonWinners { get; set; }

    public virtual DbSet<Location> Locations { get; set; }

    public virtual DbSet<Mark> Marks { get; set; }

    public virtual DbSet<Nomination> Nominations { get; set; }

    public virtual DbSet<OverdueTeam> OverdueTeams { get; set; }

    public virtual DbSet<PrizeFund> PrizeFunds { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Solution> Solutions { get; set; }

    public virtual DbSet<SolutionFeedback> SolutionFeedbacks { get; set; }

    public virtual DbSet<Sponsor> Sponsors { get; set; }

    public virtual DbSet<SponsorContribution> SponsorContributions { get; set; }

    public virtual DbSet<Stage> Stages { get; set; }

    public virtual DbSet<StageType> StageTypes { get; set; }

    public virtual DbSet<StageTask> Tasks { get; set; }

    public virtual DbSet<TaskCriterion> TaskCriteria { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<TeamHistory> TeamHistories { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRegistration> UserRegistrations { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=postgres;Username=postgres;Password=1111");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pgcrypto");

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("contact_pkey");

            entity.ToTable("contact");

            entity.HasIndex(e => e.Email, "contact_email_key").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "contact_phone_number_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(128)
                .HasColumnName("email");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<Criterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("criteria_pkey");

            entity.ToTable("criteria");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Hakaton>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hakaton_pkey");

            entity.ToTable("hakaton");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            entity.HasMany(d => d.HakatonTypes).WithMany(p => p.Hakatons)
                .UsingEntity<Dictionary<string, object>>(
                    "HakatonTag",
                    r => r.HasOne<HakatonType>().WithMany()
                        .HasForeignKey("HakatonTypeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("hakaton_tags_hakaton_type_id_fkey"),
                    l => l.HasOne<Hakaton>().WithMany()
                        .HasForeignKey("HakatonId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("hakaton_tags_hakaton_id_fkey"),
                    j =>
                    {
                        j.HasKey("HakatonId", "HakatonTypeId").HasName("hakaton_tags_pkey");
                        j.ToTable("hakaton_tags");
                        j.IndexerProperty<int>("HakatonId").HasColumnName("hakaton_id");
                        j.IndexerProperty<int>("HakatonTypeId").HasColumnName("hakaton_type_id");
                    });
        });

        modelBuilder.Entity<HakatonFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("feedback_pkey");

            entity.ToTable("hakaton_feedback");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("nextval('feedback_id_seq'::regclass)")
                .HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.Rating)
                .HasPrecision(10, 2)
                .HasColumnName("rating");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.HakatonFeedbacks)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feedback_hakaton_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.HakatonFeedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feedback_user_id_fkey");
        });

        modelBuilder.Entity<HakatonNomination>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hakaton_nomination_pkey");

            entity.ToTable("hakaton_nomination");

            entity.HasIndex(e => new { e.HakatonId, e.NominationId }, "hakaton_nomination_hakaton_id_nomination_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.NominationId).HasColumnName("nomination_id");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.HakatonNominations)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_nomination_hakaton_id_fkey");

            entity.HasOne(d => d.Nomination).WithMany(p => p.HakatonNominations)
                .HasForeignKey(d => d.NominationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_nomination_nomination_id_fkey");
        });

        modelBuilder.Entity<HakatonRegistration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hakaton_registration_pkey");

            entity.ToTable("hakaton_registration");

            entity.HasIndex(e => new { e.HakatonId, e.UserId }, "hakaton_registration_hakaton_id_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.RegistrationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("registration_date");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.HakatonRegistrations)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_registration_hakaton_id_fkey");

            entity.HasOne(d => d.Role).WithMany(p => p.HakatonRegistrations)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("hakaton_registration_role_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.HakatonRegistrations)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_registration_user_id_fkey");

            entity.HasMany(d => d.Teams).WithMany(p => p.Registrations)
                .UsingEntity<Dictionary<string, object>>(
                    "TeamList",
                    r => r.HasOne<Team>().WithMany()
                        .HasForeignKey("TeamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("team_list_team_id_fkey"),
                    l => l.HasOne<HakatonRegistration>().WithMany()
                        .HasForeignKey("RegistrationId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("team_list_registration_id_fkey"),
                    j =>
                    {
                        j.HasKey("RegistrationId", "TeamId").HasName("team_list_pkey");
                        j.ToTable("team_list");
                        j.IndexerProperty<int>("RegistrationId").HasColumnName("registration_id");
                        j.IndexerProperty<int>("TeamId").HasColumnName("team_id");
                    });
        });

        modelBuilder.Entity<HakatonSponsorsSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("hakaton_sponsors_summary");

            entity.Property(e => e.HakatonName)
                .HasMaxLength(45)
                .HasColumnName("hakaton_name");
            entity.Property(e => e.SponsorName)
                .HasMaxLength(45)
                .HasColumnName("sponsor_name");
            entity.Property(e => e.SponsoredNominations).HasColumnName("sponsored_nominations");
            entity.Property(e => e.TotalContribution).HasColumnName("total_contribution");
        });

        modelBuilder.Entity<HakatonType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("hakaton_type_pkey");

            entity.ToTable("hakaton_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(45)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<HakatonWinner>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("hakaton_winners");

            entity.HasIndex(e => e.HakatonNominationId, "hakaton_nomination_unique").IsUnique();

            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HakatonNominationId).HasColumnName("hakaton_nomination_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.HakatonNomination).WithOne()
                .HasForeignKey<HakatonWinner>(d => d.HakatonNominationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_winners_hakaton_nomination_fk");

            entity.HasOne(d => d.Team).WithMany()
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("hakaton_winners_team_fk");
        });

        modelBuilder.Entity<Location>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("location_pkey");

            entity.ToTable("location");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            entity.HasOne(d => d.Contact).WithMany(p => p.Locations)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("location_contact_id_fkey");
        });

        modelBuilder.Entity<Mark>(entity =>
        {
            entity.HasKey(e => new { e.TaskCriteriaId, e.TeamId, e.RegistrationId }).HasName("mark_pkey");

            entity.ToTable("mark");

            entity.Property(e => e.TaskCriteriaId).HasColumnName("task_criteria_id");
            entity.Property(e => e.TeamId).HasColumnName("team_id");
            entity.Property(e => e.RegistrationId).HasColumnName("registration_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Mark1)
                .HasPrecision(5, 2)
                .HasColumnName("mark");

            entity.HasOne(d => d.Registration).WithMany(p => p.Marks)
                .HasForeignKey(d => d.RegistrationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mark_registration_id_fkey");

            entity.HasOne(d => d.TaskCriteria).WithMany(p => p.Marks)
                .HasForeignKey(d => d.TaskCriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mark_task_criteria_id_fkey");

            entity.HasOne(d => d.Team).WithMany(p => p.Marks)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mark_team_id_fkey");
        });

        modelBuilder.Entity<Nomination>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("nomination_pkey");

            entity.ToTable("nomination");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<OverdueTeam>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("overdue_teams");

            entity.Property(e => e.DeliveryDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("delivery_date");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.Hakaton)
                .HasMaxLength(45)
                .HasColumnName("hakaton");
            entity.Property(e => e.Overdue).HasColumnName("overdue");
            entity.Property(e => e.Solution)
                .HasMaxLength(45)
                .HasColumnName("solution");
            entity.Property(e => e.Team)
                .HasMaxLength(45)
                .HasColumnName("team");
        });

        modelBuilder.Entity<PrizeFund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("prize_fund_pkey");

            entity.ToTable("prize_fund");

            entity.HasIndex(e => new { e.HakatonNominationId, e.Place }, "prize_fund_hakaton_nomination_id_place_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HakatonNominationId).HasColumnName("hakaton_nomination_id");
            entity.Property(e => e.Place).HasColumnName("place");

            entity.HasOne(d => d.HakatonNomination).WithMany(p => p.PrizeFunds)
                .HasForeignKey(d => d.HakatonNominationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prize_fund_hakaton_nomination_id_fkey");

            entity.HasMany(d => d.Contributions).WithMany(p => p.Prizes)
                .UsingEntity<Dictionary<string, object>>(
                    "PrizeSponsor",
                    r => r.HasOne<SponsorContribution>().WithMany()
                        .HasForeignKey("ContributionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("prize_sponsor_contribution_id_fkey"),
                    l => l.HasOne<PrizeFund>().WithMany()
                        .HasForeignKey("PrizeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("prize_sponsor_prize_id_fkey"),
                    j =>
                    {
                        j.HasKey("PrizeId", "ContributionId").HasName("prize_sponsor_pkey");
                        j.ToTable("prize_sponsor");
                        j.IndexerProperty<int>("PrizeId").HasColumnName("prize_id");
                        j.IndexerProperty<int>("ContributionId").HasColumnName("contribution_id");
                    });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("role_pkey");

            entity.ToTable("role");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Solution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("solution_pkey");

            entity.ToTable("solution");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DeliveryDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("delivery_date");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");
            entity.Property(e => e.Source).HasColumnName("source");
            entity.Property(e => e.TeamId).HasColumnName("team_id");

            entity.HasOne(d => d.Team).WithMany(p => p.Solutions)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solution_team_id_fkey");

            entity.HasMany(d => d.Tasks).WithMany(p => p.Solutions)
                .UsingEntity<Dictionary<string, object>>(
                    "TaskSolution",
                    r => r.HasOne<StageTask>().WithMany()
                        .HasForeignKey("TaskId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("task_solution_task_id_fkey"),
                    l => l.HasOne<Solution>().WithMany()
                        .HasForeignKey("SolutionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("task_solution_solution_id_fkey"),
                    j =>
                    {
                        j.HasKey("SolutionId", "TaskId").HasName("task_solution_pkey");
                        j.ToTable("task_solution");
                        j.IndexerProperty<int>("SolutionId").HasColumnName("solution_id");
                        j.IndexerProperty<int>("TaskId").HasColumnName("task_id");
                    });
        });

        modelBuilder.Entity<SolutionFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("solution_feedback_pkey");

            entity.ToTable("solution_feedback");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Rating)
                .HasPrecision(10, 2)
                .HasColumnName("rating");
            entity.Property(e => e.SolutionId).HasColumnName("solution_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Solution).WithMany(p => p.SolutionFeedbacks)
                .HasForeignKey(d => d.SolutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solution_feedback_solution_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.SolutionFeedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("solution_feedback_user_id_fkey");
        });

        modelBuilder.Entity<Sponsor>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sponsor_pkey");

            entity.ToTable("sponsor");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            entity.HasOne(d => d.Contact).WithMany(p => p.Sponsors)
                .HasForeignKey(d => d.ContactId)
                .HasConstraintName("sponsor_contact_id_fkey");
        });

        modelBuilder.Entity<SponsorContribution>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sponsor_contribution_pkey");

            entity.ToTable("sponsor_contribution");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Additional).HasColumnName("additional");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.Money)
                .HasPrecision(10, 2)
                .HasColumnName("money");
            entity.Property(e => e.SponsorId).HasColumnName("sponsor_id");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.SponsorContributions)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("sponsor_contribution_hakaton_id_fkey");

            entity.HasOne(d => d.Sponsor).WithMany(p => p.SponsorContributions)
                .HasForeignKey(d => d.SponsorId)
                .HasConstraintName("sponsor_contribution_sponsor_id_fkey");
        });

        modelBuilder.Entity<Stage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stage_pkey");

            entity.ToTable("stage");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EndDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("end_date");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.LocationId).HasColumnName("location_id");
            entity.Property(e => e.OrderNumber).HasColumnName("order_number");
            entity.Property(e => e.StageTypeId).HasColumnName("stage_type_id");
            entity.Property(e => e.StartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("start_date");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.Stages)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("stage_hakaton_id_fkey");

            entity.HasOne(d => d.Location).WithMany(p => p.Stages)
                .HasForeignKey(d => d.LocationId)
                .HasConstraintName("stage_location_id_fkey");

            entity.HasOne(d => d.StageType).WithMany(p => p.Stages)
                .HasForeignKey(d => d.StageTypeId)
                .HasConstraintName("stage_stage_type_id_fkey");
        });

        modelBuilder.Entity<StageType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stage_type_pkey");

            entity.ToTable("stage_type");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.StageType1)
                .HasMaxLength(45)
                .HasColumnName("stage_type");
        });

        modelBuilder.Entity<StageTask>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_pkey");

            entity.ToTable("task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsSolutionsPublic).HasColumnName("is_solutions_public");
            entity.Property(e => e.StageId).HasColumnName("stage_id");

            entity.HasOne(d => d.Stage).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.StageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_stage_id_fkey");
        });

        modelBuilder.Entity<TaskCriterion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_criteria_pkey");

            entity.ToTable("task_criteria");

            entity.HasIndex(e => new { e.TaskId, e.CriteriaId }, "task_criteria_task_id_criteria_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CriteriaId).HasColumnName("criteria_id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.MaxMark)
                .HasPrecision(5, 2)
                .HasColumnName("max_mark");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Criteria).WithMany(p => p.TaskCriteria)
                .HasForeignKey(d => d.CriteriaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_criteria_criteria_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.TaskCriteria)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_criteria_task_id_fkey");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("team_pkey");

            entity.ToTable("team");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.HakatonId).HasColumnName("hakaton_id");
            entity.Property(e => e.Name)
                .HasMaxLength(45)
                .HasColumnName("name");

            entity.HasOne(d => d.Hakaton).WithMany(p => p.Teams)
                .HasForeignKey(d => d.HakatonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("team_hakaton_id_fkey");
        });

        modelBuilder.Entity<TeamHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("team_history_pkey");

            entity.ToTable("team_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(20)
                .HasColumnName("action");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changed_at");
            entity.Property(e => e.NewTeamId).HasColumnName("new_team_id");
            entity.Property(e => e.OldTeamId).HasColumnName("old_team_id");
            entity.Property(e => e.RegistrationId).HasColumnName("registration_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.ContactId, "users_contact_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactId).HasColumnName("contact_id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(45)
                .HasColumnName("first_name");
            entity.Property(e => e.IsPublic).HasColumnName("is_public");
            entity.Property(e => e.IsAdmin).HasColumnName("is_admin");
            entity.Property(e => e.LastName)
                .HasMaxLength(45)
                .HasColumnName("last_name");
            entity.Property(e => e.Password)
                .HasMaxLength(64)
                .HasColumnName("password");
            entity.Property(e => e.Patronymic)
                .HasMaxLength(45)
                .HasColumnName("patronymic");
            entity.Property(e => e.RegistrationDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("registration_date");
            entity.Property(e => e.Salt)
                .HasMaxLength(16)
                .HasColumnName("salt");

            entity.HasOne(d => d.Contact).WithOne(p => p.User)
                .HasForeignKey<User>(d => d.ContactId)
                .HasConstraintName("users_contact_id_fkey");
        });

        modelBuilder.Entity<UserRegistration>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("user_registration");

            entity.Property(e => e.Hakaton)
                .HasMaxLength(45)
                .HasColumnName("hakaton");
            entity.Property(e => e.Team)
                .HasMaxLength(45)
                .HasColumnName("team");
            entity.Property(e => e.User).HasColumnName("user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
