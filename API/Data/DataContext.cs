using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetUserId }); //Sets primary key (composite)
        
        builder.Entity<UserLike>()
            .HasOne(u => u.SourceUser) //the UserLike entity has a single SourceUser (the user who is doing the "liking").
            .WithMany(l => l.LikedUsers) //an AppUser can have many UserLike entities where they are the SourceUser (i.e., the user liking others).
            .HasForeignKey(u => u.SourceUserId) //Configures SourceUserId as the foreign key for this relationship.
            .OnDelete(DeleteBehavior.Cascade); //when a SourceUser is deleted, all associated UserLike records should also be deleted automatically.
        
        builder.Entity<UserLike>()
            .HasOne(u => u.TargetUser) //the UserLike entity has a single TargetUser (the user who is being liked).
            .WithMany(l => l.LikedByUsers) //an AppUser can have many UserLike entities where they are the TargetUser (i.e., the user being liked).
            .HasForeignKey(u => u.TargetUserId) //Configures TargetUserId as the foreign key for this relationship.
            .OnDelete(DeleteBehavior.Cascade); //when a TargetUser is deleted, all associated UserLike records should also be deleted automatically.

    }
}