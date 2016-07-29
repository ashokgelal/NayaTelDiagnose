using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActivateUsersModule.Model;

namespace ActivateUsersModule.Services
{
    public delegate void userResult(ActiveUser user,Boolean Stop);
    public interface IActiveUserService
    {
       

        Task<IEnumerable<ActiveUser>> getActiveUsersAsync();
        void getUsers(Double timeOut,userResult user);
        void stopTest();


    }
}
