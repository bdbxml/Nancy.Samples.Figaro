using System.Configuration;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;

namespace Nancy.Demos.Figaro
{
    public class UsersModule : NancyModule
    {
        private readonly FigaroDataContext context;

        /// <summary>
        /// The main module for this application.
        /// </summary>
        /// <param name="dataContext"></param>
        public UsersModule(FigaroDataContext dataContext) : base("/users")
        {
            context = dataContext;
        }
    }
}
