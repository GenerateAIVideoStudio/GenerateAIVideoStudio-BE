namespace Infrastructure.Persistence;

using Domain.Common;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterAppearance> CharacterAppearances => Set<CharacterAppearance>();
    public DbSet<CharacterPersonality> CharacterPersonalities => Set<CharacterPersonality>();
    public DbSet<CharacterVoice> CharacterVoices => Set<CharacterVoice>();
    public DbSet<CharacterBible> CharacterBibles => Set<CharacterBible>();
    public DbSet<CharacterMemory> CharacterMemories => Set<CharacterMemory>();
    public DbSet<CharacterRelationship> CharacterRelationships => Set<CharacterRelationship>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Scene> Scenes => Set<Scene>();
    public DbSet<SceneCharacter> SceneCharacters => Set<SceneCharacter>();
    public DbSet<Storyboard> Storyboards => Set<Storyboard>();
    public DbSet<SceneVideo> SceneVideos => Set<SceneVideo>();
    public DbSet<SceneVoice> SceneVoices => Set<SceneVoice>();
    public DbSet<FinalVideo> FinalVideos => Set<FinalVideo>();
    public DbSet<AiGeneration> AiGenerations => Set<AiGeneration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
                entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
        return base.SaveChangesAsync(ct);
    }
}
