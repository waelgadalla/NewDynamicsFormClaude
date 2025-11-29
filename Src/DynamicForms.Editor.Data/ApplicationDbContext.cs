using DynamicForms.Editor.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DynamicForms.Editor.Data;

/// <summary>
/// Entity Framework Core database context for the Visual Form Editor.
/// Manages all database operations for editor and published data.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext.
    /// </summary>
    /// <param name="options">Database context options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // ========================================================================
    // EDITOR TABLES (Draft/Working Data)
    // ========================================================================

    /// <summary>
    /// Form modules being edited (draft versions)
    /// </summary>
    public DbSet<EditorFormModule> EditorFormModules { get; set; } = null!;

    /// <summary>
    /// Multi-module workflows being edited (draft versions)
    /// </summary>
    public DbSet<EditorWorkflow> EditorWorkflows { get; set; } = null!;

    /// <summary>
    /// Undo/redo history snapshots for editor sessions
    /// </summary>
    public DbSet<EditorHistorySnapshot> EditorHistory { get; set; } = null!;

    // ========================================================================
    // PRODUCTION TABLES (Published Data)
    // ========================================================================

    /// <summary>
    /// Published form modules (production-ready versions)
    /// </summary>
    public DbSet<PublishedFormModule> PublishedFormModules { get; set; } = null!;

    /// <summary>
    /// Published workflows (production-ready versions)
    /// </summary>
    public DbSet<PublishedWorkflow> PublishedWorkflows { get; set; } = null!;

    // ========================================================================
    // CONFIGURATION
    // ========================================================================

    /// <summary>
    /// Editor configuration settings
    /// </summary>
    public DbSet<EditorConfigurationItem> EditorConfiguration { get; set; } = null!;

    // ========================================================================
    // MODEL CONFIGURATION
    // ========================================================================

    /// <summary>
    /// Configures the entity models using Fluent API.
    /// This ensures the EF Core model matches the database schema exactly.
    /// </summary>
    /// <param name="modelBuilder">Model builder</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureEditorFormModules(modelBuilder);
        ConfigureEditorWorkflows(modelBuilder);
        ConfigureEditorHistory(modelBuilder);
        ConfigurePublishedFormModules(modelBuilder);
        ConfigurePublishedWorkflows(modelBuilder);
        ConfigureEditorConfiguration(modelBuilder);
    }

    /// <summary>
    /// Configures the EditorFormModules entity.
    /// </summary>
    private static void ConfigureEditorFormModules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EditorFormModule>(entity =>
        {
            // Table name
            entity.ToTable("EditorFormModules");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.ModuleId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TitleFr)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.DescriptionFr)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.SchemaJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.Version)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Draft");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ModifiedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(256);

            // Indexes
            entity.HasIndex(e => e.ModuleId)
                .HasDatabaseName("IX_EditorFormModules_ModuleId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_EditorFormModules_Status");

            entity.HasIndex(e => e.ModifiedAt)
                .HasDatabaseName("IX_EditorFormModules_ModifiedAt")
                .IsDescending();
        });
    }

    /// <summary>
    /// Configures the EditorWorkflows entity.
    /// </summary>
    private static void ConfigureEditorWorkflows(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EditorWorkflow>(entity =>
        {
            // Table name
            entity.ToTable("EditorWorkflows");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.WorkflowId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TitleFr)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.SchemaJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.Version)
                .IsRequired()
                .HasDefaultValue(1);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("Draft");

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ModifiedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.ModifiedBy)
                .HasMaxLength(256);

            // Indexes
            entity.HasIndex(e => e.WorkflowId)
                .HasDatabaseName("IX_EditorWorkflows_WorkflowId");

            entity.HasIndex(e => e.Status)
                .HasDatabaseName("IX_EditorWorkflows_Status");
        });
    }

    /// <summary>
    /// Configures the EditorHistory entity.
    /// </summary>
    private static void ConfigureEditorHistory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EditorHistorySnapshot>(entity =>
        {
            // Table name
            entity.ToTable("EditorHistory");

            // Primary key (BIGINT for potentially large history)
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.EditorSessionId)
                .IsRequired();

            entity.Property(e => e.EntityType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EntityId)
                .IsRequired();

            entity.Property(e => e.SnapshotJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.ActionDescription)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.SequenceNumber)
                .IsRequired();

            // Indexes
            entity.HasIndex(e => new { e.EditorSessionId, e.SequenceNumber })
                .HasDatabaseName("IX_EditorHistory_EditorSessionId")
                .IsDescending(false, true); // SequenceNumber DESC for undo operations

            entity.HasIndex(e => new { e.EntityType, e.EntityId })
                .HasDatabaseName("IX_EditorHistory_EntityTypeId");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_EditorHistory_CreatedAt"); // For cleanup queries
        });
    }

    /// <summary>
    /// Configures the PublishedFormModules entity.
    /// </summary>
    private static void ConfigurePublishedFormModules(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PublishedFormModule>(entity =>
        {
            // Table name
            entity.ToTable("PublishedFormModules");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.ModuleId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TitleFr)
                .HasMaxLength(500);

            entity.Property(e => e.SchemaJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.Version)
                .IsRequired();

            entity.Property(e => e.PublishedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.PublishedBy)
                .HasMaxLength(256);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => new { e.ModuleId, e.Version })
                .HasDatabaseName("IX_PublishedFormModules_ModuleId_Version")
                .IsDescending(false, true); // Version DESC for latest version queries

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_PublishedFormModules_IsActive")
                .HasFilter("[IsActive] = 1"); // Filtered index for active records only
        });
    }

    /// <summary>
    /// Configures the PublishedWorkflows entity.
    /// </summary>
    private static void ConfigurePublishedWorkflows(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PublishedWorkflow>(entity =>
        {
            // Table name
            entity.ToTable("PublishedWorkflows");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.WorkflowId)
                .IsRequired();

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.TitleFr)
                .HasMaxLength(500);

            entity.Property(e => e.SchemaJson)
                .IsRequired()
                .HasColumnType("NVARCHAR(MAX)");

            entity.Property(e => e.Version)
                .IsRequired();

            entity.Property(e => e.PublishedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.PublishedBy)
                .HasMaxLength(256);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Indexes
            entity.HasIndex(e => new { e.WorkflowId, e.Version })
                .HasDatabaseName("IX_PublishedWorkflows_WorkflowId_Version")
                .IsDescending(false, true); // Version DESC

            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_PublishedWorkflows_IsActive")
                .HasFilter("[IsActive] = 1");
        });
    }

    /// <summary>
    /// Configures the EditorConfiguration entity.
    /// </summary>
    private static void ConfigureEditorConfiguration(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EditorConfigurationItem>(entity =>
        {
            // Table name
            entity.ToTable("EditorConfiguration");

            // Primary key
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd();

            // Required properties with column types
            entity.Property(e => e.ConfigKey)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.ConfigValue)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.ConfigType)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(500);

            entity.Property(e => e.ModifiedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on ConfigKey
            entity.HasIndex(e => e.ConfigKey)
                .IsUnique()
                .HasDatabaseName("IX_EditorConfiguration_ConfigKey");
        });
    }

    /// <summary>
    /// Saves changes to the database, automatically updating ModifiedAt timestamps.
    /// </summary>
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    /// <summary>
    /// Saves changes to the database asynchronously, automatically updating ModifiedAt timestamps.
    /// </summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Updates ModifiedAt timestamps for modified entities.
    /// </summary>
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            // Update ModifiedAt for entities that have this property
            if (entry.Entity is EditorFormModule formModule)
            {
                formModule.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is EditorWorkflow workflow)
            {
                workflow.ModifiedAt = DateTime.UtcNow;
            }
            else if (entry.Entity is EditorConfigurationItem config)
            {
                config.ModifiedAt = DateTime.UtcNow;
            }
        }
    }
}
