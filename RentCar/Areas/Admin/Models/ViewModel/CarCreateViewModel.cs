using Microsoft.AspNetCore.Http;
using RentCar.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentCar.Areas.Admin.Models.ViewModel
{
    public class CarCreateViewModel
    {
        public string ModelName { get; set; }
        public District District { get; set; }
        public string Description { get; set; }
        public float OldPrice { get; set; }
        public float CurrentPrice { get; set; }
        public byte Rating { get; set; }
        public ushort ReleaseYear { get; set; }
        public string Color { get; set; }
        public float EngineSize { get; set; }
        public ushort HorsePower { get; set; }
        public string Fuel { get; set; }
        public uint MileAge { get; set; }
        public string Transmission { get; set; }
        public Car Car { get; set; }
        public IFormFile[] Files { get; set; }
    }
}
