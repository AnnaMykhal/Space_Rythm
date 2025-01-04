using SpaceRythm.Entities;
using Microsoft.EntityFrameworkCore;
using SpaceRythm.Interfaces;
using SpaceRythm.Models.Track;
using SpaceRythm.Util;
using SpaceRythm.Services;
using ActiveUp.Net.Security.OpenPGP.Packets;
using System.Diagnostics;
using SpaceRythm.Models_DTOs;


namespace SpaceRythm.Data;

public class DbInitializer
{
    private readonly IDbContextFactory<MyDbContext> _contextFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ITrackService _trackService;
    private readonly IUserService _userService;
    private readonly IListeningService _listeningService;
    public DbInitializer(
        IDbContextFactory<MyDbContext> contextFactory,
        IWebHostEnvironment environment,
        ITrackService trackService,
        IUserService userService,
        IListeningService listeningService)
    {
        _contextFactory = contextFactory;
        _environment = environment;
        _trackService = trackService;
        _userService = userService;
        _listeningService = listeningService;
    }


    public async Task Initialize()
    {
        using var context = _contextFactory.CreateDbContext();
        var users = new List<Entities.User>();
        var tracks = new List<PlainTrackResponse>();

        // Створення тестових користувачів
        if (!context.Users.Any())
        {
            var wwwRootPath = "wwwroot";

            //user1
            var filePath1 = Path.Combine(wwwRootPath, "avatars", "testuser1.jpg");

            if (!File.Exists(filePath1))
            {
                throw new FileNotFoundException("Avatar file not found", filePath1);
            }

            using var imageStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read);

            var user1 = new Entities.User
            {
                Email = "test1@example.com",
                Username = "testuser1",
                Password = PasswordHash.Hash("password123"),
                IsEmailConfirmed = true,
                DateJoined = DateTime.UtcNow,
                Biography = "This is test user 1",
                Country = "Ukraine",
                City = "Kyiv"
            };

            await context.Users.AddRangeAsync(user1);
            await context.SaveChangesAsync();
            users.Add(user1);

            var userId1 = user1.Id; 

            var profileImage1 = new FormFile(imageStream1, 0, imageStream1.Length, "file", "testuser1.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" 
            };

            var profileImagePath1 = await _userService.SaveAndUploadAvatarAsync(userId1, avatarFile: profileImage1, wwwRootPath);

            //user2
            var filePath2 = Path.Combine(wwwRootPath, "avatars", "testuser2.jpg");
            if (!File.Exists(filePath2))
            {
                throw new FileNotFoundException("Avatar file not found", filePath2);
            }
            using var imageStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);
            var user2 = new Entities.User
            {
                Email = "test2@example.com",
                Username = "testuser2",
                Password = PasswordHash.Hash("password456"),
                IsEmailConfirmed = true,
                DateJoined = DateTime.UtcNow,
                Biography = "This is test user 2",
                Country = "USA",
                City = "New York"
            };
            await context.Users.AddRangeAsync(user2);
            await context.SaveChangesAsync();
            users.Add(user2);

            var userId2 = user2.Id;
            var profileImage2 = new FormFile(imageStream2, 0, imageStream2.Length, "file", "testuser2.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg" 
            };

            var profileImagePath2 = await _userService.SaveAndUploadAvatarAsync(userId2, avatarFile: profileImage2, wwwRootPath);


            //user3
            var filePath3 = Path.Combine(wwwRootPath, "avatars", "testuser3.jpg");
            if (!File.Exists(filePath3))
            {
                throw new FileNotFoundException("Avatar file not found", filePath3);
            }
            using var imageStream3 = new FileStream(filePath3, FileMode.Open, FileAccess.Read);
            
            var user3 = new Entities.User
            {
                Email = "test3@example.com",
                Username = "testuser3",
                Password = PasswordHash.Hash("password789"),
                IsEmailConfirmed = true,
                DateJoined = DateTime.UtcNow,
                Biography = "This is test user 3",
                Country = "UK",
                City = "London"
            };
            await context.Users.AddRangeAsync(user3);
            await context.SaveChangesAsync();
            users.Add(user3);

