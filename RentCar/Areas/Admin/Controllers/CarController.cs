using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentCar.Areas.Admin.Constants;
using RentCar.Areas.Admin.Models.ViewModel;
using RentCar.Areas.Admin.Utilis;
using RentCar.DAL;
using RentCar.Data;
using RentCar.Models.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RentCar.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleConstants.Admin)]
    public class CarController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _dbContext;

        public CarController(AppDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        //***** Index *****//
        public async Task<IActionResult> Index()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            var carModels = await _dbContext.CarModels.Where(c => !c.IsDeleted && c.OwnerId == user.Id)
                .Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages.Where(i => i.IsMain)).ToListAsync();


            return View(carModels);
        }

        //***** Car Detail *****//
        public async Task<IActionResult> CarDetail(int id)
        {
            var car = await _dbContext.CarModels
                .Include(c => c.Car)
                .Include(c => c.District).ThenInclude(d => d.City)
                .Include(c => c.CarImages.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == id);

            return View(new CarDetailViewModel
            {
                CarModel = car
            });
        }

        //***** Create *****//
        public IActionResult Create()
        {
            return View();
        }

        //***** Add Photos *****//

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPhotos(PhotoCreateViewModel model)
        {
            var carModel = await _dbContext.CarModels.Where(c => c.Id == model.CarModelId).FirstOrDefaultAsync();

            List<CarImage> carImages = new List<CarImage>();

            foreach (var image in model.Files)
            {
                if (image == null)
                {
                    ModelState.AddModelError("Files", "Select an image");
                    return View();
                }
                if (!image.IsSupported())
                {
                    ModelState.AddModelError("Files", "File is unsupported");
                    return View();
                }
                if (image.IsGreaterThanGivenSize(1024))
                {
                    ModelState.AddModelError(nameof(PhotoCreateViewModel.Files),
                        "File size cannot be greater than 1 mb");
                    return View();
                }

                var imgName = FileUtil.CreatedFile(Path.Combine(FileConstants.ImagePath, "cars"), image);
                carImages.Add(new CarImage { ImageName = imgName });
            }

            carModel.CarImages = carImages;

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("CarDetail", "Car", new { id = model.CarModelId });
        }

        //***** Delete Photo *****//
        public async Task<IActionResult> DeletePhoto(int id)
        {
            CarImage carImage = await _dbContext.CarImages.FindAsync(id);
            if (carImage == null)
            {
                return NotFound();
            }

            return View(carImage);
        }


        [HttpPost]
        [ActionName("DeletePhoto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhotoPost(int id)
        {
            CarImage carImage = await _dbContext.CarImages.FindAsync(id);
            if(carImage == null)
            {
                return NotFound();
            }

            carImage.IsDeleted = true;
            await _dbContext.SaveChangesAsync();

            //string path = Path.Combine(FileConstants.ImagePath, carImage.ImageName);
            //FileUtil.DeleteFile(path);

            //_dbContext.CarImages.Remove(carImage);
            //await _dbContext.SaveChangesAsync();


            return RedirectToAction("Index", "Car", new { id = id });
        }
    }
}
