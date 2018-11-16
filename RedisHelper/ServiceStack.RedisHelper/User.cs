using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceStack.RedisHelper
{
    public class User
    {
        public string ID { get; set; }

        public string UserName { get; set; }

        public string PassWord { get; set; }

        public string Token { get; set; }
    }
}
