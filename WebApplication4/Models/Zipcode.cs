using System;
using System.Collections.Generic;

#nullable disable

namespace WebApplication4
{
    public partial class Zipcode
    {
        public int Id { get; set; }
        public string ZipcodeValue { get; set; }
        public int CityId { get; set; }
        public virtual City City { get; set; }
    }
}
