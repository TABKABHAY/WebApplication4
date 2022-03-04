using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4.ViewModel
{
    public class PostalCode
    {
        [Required]
        [StringLength(6, ErrorMessage = "Please Enter Valid Postal Code", MinimumLength = 6)]
        public string postalcode { get; set; }
    }

}
