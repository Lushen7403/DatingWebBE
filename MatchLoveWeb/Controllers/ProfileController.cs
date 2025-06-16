using AutoMapper;
using MatchLoveWeb.Models;
using MatchLoveWeb.Models.DTO;
using MatchLoveWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using CloudinaryDotNet.Actions;

namespace MatchLoveWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly DatingWebContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly IMapper _mapper;
        private readonly PhotoService _photoService;
        public ProfileController(DatingWebContext db,
                                 IWebHostEnvironment env,
                                 IMapper mapper,
                                 PhotoService photoService)
        {
            _db = db;
            _env = env;
            _mapper = mapper;
            _photoService = photoService;
        }

        // POST: api/profile
        [HttpPost("CreateProfile")]
        public async Task<IActionResult> Create([FromForm] ProfileDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _db.Profiles.AnyAsync(p => p.AccountId == dto.AccountId))
                return BadRequest("Tài khoản này đã có hồ sơ.");

            var profile = _mapper.Map<Models.Profile>(dto);
            profile.CreatedAt = DateTime.Now;

            // Upload avatar
            if (dto.Avatar != null)
            {
                var uploadAvatar = await _photoService.UploadAvatarAsync(dto.Avatar);
                profile.Avatar = $"{uploadAvatar.PublicId}{GetExtension(dto.Avatar)}";
                profile.PublicId = uploadAvatar.PublicId;
            }

            _db.Profiles.Add(profile);
            await _db.SaveChangesAsync();

            // Upload profile images
            if (dto.ProfileImages?.Any() == true)
            {
                var uploads = await _photoService.UploadProfileImagesAsync(dto.ProfileImages);
                foreach (var (file, up) in dto.ProfileImages.Zip(uploads))
                {
                    var ext = Path.GetExtension(file.FileName);
                    _db.ProfileImages.Add(new ProfileImage
                    {
                        ProfileId = profile.Id,
                        ImageUrl = $"{up.PublicId}{ext}",
                        PublicId = up.PublicId,
                        CreatedAt = DateTime.Now
                    });
                }
                await _db.SaveChangesAsync();
            }

            if (dto.HobbyIds?.Any() == true)
            {
                var userHobbies = dto.HobbyIds.Select(hobbyId => new UserHobby
                {
                    ProfileId = profile.Id,
                    HobbyId = hobbyId
                });

                await _db.UserHobbies.AddRangeAsync(userHobbies);
                await _db.SaveChangesAsync();
            }


            return Ok(BuildResponse(profile));
        }

        // PUT: api/profile/UpdateProfile/{accountId}
        [HttpPut("UpdateProfile/{accountId}")]
        public async Task<IActionResult> Update(int accountId, [FromForm] ProfileDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var profile = await _db.Profiles
                .Include(p => p.ProfileImages)
                .FirstOrDefaultAsync(p => p.AccountId == accountId);
            if (profile == null)
                return NotFound("Không tìm thấy hồ sơ.");

            _mapper.Map(dto, profile);
            profile.UpdatedAt = DateTime.Now;

            if (dto.Avatar != null)
            {
                if (!string.IsNullOrEmpty(profile.PublicId))
                    await _photoService.DeleteImageAsync(profile.PublicId);

                var upAv = await _photoService.UploadAvatarAsync(dto.Avatar);
                profile.Avatar = $"{upAv.PublicId}{GetExtension(dto.Avatar)}";
                profile.PublicId = upAv.PublicId;
            }

            foreach (var old in profile.ProfileImages)
            {
                if (!string.IsNullOrEmpty(old.PublicId))
                    await _photoService.DeleteImageAsync(old.PublicId);
            }
            // Xóa record cũ
            _db.ProfileImages.RemoveRange(profile.ProfileImages);
            await _db.SaveChangesAsync();

            // Upload ảnh mới
            if (dto.ProfileImages != null)
            {
                var uploads = await _photoService.UploadProfileImagesAsync(dto.ProfileImages);

                int index = 0;
                foreach (var up in uploads)
                {
                    var file = dto.ProfileImages.ElementAt(index);
                    var ext = Path.GetExtension(file.FileName);

                    _db.ProfileImages.Add(new ProfileImage
                    {
                        ProfileId = profile.Id,
                        ImageUrl = $"{up.PublicId}{ext}",
                        PublicId = up.PublicId,
                        CreatedAt = DateTime.Now
                    });

                    index++;
                }
                await _db.SaveChangesAsync();
            }

            var existingHobbies = _db.UserHobbies.Where(uh => uh.ProfileId == profile.Id);
            _db.UserHobbies.RemoveRange(existingHobbies);
            await _db.SaveChangesAsync();

            // Thêm sở thích mới
            if (dto.HobbyIds?.Any() == true)
            {

                var newHobbies = dto.HobbyIds.Select(hobbyId => new UserHobby
                {
                    ProfileId = profile.Id,
                    HobbyId = hobbyId
                });
                await _db.UserHobbies.AddRangeAsync(newHobbies);
                await _db.SaveChangesAsync();
            }

            return Ok(BuildResponse(profile));
        }

        // GET: api/profile/random/{accountId}
        [HttpGet("GetProfilesToMatch/{accountId}")]
        public async Task<IActionResult> GetRandomProfiles(int accountId)
        {
            try
            {
                var location = _db.Locations.First(p => p.AccountId == accountId);

                var profilesQuery = _db.Profiles.AsQueryable();
                var condition = _db.MatchConditons.FirstOrDefault(x => x.AccountId == accountId);

                if (condition != null)
                {
                    if (condition.GenderId != null)
                    {
                        profilesQuery = profilesQuery.Where(p => p.GenderId == condition.GenderId);
                    }

                    if (condition.MinAge != null && condition.MaxAge != null)
                    {
                        var minDay = DateTime.Now.AddYears(-condition.MinAge.Value);
                        var maxDay = DateTime.Now.AddYears(-condition.MaxAge.Value);
                        profilesQuery = profilesQuery.Where(p => p.Birthday <= minDay && p.Birthday >= maxDay);
                    }
                }

                // Load trước vào bộ nhớ để tránh conflict
                var profiles = profilesQuery.ToList();

                profiles = profiles
                            .Where(p => p.AccountId != accountId)   
                            .ToList();

                // Lọc theo khoảng cách nếu có
                if (condition?.MaxDistanceKm != null)
                {
                    var allLocations = _db.Locations.ToList();

                    profiles = profiles.Where(p =>
                    {
                        var otherLoc = allLocations.FirstOrDefault(x => x.AccountId == p.AccountId);
                        if (otherLoc == null)
                        {
                            Console.WriteLine($"Không tìm thấy location cho AccountId = {p.AccountId}");
                            return false;
                        }

                        var distance = Caculate(location.Latitude, location.Longitude, otherLoc.Latitude, otherLoc.Longitude);
                        Console.WriteLine($"AccountId: {p.AccountId}, Distance: {distance} km");
                        return distance <= condition.MaxDistanceKm;
                    }).ToList();
                }

                // Lọc các profile đã quẹt
                var swipedIds = _db.Swipes
                                  .Where(s => s.AccountId == accountId)
                                  .Select(s => s.SwipedAccountId)
                                  .ToHashSet();

                profiles = profiles.Where(p => !swipedIds.Contains(p.AccountId)).ToList();

                var blockedIds = _db.Blocks
                            .Where(b => b.BlockerId == accountId || b.BlockedUserId == accountId)
                            .Select(b => b.BlockerId == accountId ? b.BlockedUserId : b.BlockerId)
                            .ToHashSet();

                profiles = profiles.Where(p => !blockedIds.Contains(p.AccountId)).ToList();

                if (!profiles.Any())
                    return NotFound("Không tìm thấy hồ sơ phù hợp.");

                var myHobbyIds = _db.UserHobbies
                        .Where(uh => uh.Profile.AccountId == accountId)
                        .Select(uh => uh.HobbyId)
                        .ToHashSet(); 

                profiles = profiles
                    .Select(p => new
                    {
                        Profile = p,
                        CommonCount = _db.UserHobbies
                            .Count(uh => uh.ProfileId == p.Id && myHobbyIds.Contains(uh.HobbyId))
                    })
                    .OrderByDescending(x => x.CommonCount)
                    .Select(x => x.Profile)
                    .ToList();

                var result = profiles.Select(p =>
                {
                    var loc = _db.Locations.First(l => l.AccountId == p.AccountId);
                    return new
                    {
                        p.Id,
                        p.AccountId,
                        p.FullName,
                        p.GenderId,
                        p.Birthday,
                        p.Description,
                        p.Avatar,
                        p.CreatedAt,
                        p.PublicId,
                        p.Account,
                        p.ProfileImages,
                        p.Gender,
                        p.UserHobbies,
                        latitude = loc.Latitude,
                        longitude = loc.Longitude
                    };
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }


        private double Caculate(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // bán kính Trái Đất (km)

            // Chuyển đổi độ thành radian ngay tại chỗ
            double toRad = Math.PI / 180.0;
            double φ1 = lat1 * toRad;
            double φ2 = lat2 * toRad;
            double Δφ = (lat2 - lat1) * toRad;
            double Δλ = (lon2 - lon1) * toRad;

            double a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                       Math.Cos(φ1) * Math.Cos(φ2) *
                       Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
        // GET: api/profile/GetProfile/{accountId}
        [HttpGet("GetProfile/{accountId}")]
        public async Task<IActionResult> GetProfile(int accountId)
        {
            var profile = await _db.Profiles
                .Include(p => p.ProfileImages)
                .FirstOrDefaultAsync(p => p.AccountId == accountId);
            if (profile == null)
                return NotFound("Không tìm thấy hồ sơ.");
            return Ok(BuildResponse(profile));
        }

        [HttpGet("GetDistance")]
        public async Task<IActionResult> GetDistance(int accountId, int profileId)
        {
            try
            {
                // Tìm profile của người kia
                var targetProfile = await _db.Profiles.FirstOrDefaultAsync(p => p.Id == profileId);
                if (targetProfile == null)
                    return NotFound("Không tìm thấy profile.");

                var targetAccountId = targetProfile.AccountId;

                // Lấy thông tin vị trí của 2 account
                var currentLocation = await _db.Locations.FirstOrDefaultAsync(l => l.AccountId == accountId);
                var targetLocation = await _db.Locations.FirstOrDefaultAsync(l => l.AccountId == targetAccountId);

                if (currentLocation == null || targetLocation == null)
                    return NotFound("Không tìm thấy vị trí của một trong hai tài khoản.");

                // Tính khoảng cách
                double distanceKm = Caculate(
                    currentLocation.Latitude,
                    currentLocation.Longitude,
                    targetLocation.Latitude,
                    targetLocation.Longitude
                );

                return Ok(new { DistanceKm = distanceKm });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }


        // Helper: nối scheme/host với publicId thành URL đầy đủ
        private object BuildResponse(Models.Profile p)
        {
            var scheme = Request.Scheme;
            var host = Request.Host.Value;
            var baseUrl = $"https://res.cloudinary.com/dfvhhpkyg/image/upload/";

            string avatarUrl = p.PublicId is not null
                ? $"{baseUrl}{p.Avatar}"
                : null;

            var images = p.ProfileImages
                .Select(pi => $"{baseUrl}{pi.ImageUrl}")
                .ToList();

            var hobbyIds = _db.UserHobbies
                      .Where(uh => uh.ProfileId == p.Id)
                      .Select(uh => uh.HobbyId)
                      .ToList();

            return new
            {
                p.Id,
                p.AccountId,
                p.FullName,
                p.Birthday,
                p.GenderId,
                p.Description,
                AvatarUrl = avatarUrl,
                ImageUrls = images,
                HobbyIds = hobbyIds,
                p.CreatedAt,
                p.UpdatedAt
            };
        }

        private string GetExtension(IFormFile file)
           => System.IO.Path.GetExtension(file.FileName);

        private string GetExtensionByPublicId(string publicId)
            => System.IO.Path.GetExtension(publicId);

        [HttpGet("hobbies")]
        public async Task<IActionResult> GetAllHobbies()
        {
            var hobbies = await _db.Hobbies.ToListAsync();
            return Ok(hobbies);
        }

        [HttpDelete("DeleteProfile/{accountId}")]
        public async Task<IActionResult> DeleteProfile(int accountId)
        {
            try
            {
                var profile = await _db.Profiles.FirstOrDefaultAsync(p => p.AccountId == accountId);

                if (profile == null)
                {
                    return NotFound($"Không tìm thấy hồ sơ với AccountId = {accountId}");
                }
                var userHobbies = _db.UserHobbies.Where(uh => uh.ProfileId == profile.Id);
                _db.UserHobbies.RemoveRange(userHobbies);
                _db.Profiles.Remove(profile);
                await _db.SaveChangesAsync();

                return NoContent(); // 204
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi máy chủ: {ex.Message}");
            }
        }
    }


}
