using WebApplication4.Models;
using Newtonsoft.Json;

namespace WebApplication4.ViewModel
{
    public class BlockCustomerData
    {
        
        public  User user { get; set; }
      
        public FavoriteAndBlocked favoriteAndBlocked { get; set; }
    }
}
