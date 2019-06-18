using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsPrincipalDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            ExploreWindowsAccount();
            SetPrincipal();
            UsePrincipal();
        }

        private static void UsePrincipal()
        {
            var principal = Thread.CurrentPrincipal;
            Console.WriteLine(principal.Identity.Name);

            // downcast to specific implementation
            var id = principal.Identity as WindowsIdentity;
            Console.WriteLine("Token handle: {0}", id.Token);
        }

        private static void SetPrincipal()
        {
            Thread.CurrentPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
        }

        private static void ExploreWindowsAccount()
        {
            // creating a WindowsIdentity from the current process token
            var id = WindowsIdentity.GetCurrent();

            // accessing the Windows account name
            Console.WriteLine("\nName:");
            Console.WriteLine(id.Name);

            // getting the SID for that account name
            Console.WriteLine("\nSID:");
            var sid = new NTAccount(id.Name).Translate(typeof(SecurityIdentifier));
            Console.WriteLine(sid.Value);

            // accessing the Windows groups of the user
            var groups = id.Groups;

            Console.WriteLine("\nGroups (SIDs):");
            foreach (var group in groups)
            {
                Console.WriteLine(group.Value);
            }

            // convert all group SIDs to their corresponding names
            var groupNames = id.Groups.Translate(typeof(NTAccount));

            Console.WriteLine("\nGroups (Names):");
            foreach (var group in groupNames)
            {
                Console.WriteLine(group.Value);
            }

            // playing with well known SIDs
            var localAdmins = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var domainAdmins = new SecurityIdentifier(WellKnownSidType.AccountDomainAdminsSid, id.User.AccountDomainSid);
            var interactiveUsers = new SecurityIdentifier(WellKnownSidType.InteractiveSid, null);
            var rdpUsers = new SecurityIdentifier(WellKnownSidType.BuiltinRemoteDesktopUsersSid, null);

            // using WindowsPrincipal to query for groups
            var p = new WindowsPrincipal(id);

            Console.WriteLine("Local Admin:  {0}", p.IsInRole(localAdmins));
            Console.WriteLine("Domain Admin: {0}", p.IsInRole(domainAdmins));
            Console.WriteLine("Interactive:  {0}", p.IsInRole(interactiveUsers));
            Console.WriteLine("RDP User:     {0}", p.IsInRole(rdpUsers));
        }
    }
}
