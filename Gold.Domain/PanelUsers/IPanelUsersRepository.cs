using Gold.Core.PanelUsers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Domain.PanelUsers
{
    public interface IPanelUsersRepository
    {

        void AddNewPanelUser(PanelUser panelUser);

        void SaveChanges();

       
    }
}
