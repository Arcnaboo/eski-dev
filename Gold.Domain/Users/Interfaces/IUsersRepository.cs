using System;
using System.Collections.Generic;
using System.Text;
using Gold.Core.Users;
using System.Linq;

namespace Gold.Domain.Users.Interfaces
{
    public interface IUsersRepository
    {
        void AddUserRef(UserRef userRef);
        void AddRefKod(ReferansCode referansCode);
        void AddUserLevel(UserLevel userLevel);
        void AddSilverBalance(SilverBalance silverBalance);
        void AddCekilis(Cekilis cekilis);
        void AddCekilisHakki(CekilisHak cekHak);
        void AddKimlikInfo(KimlikInfo info);
        void AddLog(InternalLog log);
        void AddUser(User user);
        void AddForgotPass(ForgotPassword forgot);
        void AddNotification(Notification notification);
        void AddBannedIp(BannedIp bannedIp);
        bool RemoveKimlikInfo(KimlikInfo info);
        bool RemoveForgotPass(ForgotPassword forgot);
        bool RemoveUsers(User user);

        void AddChange(ProfileChange change);
        void AddLogin(Login login);

        Login GetLogin(Guid loginId);
        
        Notification DeleteNotification(Notification notification);

        KimlikInfo GetKimlikInfoByUser(Guid userId);
        KimlikInfo GetKimlikInfoByItsId(Guid kimlikInfoId);
        UserLevel GetUserLevel(Guid userId);
        SilverBalance GetSilverBalance(Guid userId);
        ReferansCode GetUserReferanse(Guid userId);
        IQueryable<UserRef> GetUserRefs();
        IQueryable<ReferansCode> GetReferansCodes();
        IQueryable<KimlikInfo> GetAllKimlikInfos();
        IQueryable<User> GetAllUsers();
        IQueryable<ForgotPassword> GetAllForgotPasswords();
        IQueryable<Notification> GetAllNotifications();
        IQueryable<Login> GetAllLogins();
        IQueryable<ProfileChange> GetAllChanges();
        IQueryable<InternalLog> GetAllLogsOfUser(Guid userid);
        IQueryable<BannedIp> GetAllBannedIps();
        IQueryable<Cekilis> GetAllCekilis();
        IQueryable<CekilisHak> GetAllCekilisHaks();
        void SaveChanges();
    }
}
