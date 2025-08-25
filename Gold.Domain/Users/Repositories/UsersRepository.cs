using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gold.Core.Users;
using Gold.Domain.Users.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Gold.Domain.Users.Repositories
{
    public class UsersRepository : IUsersRepository, IDisposable
    {

        private readonly UserDbContext Context;

        public UsersRepository()
        {
            Context = new UserDbContext();
        }

        UserLevel IUsersRepository.GetUserLevel(Guid userId)
        {
            return Context.UserLevels.Where(x => x.UserId == userId).FirstOrDefault();
        }

        void IUsersRepository.AddUserLevel(UserLevel userLevel)
        {
            Context.UserLevels.Add(userLevel);
        }

        void IUsersRepository.AddBannedIp(BannedIp bannedIp)
        {
            Context.BannedIps.Add(bannedIp);
        }

        void IUsersRepository.AddCekilis(Cekilis cekilis)
        {
            Context.Cekilis.Add(cekilis);
        }

        void IUsersRepository.AddCekilisHakki(CekilisHak cekHak)
        {
            Context.CekilisHaks.Add(cekHak);
        }

        void IUsersRepository.AddChange(ProfileChange change)
        {
            Context.ProfileChanges.Add(change);
        }

        void IUsersRepository.AddForgotPass(ForgotPassword forgot)
        {
            Context.ForgotPasswords.Add(forgot);
        }

        void IUsersRepository.AddKimlikInfo(KimlikInfo info)
        {
            Context.KimlikInfos.Add(info);
        }

        void IUsersRepository.AddLog(InternalLog log)
        {
            Context.Logs.Add(log);
        }

        void IUsersRepository.AddLogin(Login login)
        {
            Context.Logins.Add(login);
        }

        void IUsersRepository.AddNotification(Notification notification)
        {
            Context.Notifications.Add(notification);
        }

        void IUsersRepository.AddSilverBalance(SilverBalance silverBalance)
        {
            Context.SilverBalances.Add(silverBalance);
        }

        void IUsersRepository.AddUser(User user)
        {

            Context.Users.Add(user);


        }

        Notification IUsersRepository.DeleteNotification(Notification notification)
        {
            return Context.Notifications.Remove(notification).Entity;
        }

        IQueryable<BannedIp> IUsersRepository.GetAllBannedIps()
        {
            return Context.BannedIps;
        }

        IQueryable<Cekilis> IUsersRepository.GetAllCekilis()
        {
            return Context.Cekilis;
        }

        IQueryable<CekilisHak> IUsersRepository.GetAllCekilisHaks()
        {
            return Context.CekilisHaks;
        }

        IQueryable<ProfileChange> IUsersRepository.GetAllChanges()
        {
            return Context.ProfileChanges;
        }

        IQueryable<ForgotPassword> IUsersRepository.GetAllForgotPasswords()
        {
            return Context.ForgotPasswords;
        }

        IQueryable<KimlikInfo> IUsersRepository.GetAllKimlikInfos()
        {
            return Context.KimlikInfos;
        }

        IQueryable<Login> IUsersRepository.GetAllLogins()
        {
            return Context.Logins;
        }

        IQueryable<InternalLog> IUsersRepository.GetAllLogsOfUser(Guid userid)
        {
            var query = Context.Logs.Where(x => x.UserId == userid).OrderByDescending(x => x.LogDateTime);

            return query;
        }

        IQueryable<Notification> IUsersRepository.GetAllNotifications()
        {
            return Context.Notifications;
        }


        IQueryable<User> IUsersRepository.GetAllUsers()
        {
            return Context.Users;
        }

        KimlikInfo IUsersRepository.GetKimlikInfoByItsId(Guid kimlikInfoId)
        {
            return Context.KimlikInfos.Where(x => x.KimlikInfoId == kimlikInfoId).FirstOrDefault();
        }

        KimlikInfo IUsersRepository.GetKimlikInfoByUser(Guid userId)
        {
            return Context.KimlikInfos.Where(x => x.UserId == userId).FirstOrDefault();
        }

        Login IUsersRepository.GetLogin(Guid loginId)
        {
            return Context.Logins.Where(x => x.LoginId == loginId).FirstOrDefault();
        }

        SilverBalance IUsersRepository.GetSilverBalance(Guid userId)
        {
            return Context.SilverBalances.Where(x => x.UserId == userId).FirstOrDefault();
        }

        bool IUsersRepository.RemoveForgotPass(ForgotPassword forgot)
        {
            Context.ForgotPasswords.Remove(forgot);
            return true;
        }

        bool IUsersRepository.RemoveKimlikInfo(KimlikInfo info)
        {
            Context.KimlikInfos.Remove(info);
            return true;
        }

        bool IUsersRepository.RemoveUsers(User user)
        {
            Context.Users.Remove(user);
            return true;
        }

        void IUsersRepository.SaveChanges()
        {
            Context.SaveChanges();
        }

        void IUsersRepository.AddRefKod(ReferansCode referansCode)
        {
            Context.ReferansCodes.Add(referansCode);
        }

        ReferansCode IUsersRepository.GetUserReferanse(Guid userId)
        {
            return Context.ReferansCodes.Where(x => x.UserId == userId).FirstOrDefault();
        }

        IQueryable<ReferansCode> IUsersRepository.GetReferansCodes()
        {
            return Context.ReferansCodes;
        }

        void IUsersRepository.AddUserRef(UserRef userRef)
        {
            Context.UserRefs.Add(userRef);
        }

        IQueryable<UserRef> IUsersRepository.GetUserRefs()
        {
            return Context.UserRefs;
        }

        void IDisposable.Dispose()
        {
            Context.DisposeAsync();    
        }
    }
}
