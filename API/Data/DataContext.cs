using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext(DbContextOptions<DataContext> options) : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>(options)
{
    //IdentityDbContext comes with a DbSet for users
    // public DbSet<AppUser> Users { get; set; }
    public DbSet<UserLike> Likes { get; set; }

    public DbSet<Message> Messages { get; set; } 
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>()
            .HasMany(ur => ur.UserRoles)//One AppUser can have many AppUserRoles
            .WithOne(u => u.User) //Each AppUserRole is linked to one AppUser
            .HasForeignKey(ur => ur.UserId)
            .IsRequired();
        
        builder.Entity<AppRole>()
            .HasMany(ur => ur.UserRoles) //One AppRole can have many AppUserRoles
            .WithOne(u => u.Role) //Each AppUserRole is linked to one AppRole
            .HasForeignKey(ur => ur.RoleId)
            .IsRequired();

        //Likes (many to many)
        builder.Entity<UserLike>()
            .HasKey(k => new { k.SourceUserId, k.TargetUserId }); //Sets primary key (composite)
        
        //(this configures the navigation properties)
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

            //Messages (many to many) (this configures the navigation properties)
            builder.Entity<Message>()
                .HasOne(x => x.Recipient)
                .WithMany(x => x.MessagesReceived)
                .HasForeignKey(x => x.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.Entity<Message>()
                .HasOne(x => x.Sender)
                .WithMany(x => x.MessagesSent)
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict);
        
        
    }
}