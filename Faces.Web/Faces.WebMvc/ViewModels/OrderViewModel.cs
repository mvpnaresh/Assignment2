using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Faces.WebMvc.ViewModels
{
    public class OrderViewModel
    {
        [Display(Name = "Order Id")]
        public Guid OrderId { get; set; }

        [Display(Name = "Email")]
        public string UserEmail { get; set; }

        [Display(Name = "Image File")]
        public IFormFile File { get; set; }

        [Display(Name = "Image Url")]
        public string PictureUrl { get; set; }

        [Display(Name = "Order Status")]
        public StatusViewModel Status { get; set; }

        public byte[] ImageData { get; set; }

        public string ImageString { get; set; }

        public List<OrderDetailViewModel> OrderDetails { get; set; }
    }
}
