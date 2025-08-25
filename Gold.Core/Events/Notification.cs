using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Events
{
    /// <summary>
    /// Represents a notification for the app
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Database unique id
        /// </summary>
        public Guid NotificationId { get; set; }
        /// <summary>
        /// User id whom this notification for
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// message of the notification
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// true iff user seen the notification
        /// </summary>
        public bool Seen { get; set; }
        /// <summary>
        /// Notification creation time
        /// </summary>
        public DateTime NotificationDateTime { get; set; }
        /// <summary>
        /// Notification seen time
        /// </summary>
        public DateTime? SeenDateTime { get; set; }
        /// <summary>
        /// its true iff this notificaiton can be clicked by the user
        /// </summary>
        public bool Interactable { get; set; }
        /// <summary>
        /// type of the notification (info / transfer...)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// this is a valid id if the notificaiton is interactable
        /// </summary>
        public string RelatedId { get; set; }
        /// <summary>
        /// User of this notification
        /// </summary>
        public virtual User2 User { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Notification() { }

        /// <summary>
        /// Creates new notification
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="message">The message</param>
        /// <param name="seen">seen should be false</param>
        /// <param name="inter">is it interactable</param>
        /// <param name="type">what type of notification</param>
        /// <param name="id">related id if interactable</param>
        public Notification(Guid userId, string message, bool seen, bool inter, string type, string id)
        {
            Seen = seen;
            UserId = userId;
            Message = message;
            Interactable = inter;
            Type = type;
            RelatedId = id;
            NotificationDateTime = DateTime.Now;
        }

        
    }
}
