using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Transactions
{
    /// <summary>
    /// Represents a User Level
    /// </summary>
    public class UserLevel
    {
        /// <summary>
        /// DB Id
        /// </summary>
        public Guid LevelId { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        ///  User level 0, 1, 2
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private UserLevel() { }
        /// <summary>
        /// Creates new user level for a user 
        /// Starts from level 0
        /// </summary>
        /// <param name="userId">User id of the user</param>
        public UserLevel(Guid userId)
        {
            UserId = userId;
            Value = 0;
        }
        /// <summary>
        /// Creates new user lvel for a user
        /// Starts from given level
        /// </summary>
        /// <param name="userId">User id of the user</param>
        /// <param name="value">Given level</param>
        public UserLevel(Guid userId, int value)
        {
            Value = value;
            UserId = userId;
        }
    }
}
