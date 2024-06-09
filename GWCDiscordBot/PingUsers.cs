using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GWCDiscordBot
{
    public class PingUsers
    {
        private readonly UserChecker _userChecker;

        public PingUsers(UserChecker userChecker) 
        {
            _userChecker = userChecker;
        }

        public async Task SendPings()
        {

        }
    }
}
