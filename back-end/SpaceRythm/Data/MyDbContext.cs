using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Entities;
using SpaceRythm.Models;
using SpaceRythm.Models_DTOs.Role;


namespace SpaceRythm.Data;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Track> Tracks { get; set; }
    public DbSet<Artist> Artists { get; set; }

    public DbSet<Playlist> Playlists { get; set; }

    public DbSet<PlaylistTracks> PlaylistTracks { get; set; }
    public DbSet<TrackMetadata> TrackMetadatas { get; set; }
    public DbSet<Listening> Listenings { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Follower> Followers { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<AdminLog> AdminLogs { get; set; }
    public DbSet<TrackCategory> TrackCategories { get; set; }
    public DbSet<TrackLiked> TrackLiked { get; set; }
    public DbSet<TrackCategoryLink> TrackCategoryLink { get; set; }
    public DbSet<ArtistLiked> ArtistsLiked { get; set; }

    public DbSet<CategoryLiked> CategoriesLiked { get; set; }
    public DbSet<UserListeningHistory> UserListeningHistories { get; set; }
    public DbSet<CommentLike> CommentLikes { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    //public DbSet<IdentityUserRole<string>> UserRoles { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Composite key for PlaylistTracks
        modelBuilder.Entity<PlaylistTracks>()
            .HasKey(pt => new { pt.PlaylistId, pt.TrackId });  // Налаштовуємо складений первинний ключ для PlaylistTracks, де первинним ключем є комбінація PlaylistId та TrackId

        // Composite key for Like
        modelBuilder.Entity<Like>()
            .HasKey(l => new { l.UserId, l.TrackId });  // Налаштовуємо складений первинний ключ для Like, де первинним ключем є комбінація UserId та TrackId

        modelBuilder.Entity<Like>()
            .HasIndex(l => new { l.UserId, l.TrackId })
            .IsUnique();

        // Складений ключ для Follower
        modelBuilder.Entity<Follower>()
            .HasKey(f => new { f.UserId, f.FollowedUserId });

        // Зв’язок: User -> Followers (ті, хто підписався)
        modelBuilder.Entity<Follower>()
            .HasOne(f => f.User)
            .WithMany(u => u.FollowedBy)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Зв’язок: User -> FollowedBy (ті, на кого підписалися)
        modelBuilder.Entity<Follower>()
            .HasOne(f => f.FollowedUser)
            .WithMany(u => u.Followers)
            .HasForeignKey(f => f.FollowedUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Track to Artist relationship
         modelBuilder.Entity<Track>()
            .HasOne(t => t.Artist)
            .WithMany(a => a.Tracks)
            .HasForeignKey(t => t.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure Genre property conversion to string
        modelBuilder.Entity<Track>()
            .Property(t => t.Genre)
            .HasConversion<string>();

        // Configure Track to User relationship
        modelBuilder.Entity<Track>()
            .HasOne(t => t.User) // Each Track has one User
            .WithMany(u => u.Tracks)  // Assuming User can have many Tracks
            .HasForeignKey(t => t.UserId)  // Explicitly specify UserId as the foreign key
            .OnDelete(DeleteBehavior.Cascade);  // Optional: specify delete behavior

        // Composite key for TrackCategoryLink (many-to-many relationship)
        modelBuilder.Entity<TrackCategoryLink>()
            .HasKey(tcl => new { tcl.TrackId, tcl.TrackCategoryId });  // Налаштовуємо складений первинний ключ для TrackCategoryLink, де первинним ключем є комбінація TrackId та TrackCategoryId

        // Configure TrackCategoryLink to Track relationship
        modelBuilder.Entity<TrackCategoryLink>()
            .HasOne(tcl => tcl.Track)  // Кожен TrackCategoryLink має один Track
            .WithMany(t => t.TrackCategoryLink)  // Кожен Track може мати багато зв'язків TrackCategoryLink
            .HasForeignKey(tcl => tcl.TrackId)  // Вказуємо, що TrackId є зовнішнім ключем в таблиці TrackCategoryLink
            .OnDelete(DeleteBehavior.Cascade);  // Якщо Track видалений, то видаляються відповідні записи в таблиці TrackCategoryLink

        // Configure TrackCategoryLink to TrackCategory relationship
        modelBuilder.Entity<TrackCategoryLink>()
            .HasOne(tcl => tcl.TrackCategory)  // Кожен TrackCategoryLink має одну TrackCategory
            .WithMany(tc => tc.TrackCategoryLinks)  // Кожна TrackCategory може мати багато зв'язків TrackCategoryLink
            .HasForeignKey(tcl => tcl.TrackCategoryId)  // Вказуємо, що TrackCategoryId є зовнішнім ключем в таблиці TrackCategoryLink
            .OnDelete(DeleteBehavior.Cascade);  // Якщо TrackCategory видалений, то видаляються відповідні записи в таблиці TrackCategoryLink

        modelBuilder.Entity<TrackCategory>()
            .HasIndex(tc => tc.Category)
            .IsUnique();

        // Configure TrackLiked relationship with Track
        modelBuilder.Entity<TrackLiked>()
            .HasOne(tl => tl.Track)  // Кожен TrackLiked має один Track
            .WithMany(t => t.TracksLiked)  // Кожен Track може мати багато TrackLiked
            .HasForeignKey(tl => tl.TrackId)  // Вказуємо, що TrackId є зовнішнім ключем в таблиці TrackLiked
            .OnDelete(DeleteBehavior.Cascade);  // Якщо Track видалений, то видаляються відповідні записи в таблиці TrackLiked

        // Configure TrackLiked relationship with User
        modelBuilder.Entity<TrackLiked>()
            .HasOne(tl => tl.User)  // Кожен TrackLiked має одного User
            .WithMany(u => u.TracksLiked)  // Кожен User може мати багато TrackLiked
            .HasForeignKey(tl => tl.UserId)  // Вказуємо, що UserId є зовнішнім ключем в таблиці TrackLiked
            .OnDelete(DeleteBehavior.Cascade);  // Якщо User видалений, то видаляються відповідні записи в таблиці TrackLiked

        // Composite key for CategoryLiked
        modelBuilder.Entity<CategoryLiked>()
            .HasKey(cl => new { cl.UserId, cl.CategoryId });  // Налаштовуємо складений первинний ключ для CategoryLiked, де первинним ключем є комбінація UserId та CategoryId

        // Configure CategoryLiked relationship with User
        modelBuilder.Entity<CategoryLiked>()
            .HasOne(cl => cl.User)  // Кожен CategoryLiked має одного User
            .WithMany(u => u.CategoriesLiked)  // Кожен User може мати багато CategoryLiked
            .HasForeignKey(cl => cl.UserId)  // Вказуємо, що UserId є зовнішнім ключем в таблиці CategoryLiked
            .OnDelete(DeleteBehavior.Cascade);  // Якщо User видалений, то видаляються відповідні записи в таблиці CategoryLiked

        // Configure CategoryLiked relationship with TrackCategory
        modelBuilder.Entity<CategoryLiked>()
            .HasOne(cl => cl.TrackCategory)  // Кожен CategoryLiked має одну TrackCategory
            .WithMany(tc => tc.CategoriesLiked)  // Кожна TrackCategory може мати багато CategoryLiked
            .HasForeignKey(cl => cl.CategoryId)  // Вказуємо, що CategoryId є зовнішнім ключем в таблиці CategoryLiked
            .OnDelete(DeleteBehavior.Cascade);  // Якщо TrackCategory видалений, то видаляються відповідні записи в таблиці CategoryLiked

        // Composite key for ArtistCategoryLink
        modelBuilder.Entity<ArtistCategoryLink>()
            .HasKey(ac => new { ac.ArtistId, ac.CategoryId });  // Налаштовуємо складений первинний ключ для ArtistCategoryLink, де первинним ключем є комбінація ArtistId та CategoryId

        // Configure ArtistCategoryLink relationship with Artist
        modelBuilder.Entity<ArtistCategoryLink>()
            .HasOne(ac => ac.Artist)  // Кожен ArtistCategoryLink має одного Artist
            .WithMany(a => a.ArtistCategoryLinks)  // Кожен Artist може мати багато ArtistCategoryLinks
            .HasForeignKey(ac => ac.ArtistId);  // Вказуємо, що ArtistId є зовнішнім ключем в таблиці ArtistCategoryLink

        // Configure ArtistCategoryLink relationship with Category
        modelBuilder.Entity<ArtistCategoryLink>()
            .HasOne(ac => ac.Category)  // Кожен ArtistCategoryLink має одну Category
            .WithMany(c => c.ArtistCategoryLinks)  // Кожна Category може мати багато ArtistCategoryLinks
            .HasForeignKey(ac => ac.CategoryId);

        modelBuilder.Entity<Listening>()
            .HasOne(l => l.Track)  // Listening має один Track
            .WithMany()  // Track не має навігаційного властивості для Listening
            .HasForeignKey(l => l.TrackId)  // Зовнішній ключ в Listening
            .OnDelete(DeleteBehavior.Cascade);  // Якщо трек видалено, то видаляються відповідні записи Listening

        // Зв'язок між Listening і User
        modelBuilder.Entity<Listening>()
            .HasOne(l => l.User)  // Listening має одного User
            .WithMany()  // User не має навігаційного властивості для Listening
            .HasForeignKey(l => l.UserId)  // Зовнішній ключ в Listening
            .OnDelete(DeleteBehavior.Cascade);  // Якщо користувач видалений, то видаляються відповідні записи Listening

        // Зв'язок між Playlist і User
        modelBuilder.Entity<Playlist>()
            .HasOne(p => p.User)  // Playlist має одного User
            .WithMany(u => u.Playlists)  // Користувач може мати багато плейлистів
            .HasForeignKey(p => p.UserId)  // Зовнішній ключ в Playlist
            .OnDelete(DeleteBehavior.Cascade);  // Якщо користувач видалений, то видаляються відповідні плейлисти

        // Зв'язок між PlaylistTracks і Playlist
        modelBuilder.Entity<PlaylistTracks>()
            .HasOne(pt => pt.Playlist)  // PlaylistTracks має один Playlist
            .WithMany(p => p.PlaylistTracks)  // Playlist може мати багато записів в PlaylistTracks
            .HasForeignKey(pt => pt.PlaylistId)  // Зовнішній ключ в PlaylistTracks
            .OnDelete(DeleteBehavior.Cascade);  // Якщо плейлист видалений, видаляються записи PlaylistTracks

        // Зв'язок між PlaylistTracks і Track
        modelBuilder.Entity<PlaylistTracks>()
            .HasOne(pt => pt.Track)  // PlaylistTracks має один Track
            .WithMany(t => t.PlaylistTracks)  // Track може бути в багатьох PlaylistTracks
            .HasForeignKey(pt => pt.TrackId)  // Зовнішній ключ в PlaylistTracks
            .OnDelete(DeleteBehavior.Cascade);  // Якщо трек видалений, то видаляються записи PlaylistTracks

        // Встановлення складового ключа
        modelBuilder.Entity<CommentLike>()
            .HasKey(cl => new { cl.UserId, cl.CommentId });

        // Додавання унікального індексу
        modelBuilder.Entity<CommentLike>()
            .HasIndex(cl => new { cl.UserId, cl.CommentId })
            .IsUnique();

        // Налаштування зв’язку між User і Role через UserRole
        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.UserId, ur.RoleId });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        // Початкові дані (опціонально)
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Admin" },
            new Role { Id = 2, Name = "User" }
        );

        //// Додаткові налаштування (за потреби)
        //modelBuilder.Entity<CommentLike>()
        //    .HasOne(cl => cl.User)
        //    .WithMany(u => u.CommentLikes)
        //    .HasForeignKey(cl => cl.UserId);

        //modelBuilder.Entity<CommentLike>()
        //    .HasOne(cl => cl.Comment)
        //    .WithMany(c => c.CommentLikes)
        //    .HasForeignKey(cl => cl.CommentId);



        //Seed(modelBuilder);
    }

    //   private void Seed(ModelBuilder modelBuilder)
    //   {
    //       
    //   }
}
