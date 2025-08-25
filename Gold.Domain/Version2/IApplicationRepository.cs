using Gold.Core.Version2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gold.Domain.Version2
{
    public interface IApplicationRepository
    {

        void AddRegistration(UserRegistration registration);
        void AddRegisteredUser(RegisteredUser registeredUser);

        IQueryable<RegisteredUser> GetRegisteredUsers();
        IQueryable<UserRegistration> GetUserRegistrations();
        UserRegistration GetRegistrationWithCode(int code);
        RegisteredUser GetRegisteredUserWithGuid(Guid id);
        RegisteredUser GetRegisteredUserWithMemberId(int memberId);
        
        void DeleteRegistration(UserRegistration registration);
    }
}