            var userId3 = user3.Id;
            var profileImage3 = new FormFile(imageStream3, 0, imageStream3.Length, "file", "testuser3.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var profileImagePath3 = await _userService.SaveAndUploadAvatarAsync(userId3, avatarFile: profileImage3, wwwRootPath);


            //user4
            var filePath4 = Path.Combine(wwwRootPath, "avatars", "testuser4.jpg");
            if (!File.Exists(filePath4))
            {
                throw new FileNotFoundException("Avatar file not found", filePath4);
            }
            using var imageStream4 = new FileStream(filePath4, FileMode.Open, FileAccess.Read);

            var user4 = new Entities.User
            {
                Email = "test4@example.com",
                Username = "testuser4",
                Password = PasswordHash.Hash("password123"),
                IsEmailConfirmed = true,
                DateJoined = DateTime.UtcNow,
                Biography = "This is test user 4",
                Country = "Ukraine",
                City = "Zhytomyr"
            };
            await context.Users.AddRangeAsync(user4);
            await context.SaveChangesAsync();
            users.Add(user4);

            var userId4 = user4.Id;
            var profileImage4 = new FormFile(imageStream4, 0, imageStream4.Length, "file", "testuser4.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var profileImagePath4 = await _userService.SaveAndUploadAvatarAsync(userId4, avatarFile: profileImage4, wwwRootPath);


            //user5
            var filePath5 = Path.Combine(wwwRootPath, "avatars", "testuser5.jpg");
            if (!File.Exists(filePath4))
            {
                throw new FileNotFoundException("Avatar file not found", filePath5);
            }
            using var imageStream5 = new FileStream(filePath5, FileMode.Open, FileAccess.Read);

            var user5 = new Entities.User
            {
                Email = "test5@example.com",
                Username = "testuser5",
                Password = PasswordHash.Hash("password456"),
                IsEmailConfirmed = true,
                DateJoined = DateTime.UtcNow,
                Biography = "This is test user 5",
                Country = "Ukraine",
                City = "Odessa"
            };
            await context.Users.AddRangeAsync(user5);
            await context.SaveChangesAsync();
            users.Add(user5);

            var userId5 = user5.Id;
            var profileImage5 = new FormFile(imageStream5, 0, imageStream5.Length, "file", "testuser5.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };

            var profileImagePath5 = await _userService.SaveAndUploadAvatarAsync(userId5, avatarFile: profileImage5, wwwRootPath);;

            await context.SaveChangesAsync();


            if (!context.Followers.Any())
            {
                // Створення тестових підписок
                var follow1 = new Entities.Follower
                {
                    UserId = user1.Id,
                    FollowedUserId = user2.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow2 = new Entities.Follower
                {
                    UserId = user2.Id,
                    FollowedUserId = user3.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow3 = new Entities.Follower
                {
                    UserId = user3.Id,
                    FollowedUserId = user1.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow4 = new Entities.Follower
                {
                    UserId = user1.Id,
                    FollowedUserId = user3.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow5 = new Entities.Follower
                {
                    UserId = user4.Id,
                    FollowedUserId = user1.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow6 = new Entities.Follower
                {
                    UserId = user5.Id,
                    FollowedUserId = user2.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow7 = new Entities.Follower
                {
                    UserId = user1.Id,
                    FollowedUserId = user5.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow8 = new Entities.Follower
                {
                    UserId = user4.Id,
                    FollowedUserId = user2.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow9 = new Entities.Follower
                {
                    UserId = user4.Id,
                    FollowedUserId = user3.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow10 = new Entities.Follower
                {
                    UserId = user2.Id,
                    FollowedUserId = user4.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow11 = new Entities.Follower
                {
                    UserId = user5.Id,
                    FollowedUserId = user4.Id,
                    FollowDate = DateTime.UtcNow
                };

                var follow12 = new Entities.Follower
                {
                    UserId = user5.Id,
                    FollowedUserId = user3.Id,
                    FollowDate = DateTime.UtcNow
                };

                await context.Followers.AddRangeAsync(follow1, follow2, follow3, follow4, follow5, follow6, follow7, follow8, follow9, follow10, follow11, follow12);
                await context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine("Followers table already contains data. Skipping initialization.");
            }
        }
        else
        {
            Console.WriteLine("Users table already contains data. Skipping initialization.");
        }

        // Створення тестових категорій
        if (!context.TrackCategories.Any())
        {
            context.TrackCategories.AddRange(
                new TrackCategory { Id = 1, Category = "Club", ImageUrl = "categoryImages/music.jpg" },
                new TrackCategory { Id = 2, Category = "Podcasts", ImageUrl = "categoryImages/podcast.jpg" },
                new TrackCategory { Id = 3, Category = "Audiobooks", ImageUrl = "categoryImages/audiobook.jpg" },
                new TrackCategory { Id = 4, Category = "Relax", ImageUrl = "categoryImages/relax.jpg" },
                new TrackCategory { Id = 5, Category = "Спорт", ImageUrl = "categoryImages/sport.jpg" },
                new TrackCategory { Id = 6, Category = "Для дітей", ImageUrl = "categoryImages/kids.jpg" },
                new TrackCategory { Id = 7, Category = "Хіти", ImageUrl = "categoryImages/hits.jpg" },
                new TrackCategory { Id = 8, Category = "80-ті", ImageUrl = "categoryImages/80hits.jpg" },
                new TrackCategory { Id = 9, Category = "90-ті", ImageUrl = "categoryImages/90hits.jpg" },
                new TrackCategory { Id = 10, Category = "2000-ні", ImageUrl = "categoryImages/00hits.jpg" },
                new TrackCategory { Id = 11, Category = "2010-ті", ImageUrl = "categoryImages/2010hits.jpg" },
                new TrackCategory { Id = 12, Category = "Dance", ImageUrl = "categoryImages/dance.jpg" },
                new TrackCategory { Id = 13, Category = "Електроніка", ImageUrl = "categoryImages/electronic.jpg" },
                new TrackCategory { Id = 14, Category = "K-pop", ImageUrl = "categoryImages/kpop.jpg" },
                new TrackCategory { Id = 15, Category = "Інді", ImageUrl = "categoryImages/indie.jpg" },
                new TrackCategory { Id = 16, Category = "Альтернатива", ImageUrl = "categoryImages/alternative.jpg" },
                new TrackCategory { Id = 17, Category = "Хіп-хоп", ImageUrl = "categoryImages/hiphop.jpg" },
                new TrackCategory { Id = 18, Category = "Поп", ImageUrl = "categoryImages/pop.jpg" },
                new TrackCategory { Id = 19, Category = "Dj-мікси", ImageUrl = "categoryImages/dj.jpg" },
                new TrackCategory { Id = 20, Category = "Класика року", ImageUrl = "categoryImages/classicRock.jpg" },
                new TrackCategory { Id = 21, Category = "Рок", ImageUrl = "categoryImages/rock.jpg" },
                new TrackCategory { Id = 22, Category = "Джаз", ImageUrl = "categoryImages/jazz.jpg" },
                new TrackCategory { Id = 23, Category = "Метал", ImageUrl = "categoryImages/metal.jpg" },
                new TrackCategory { Id = 24, Category = "Класика", ImageUrl = "categoryImages/classic.jpg" },
                new TrackCategory { Id = 25, Category = "Романтика", ImageUrl = "categoryImages/romantic.jpg" },
                new TrackCategory { Id = 26, Category = "R&B", ImageUrl = "categoryImages/r&b.jpg" },
                new TrackCategory { Id = 27, Category = "Саундтреки", ImageUrl = "categoryImages/soundtrack.jpg" },
                new TrackCategory { Id = 28, Category = "Соул та фанк", ImageUrl = "categoryImages/soulfunc.jpg" },
                new TrackCategory { Id = 29, Category = "Ретро", ImageUrl = "categoryImages/retro.jpg" },
                new TrackCategory { Id = 30, Category = "В машину", ImageUrl = "categoryImages/car.jpg" },
                new TrackCategory { Id = 31, Category = "Інше", ImageUrl = "categoryImages/other.jpg" }
                
            );

            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("TrackCategories table already contains data. Skipping initialization.");
        }


        if (!await context.Tracks.AnyAsync())
        {
            var wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

            //Track1
            var filePath1 = Path.Combine(wwwRootPath, "tracks", "track1.mp3"); // Шлях до файл
            using var fileStream1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read);
            var fileName1 = Path.GetFileName(filePath1); // Отримати ім'я файлу
            var file1 = new FormFile(fileStream1, 0, fileStream1.Length, "file", fileName1);

            var imagePath1 = Path.Combine(wwwRootPath, "trackImages", "image1.jpg"); // Шлях до файлу зображення
            using var imageStream1 = new FileStream(imagePath1, FileMode.Open, FileAccess.Read);
            var imageName1 = Path.GetFileName(imagePath1); // Отримати ім'я зображення
            var image1 = new FormFile(imageStream1, 0, imageStream1.Length, "image", imageName1);

            var createTrackRequest1 = new CreateTrackRequest
            {
                Title = "Crazy Frog",
                ArtistName = "Alex F",
                Genre = Genre.Pop,
                Tags = new List<string> { "#fun", },
                Categories = new List<string> { "Хіти", "Поп", "Електроніка" },
                Description = "Description for Crazy Frog"
            };
           

            //Track2
            var filePath2 = Path.Combine(wwwRootPath, "tracks", "track2.mp3");
            using var fileStream2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);
            var fileName2 = Path.GetFileName(filePath2);
            var file2 = new FormFile(fileStream2, 0, fileStream2.Length, "file", fileName2);

            var imagePath2 = Path.Combine(wwwRootPath, "trackImages", "image2.jpg");
            using var imageStream2 = new FileStream(imagePath2, FileMode.Open, FileAccess.Read);
            var imageName2 = Path.GetFileName(imagePath2);
            var image2 = new FormFile(imageStream2, 0, imageStream2.Length, "image", imageName2);

            var createTrackRequest2 = new CreateTrackRequest
            {
                Title = "Smells like teen spirit",
                ArtistName = "Nirvana",
                Genre = Genre.Rock,
                Tags = new List<string> { "#rock", "#nirvana" },
                Categories = new List<string> { "Рок", "90-ті" },
                Description = "Description for Smells like teen spirit"
            };
            

            //Track3
            var filePath3 = Path.Combine(wwwRootPath, "tracks", "track3.mp3");
            using var fileStream3 = new FileStream(filePath3, FileMode.Open, FileAccess.Read);
            var fileName3 = Path.GetFileName(filePath3);
            var file3 = new FormFile(fileStream3, 0, fileStream3.Length, "file", fileName3);

            var imagePath3 = Path.Combine(wwwRootPath, "trackImages", "image3.jpg");
            using var imageStream3 = new FileStream(imagePath3, FileMode.Open, FileAccess.Read);
            var imageName3 = Path.GetFileName(imagePath3);
            var image3 = new FormFile(imageStream3, 0, imageStream3.Length, "image", imageName3);

            var createTrackRequest3 = new CreateTrackRequest
            {
                Title = "Numb",
                ArtistName = "Linkin Park",
                Genre = Genre.Rock,
                Tags = new List<string> { "#rock", "#linkinpark" },
                Categories = new List<string> { "Рок", "2000-ні" },
                Description = "Description for Numb"
            };
            

            //Track4
            var filePath4 = Path.Combine(wwwRootPath, "tracks", "track4.mp3");
            using var fileStream4 = new FileStream(filePath4, FileMode.Open, FileAccess.Read);
            var fileName4 = Path.GetFileName(filePath4);
            var file4 = new FormFile(fileStream4, 0, fileStream4.Length, "file", fileName4);

            var imagePath4 = Path.Combine(wwwRootPath, "trackImages", "image4.jpg");
            using var imageStream4 = new FileStream(imagePath4, FileMode.Open, FileAccess.Read);
            var imageName4 = Path.GetFileName(imagePath4);
            var image4 = new FormFile(imageStream4, 0, imageStream4.Length, "image", imageName4);

            var createTrackRequest4 = new CreateTrackRequest
            {
                Title = "In the end",
                ArtistName = "Linkin Park",
                Genre = Genre.Rock,
                Tags = new List<string> { "#rock", "#linkinpark" },
                Categories = new List<string> { "Рок", "2000-ні", "Хіти" },
                Description = "Description for In the end"
            };


            //Track5
            var filePath5 = Path.Combine(wwwRootPath, "tracks", "track5.mp3");
            using var fileStream5 = new FileStream(filePath5, FileMode.Open, FileAccess.Read);
            var fileName5 = Path.GetFileName(filePath5);
            var file5 = new FormFile(fileStream5, 0, fileStream5.Length, "file", fileName5);

            var imagePath5 = Path.Combine(wwwRootPath, "trackImages", "image5.jpg");
            using var imageStream5 = new FileStream(imagePath5, FileMode.Open, FileAccess.Read);
            var imageName5 = Path.GetFileName(imagePath5);
            var image5 = new FormFile(imageStream5, 0, imageStream5.Length, "image", imageName5);

            var createTrackRequest5 = new CreateTrackRequest
            {
                Title = "Та4то",
                ArtistName = "Бумбокс",
                Genre = Genre.Pop,
                Tags = new List<string> { "#pop", "#бумбокс" },
                Categories = new List<string> { "Романтика", "Поп" },
                Description = "Description for Та4то"
            };


            //Track6
            var filePath6 = Path.Combine(wwwRootPath, "tracks", "track6.mp3");
            using var fileStream6 = new FileStream(filePath6, FileMode.Open, FileAccess.Read);
            var fileName6 = Path.GetFileName(filePath6);
            var file6 = new FormFile(fileStream6, 0, fileStream6.Length, "file", fileName6);

            var imagePath6 = Path.Combine(wwwRootPath, "trackImages", "image6.jpg");
            using var imageStream6 = new FileStream(imagePath6, FileMode.Open, FileAccess.Read);
            var imageName6 = Path.GetFileName(imagePath6);
            var image6 = new FormFile(imageStream6, 0, imageStream6.Length, "image", imageName6);

            var createTrackRequest6 = new CreateTrackRequest
            {
                Title = "Last resort",
                ArtistName = "Papa Roach",
                Genre = Genre.Rock,
                Tags = new List<string> { "#rock", "#paparoach" },
                Categories = new List<string> { "Рок" },
                Description = "Description for Last resort"
            };


            //Track7
            var filePath7 = Path.Combine(wwwRootPath, "tracks", "track7.mp3");
            using var fileStream7 = new FileStream(filePath7, FileMode.Open, FileAccess.Read);
            var fileName7 = Path.GetFileName(filePath7);
            var file7 = new FormFile(fileStream7, 0, fileStream7.Length, "file", fileName7);

            var imagePath7 = Path.Combine(wwwRootPath, "trackImages", "image7.jpg");
            using var imageStream7 = new FileStream(imagePath7, FileMode.Open, FileAccess.Read);
            var imageName7 = Path.GetFileName(imagePath7);
            var image7 = new FormFile(imageStream7, 0, imageStream7.Length, "image", imageName7);

            var createTrackRequest7 = new CreateTrackRequest
            {
                Title = "Between angel and insects",
                ArtistName = "Papa Roach",
                Genre = Genre.Rock,
                Tags = new List<string> { "#rock", "#paparoach" },
                Categories = new List<string> { "Рок" },
                Description = "Description for Between angel and insects"
            };


            //Track8
            var filePath8 = Path.Combine(wwwRootPath, "tracks", "track8.mp3");
            using var fileStream8 = new FileStream(filePath8, FileMode.Open, FileAccess.Read);
            var fileName8 = Path.GetFileName(filePath8);
            var file8 = new FormFile(fileStream8, 0, fileStream8.Length, "file", fileName8);

            var imagePath8 = Path.Combine(wwwRootPath, "trackImages", "image8.jpg");
            using var imageStream8 = new FileStream(imagePath8, FileMode.Open, FileAccess.Read);
            var imageName8 = Path.GetFileName(imagePath8);
            var image8 = new FormFile(imageStream8, 0, imageStream8.Length, "image", imageName8);

            var createTrackRequest8 = new CreateTrackRequest
            {
                Title = "Love the way you lie",
                ArtistName = "Rihanna",
                Genre = Genre.Rock,
                Tags = new List<string> { "#pop", "#rihanna" },
                Categories = new List<string> { "Поп" },
                Description = "Description for Love the way you lie"
            };


            //Track9
            var filePath9 = Path.Combine(wwwRootPath, "tracks", "track9.mp3");
            using var fileStream9 = new FileStream(filePath9, FileMode.Open, FileAccess.Read);
            var fileName9 = Path.GetFileName(filePath9);
            var file9 = new FormFile(fileStream9, 0, fileStream9.Length, "file", fileName9);

            var imagePath9 = Path.Combine(wwwRootPath, "trackImages", "image9.jpg");
            using var imageStream9 = new FileStream(imagePath9, FileMode.Open, FileAccess.Read);
            var imageName9 = Path.GetFileName(imagePath9);
            var image9 = new FormFile(imageStream9, 0, imageStream9.Length, "image", imageName9);

            var createTrackRequest9 = new CreateTrackRequest
            {
                Title = "The Monster",
                ArtistName = "Rihanna",
                Genre = Genre.Pop,
                Tags = new List<string> { "#pop", "#rihanna" },
                Categories = new List<string> { "Поп" },
                Description = "Description for The Monster"
            };


            //Track10
            var filePath10 = Path.Combine(wwwRootPath, "tracks", "track10.mp3");
            using var fileStream10 = new FileStream(filePath10, FileMode.Open, FileAccess.Read);
            var fileName10 = Path.GetFileName(filePath10);
            var file10 = new FormFile(fileStream10, 0, fileStream10.Length, "file", fileName10);

            var imagePath10 = Path.Combine(wwwRootPath, "trackImages", "image10.jpg");
            using var imageStream10 = new FileStream(imagePath10, FileMode.Open, FileAccess.Read);
            var imageName10 = Path.GetFileName(imagePath10);
            var image10 = new FormFile(imageStream10, 0, imageStream10.Length, "image", imageName10);

            var createTrackRequest10 = new CreateTrackRequest
            {
                Title = "А липи цвітуть",
                ArtistName = "Іво Бобул",
                Genre = Genre.Classical,
                Tags = new List<string> { "#естрада", "#івобобул" },
                Categories = new List<string> { "Інше", "Романтика" },
                Description = "Description for А липи цвітуть"
            };


            //Track11
            var filePath11 = Path.Combine(wwwRootPath, "tracks", "track11.mp3");
            using var fileStream11 = new FileStream(filePath11, FileMode.Open, FileAccess.Read);
            var fileName11 = Path.GetFileName(filePath11);
            var file11 = new FormFile(fileStream11, 0, fileStream11.Length, "file", fileName11);

            var imagePath11 = Path.Combine(wwwRootPath, "trackImages", "image11.jpg");
            using var imageStream11 = new FileStream(imagePath11, FileMode.Open, FileAccess.Read);
            var imageName11 = Path.GetFileName(imagePath11);
            var image11 = new FormFile(imageStream11, 0, imageStream11.Length, "image", imageName11);

            var createTrackRequest11 = new CreateTrackRequest
            {
                Title = "The Door",
                ArtistName = "Teddy Swims",
                Genre = Genre.Soul,
                Tags = new List<string> { "#soul", "#swims" },
                Categories = new List<string> { "Хіти", "R&B", "Соул та фанк" },
                Description = "Description for The Door"
            };


            //Track12
            var filePath12 = Path.Combine(wwwRootPath, "tracks", "track12.mp3");
            using var fileStream12 = new FileStream(filePath12, FileMode.Open, FileAccess.Read);
            var fileName12 = Path.GetFileName(filePath12);
            var file12 = new FormFile(fileStream12, 0, fileStream12.Length, "file", fileName12);

            var imagePath12 = Path.Combine(wwwRootPath, "trackImages", "image12.jpg");
            using var imageStream12 = new FileStream(imagePath12, FileMode.Open, FileAccess.Read);
            var imageName12 = Path.GetFileName(imagePath12);
            var image12 = new FormFile(imageStream12, 0, imageStream12.Length, "image", imageName12);

            var createTrackRequest12 = new CreateTrackRequest
            {
                Title = "Уночі",
                ArtistName = "Yaktak",
                Genre = Genre.Pop,
                Tags = new List<string> { "#yaktak", "#pop" },
                Categories = new List<string> { "Поп", "Романтика" },
                Description = "Description for Уночі"
            };


            //Track13
            var filePath13 = Path.Combine(wwwRootPath, "tracks", "track13.mp3");
            using var fileStream13 = new FileStream(filePath13, FileMode.Open, FileAccess.Read);
            var fileName13 = Path.GetFileName(filePath13);
            var file13 = new FormFile(fileStream13, 0, fileStream13.Length, "file", fileName13);

            var imagePath13 = Path.Combine(wwwRootPath, "trackImages", "image13.jpg");
            using var imageStream13 = new FileStream(imagePath13, FileMode.Open, FileAccess.Read);
            var imageName13 = Path.GetFileName(imagePath13);
            var image13 = new FormFile(imageStream13, 0, imageStream13.Length, "image", imageName13);

            var createTrackRequest13 = new CreateTrackRequest
            {
                Title = "Lalala",
                ArtistName = "Yaktak",
                Genre = Genre.Pop,
                Tags = new List<string> { "#yaktak", "#pop" },
                Categories = new List<string> { "Dance", "Поп" },
                Description = "Description for Lalala"
            };


            //Track14
            var filePath14 = Path.Combine(wwwRootPath, "tracks", "track14.mp3");
            using var fileStream14 = new FileStream(filePath14, FileMode.Open, FileAccess.Read);
            var fileName14 = Path.GetFileName(filePath14);
            var file14 = new FormFile(fileStream14, 0, fileStream14.Length, "file", fileName14);

            var imagePath14 = Path.Combine(wwwRootPath, "trackImages", "image14.jpg");
            using var imageStream14 = new FileStream(imagePath14, FileMode.Open, FileAccess.Read);
            var imageName14 = Path.GetFileName(imagePath14);
            var image14 = new FormFile(imageStream14, 0, imageStream14.Length, "image", imageName14);

            var createTrackRequest14 = new CreateTrackRequest
            {
                Title = "Supergirl",
                ArtistName = "Reamonn",
                Genre = Genre.Rock,
                Tags = new List<string> { "#reamonn", "#supergirl", "Романтика" },
                Categories = new List<string> { "2000-ні", "Рок", "Хіти" },
                Description = "Description for Lalala"
            };


            //Track15
            var filePath15 = Path.Combine(wwwRootPath, "tracks", "track15.mp3");
            using var fileStream15 = new FileStream(filePath15, FileMode.Open, FileAccess.Read);
            var fileName15 = Path.GetFileName(filePath15);
            var file15 = new FormFile(fileStream15, 0, fileStream15.Length, "file", fileName15);

            var imagePath15 = Path.Combine(wwwRootPath, "trackImages", "image15.jpg");
            using var imageStream15 = new FileStream(imagePath15, FileMode.Open, FileAccess.Read);
            var imageName15 = Path.GetFileName(imagePath15);
            var image15 = new FormFile(imageStream15, 0, imageStream15.Length, "image", imageName15);

            var createTrackRequest15 = new CreateTrackRequest
            {
                Title = "Без неї ніяк",
                ArtistName = "Без Обмежень",
                Genre = Genre.Rock,
                Tags = new List<string> { "#безобмежень" },
                Categories = new List<string> { "Рок", "Романтика", "В машину" },
                Description = "Description for Без неї ніяк"
            };


            //Track16
            var filePath16 = Path.Combine(wwwRootPath, "tracks", "track16.mp3");
            using var fileStream16 = new FileStream(filePath16, FileMode.Open, FileAccess.Read);
            var fileName16 = Path.GetFileName(filePath16);
            var file16 = new FormFile(fileStream16, 0, fileStream16.Length, "file", fileName16);

            var imagePath16 = Path.Combine(wwwRootPath, "trackImages", "image16.jpg");
            using var imageStream16 = new FileStream(imagePath16, FileMode.Open, FileAccess.Read);
            var imageName16 = Path.GetFileName(imagePath16);
            var image16 = new FormFile(imageStream16, 0, imageStream16.Length, "image", imageName16);

            var createTrackRequest16 = new CreateTrackRequest
            {
                Title = "Story of my life",
                ArtistName = "One Direction",
                Genre = Genre.Pop,
                Tags = new List<string> { "#onedirection" },
                Categories = new List<string> { "Поп", "Романтика", "2010-ті" },
                Description = "Description for Story of my life"
            };


            //Track17
            var filePath17 = Path.Combine(wwwRootPath, "tracks", "track17.mp3");
            using var fileStream17 = new FileStream(filePath17, FileMode.Open, FileAccess.Read);
            var fileName17 = Path.GetFileName(filePath17);
            var file17 = new FormFile(fileStream17, 0, fileStream17.Length, "file", fileName17);

            var imagePath17 = Path.Combine(wwwRootPath, "trackImages", "image17.jpg");
            using var imageStream17 = new FileStream(imagePath17, FileMode.Open, FileAccess.Read);
            var imageName17 = Path.GetFileName(imagePath17);
            var image17 = new FormFile(imageStream17, 0, imageStream17.Length, "image", imageName17);

            var createTrackRequest17 = new CreateTrackRequest
            {
                Title = "Gossip",
                ArtistName = "Maneskin",
                Genre = Genre.Rock,
                Tags = new List<string> { "#maneskin" },
                Categories = new List<string> { "Рок", "В машину", "Хіти" },
                Description = "Description for Gossip"
            };


            //Track18
            var filePath18 = Path.Combine(wwwRootPath, "tracks", "track18.mp3");
            using var fileStream18 = new FileStream(filePath18, FileMode.Open, FileAccess.Read);
            var fileName18 = Path.GetFileName(filePath18);
            var file18 = new FormFile(fileStream18, 0, fileStream18.Length, "file", fileName18);

            var imagePath18 = Path.Combine(wwwRootPath, "trackImages", "image18.jpg");
            using var imageStream18 = new FileStream(imagePath18, FileMode.Open, FileAccess.Read);
            var imageName18 = Path.GetFileName(imagePath18);
            var image18 = new FormFile(imageStream18, 0, imageStream18.Length, "image", imageName18);

            var createTrackRequest18 = new CreateTrackRequest
            {
                Title = "It must have been love",
                ArtistName = "Roxette",
                Genre = Genre.Rock,
                Tags = new List<string> { "#roxette" },
                Categories = new List<string> { "Рок", "В машину", "80-ті", "Романтика" },
                Description = "Description for It must have been love"
            };


            //Track19
            var filePath19 = Path.Combine(wwwRootPath, "tracks", "track19.mp3");
            using var fileStream19 = new FileStream(filePath19, FileMode.Open, FileAccess.Read);
            var fileName19 = Path.GetFileName(filePath19);
            var file19 = new FormFile(fileStream19, 0, fileStream19.Length, "file", fileName19);

            var imagePath19 = Path.Combine(wwwRootPath, "trackImages", "image19.jpg");
            using var imageStream19 = new FileStream(imagePath19, FileMode.Open, FileAccess.Read);
            var imageName19 = Path.GetFileName(imagePath19);
            var image19 = new FormFile(imageStream19, 0, imageStream19.Length, "image", imageName19);

            var createTrackRequest19 = new CreateTrackRequest
            {
                Title = "Love Don't Let Me Go",
                ArtistName = "David Guetta",
                Genre = Genre.Electronic,
                Tags = new List<string> { "#davidguetta" },
                Categories = new List<string> { "Електроніка", "Club", "2000-ні", "Dance", "В машину" },
                Description = "Description for Love Don't Let Me Go"
            };


            //Track20
            var filePath20 = Path.Combine(wwwRootPath, "tracks", "track20.mp3");
            using var fileStream20 = new FileStream(filePath20, FileMode.Open, FileAccess.Read);
            var fileName20 = Path.GetFileName(filePath20);
            var file20 = new FormFile(fileStream20, 0, fileStream20.Length, "file", fileName20);

            var imagePath20 = Path.Combine(wwwRootPath, "trackImages", "image20.jpg");
            using var imageStream20 = new FileStream(imagePath20, FileMode.Open, FileAccess.Read);
            var imageName20 = Path.GetFileName(imagePath20);
            var image20 = new FormFile(imageStream20, 0, imageStream20.Length, "image", imageName20);

            var createTrackRequest20 = new CreateTrackRequest
            {
                Title = "Starboy",
                ArtistName = "Weeknd",
                Genre = Genre.RAndB,
                Tags = new List<string> { "#weeknd" },
                Categories = new List<string> { "R&B", "В машину" },
                Description = "Description for Starboy"
            };


            //Track21
            var filePath21 = Path.Combine(wwwRootPath, "tracks", "track21.mp3");
            using var fileStream21 = new FileStream(filePath21, FileMode.Open, FileAccess.Read);
            var fileName21 = Path.GetFileName(filePath21);
            var file21 = new FormFile(fileStream21, 0, fileStream21.Length, "file", fileName21);

            var imagePath21 = Path.Combine(wwwRootPath, "trackImages", "image21.jpg");
            using var imageStream21 = new FileStream(imagePath21, FileMode.Open, FileAccess.Read);
            var imageName21 = Path.GetFileName(imagePath21);
            var image21 = new FormFile(imageStream21, 0, imageStream21.Length, "image", imageName21);

            var createTrackRequest21 = new CreateTrackRequest
            {
                Title = "Tatoo",
                ArtistName = "Loreen",
                Genre = Genre.Pop,
                Tags = new List<string> { "#weeknd" },
                Categories = new List<string> { "Поп", "Електроніка", "Інді", "Dance" },
                Description = "Description for Tatoo"
            };
            

            // track1
            var trackResponse1 = await _trackService.UploadTrackAsync(file1, image1, createTrackRequest1, 1);
            if (trackResponse1 != null)
            {
                tracks.Add(trackResponse1);
                var trackId = trackResponse1.TrackId;
                var userId = 1;
         
                // Додаємо 55 прослуховувань в циклі
                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 55; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }
                
                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();
            

            // track2
            var trackResponse2 = await _trackService.UploadTrackAsync(file2, image2, createTrackRequest2, 2);
            if (trackResponse2 != null)
            {
                tracks.Add(trackResponse2);
                var trackId = trackResponse2.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 91; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i) 
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track3
            var trackResponse3 = await _trackService.UploadTrackAsync(file3, image3, createTrackRequest3, 1);
            if (trackResponse3 != null)
            {
                tracks.Add(trackResponse3);
                var trackId = trackResponse3.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 59; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track4
            var trackResponse4 = await _trackService.UploadTrackAsync(file4, image4, createTrackRequest4, 1);
            if (trackResponse4 != null)
            {
                tracks.Add(trackResponse4);
                var trackId = trackResponse4.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 60; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i) 
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track5
            var trackResponse5 = await _trackService.UploadTrackAsync(file5, image5, createTrackRequest5, 3);
            if (trackResponse5 != null)
            {
                tracks.Add(trackResponse5);
                var trackId = trackResponse5.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 19; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track6
            var trackResponse6 = await _trackService.UploadTrackAsync(file6, image6, createTrackRequest6, 2);
            if (trackResponse6 != null)
            {
                tracks.Add(trackResponse6);
                var trackId = trackResponse6.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 36; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track7
            var trackResponse7 = await _trackService.UploadTrackAsync(file7, image7, createTrackRequest7, 3);
            if (trackResponse7 != null)
            {
                tracks.Add(trackResponse7);
                var trackId = trackResponse7.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 25; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track8
            var trackResponse8 = await _trackService.UploadTrackAsync(file8, image8, createTrackRequest8, 1);
            if (trackResponse8 != null)
            {
                tracks.Add(trackResponse8);
                var trackId = trackResponse8.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 33; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track9
            var trackResponse9 = await _trackService.UploadTrackAsync(file9, image9, createTrackRequest9, 2);
            if (trackResponse9 != null)
            {
                tracks.Add(trackResponse9);
                var trackId = trackResponse9.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 72; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track10
            var trackResponse10 = await _trackService.UploadTrackAsync(file10, image10, createTrackRequest10, 2);
            if (trackResponse10 != null)
            {
                tracks.Add(trackResponse10);
                var trackId = trackResponse10.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 97; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track11
            var trackResponse11 = await _trackService.UploadTrackAsync(file11, image11, createTrackRequest11, 4);
            if (trackResponse11 != null)
            {
                tracks.Add(trackResponse11);
                var trackId = trackResponse11.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 78; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track12
            var trackResponse12 = await _trackService.UploadTrackAsync(file12, image12, createTrackRequest12, 4);
            if (trackResponse12 != null)
            {
                tracks.Add(trackResponse12);
                var trackId = trackResponse12.TrackId;
                var userId = 3;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 71; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track13
            var trackResponse13 = await _trackService.UploadTrackAsync(file13, image13, createTrackRequest13, 2);
            if (trackResponse13 != null)
            {
                tracks.Add(trackResponse13);
                var trackId = trackResponse13.TrackId;
                var userId = 4;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 69; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track14
            var trackResponse14 = await _trackService.UploadTrackAsync(file14, image14, createTrackRequest14, 1);
            if (trackResponse14 != null)
            {
                tracks.Add(trackResponse14);
                var trackId = trackResponse14.TrackId;
                var userId = 4;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 89; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track15
            var trackResponse15 = await _trackService.UploadTrackAsync(file15, image15, createTrackRequest15, 2);
            if (trackResponse15 != null)
            {
                tracks.Add(trackResponse15);
                var trackId = trackResponse15.TrackId;
                var userId = 5;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 62; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track16
            var trackResponse16 = await _trackService.UploadTrackAsync(file16, image16, createTrackRequest16, 4);
            if (trackResponse16 != null)
            {
                tracks.Add(trackResponse16);
                var trackId = trackResponse16.TrackId;
                var userId = 3;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 44; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track17
            var trackResponse17 = await _trackService.UploadTrackAsync(file17, image17, createTrackRequest17, 3);
            if (trackResponse17 != null)
            {
                tracks.Add(trackResponse17);
                var trackId = trackResponse17.TrackId;
                var userId = 3;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 84; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track18
            var trackResponse18 = await _trackService.UploadTrackAsync(file18, image18, createTrackRequest18, 4);
            if (trackResponse18 != null)
            {
                tracks.Add(trackResponse18);
                var trackId = trackResponse18.TrackId;
                var userId = 1;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 90; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track19
            var trackResponse19 = await _trackService.UploadTrackAsync(file19, image19, createTrackRequest19, 3);
            if (trackResponse19 != null)
            {
                tracks.Add(trackResponse19);
                var trackId = trackResponse19.TrackId;
                var userId = 2;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 63; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track20
            var trackResponse20 = await _trackService.UploadTrackAsync(file20, image20, createTrackRequest20, 1);
            if (trackResponse20 != null)
            {
                tracks.Add(trackResponse20);
                var trackId = trackResponse20.TrackId;
                var userId = 4;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 12; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();


            // track21
            var trackResponse21 = await _trackService.UploadTrackAsync(file21, image21, createTrackRequest21, 2);
            if (trackResponse21 != null)
            {
                tracks.Add(trackResponse21);
                var trackId = trackResponse21.TrackId;
                var userId = 3;

                var listenings = new List<Listening>();
                var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                for (int i = 0; i < 68; i++)
                {
                    listenings.Add(new Listening
                    {
                        UserId = userId,
                        TrackId = trackId,
                        ListenedDate = DateTime.Now.AddMinutes(i)
                    });
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        trackMetadata.Plays+=2;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }

                context.Listenings.AddRange(listenings);
            }
            else
            {
                Console.WriteLine("Failed to create the track.");
            }
            await context.SaveChangesAsync();

            
            //Генеруємо 60 випадкових лайків
            var random = new Random();
            var likes = new HashSet<(int UserId, int TrackId)>();
            for (int i = 0; i < 60; i++)
            {
                int userId = users[random.Next(users.Count)].Id;
                int trackId = tracks[random.Next(tracks.Count)].TrackId;

                if (!likes.Contains((userId, trackId)))
                {
                    likes.Add((userId, trackId));

                    context.Likes.Add(new Like
                    {
                        UserId = userId,
                        TrackId = trackId,
                        LikedDate = DateTime.Now
                    });

                    var trackMetadata = await context.TrackMetadatas.FirstOrDefaultAsync(tm => tm.TrackId == trackId);
                    if (trackMetadata != null)
                    {
                        trackMetadata.Likes++;
                        context.TrackMetadatas.Update(trackMetadata);
                    }
                }
            }
            await context.SaveChangesAsync();

        }
        else
        {
            Console.WriteLine("Tracks table already contains data. Skipping initialization.");
        }
    }
}


