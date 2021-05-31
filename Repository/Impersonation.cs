using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.AspNetCore.Http;
using Microsoft.Win32.SafeHandles;

namespace StudentAPI.Repository
{
    public class Impersonation
    {

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeAccessTokenHandle phToken);

        public void SaveBioFile(IFormFile file, string filePath)
        {
            // Get the user token for the specified user, domain, and password using the 
            // unmanaged LogonUser method. 
            // The local machine name can be used for the domain name to impersonate a user on this machine.

            string domainName = "mystudent-server.database.windows.net";

            string userName = "studentadmin";

            string pass = "Rs809#5683@";

            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // Call LogonUser to obtain a handle to an access token. 
            SafeAccessTokenHandle safeAccessTokenHandle;
            bool returnValue = LogonUser(userName, domainName, pass,
                LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                out safeAccessTokenHandle);

            if (false == returnValue)
            {
                int ret = Marshal.GetLastWin32Error();
                throw new System.ComponentModel.Win32Exception(ret);
            }

            // Note: if you want to run as unimpersonated, pass
            //       'SafeAccessTokenHandle.InvalidHandle' instead of variable 'safeAccessTokenHandle'
            WindowsIdentity.RunImpersonated(
               safeAccessTokenHandle,
               // User action
               () =>
               {
                   try
                   {
                       if (file.Length > 0)
                       {
                           using (FileStream stream = File.Create(filePath))
                           {
                               file.CopyTo(stream);
                           }
                       }

                   }
                   catch (Exception ex)
                   {

                       throw ex;
                   }

               }
               );

        }
    }
}
