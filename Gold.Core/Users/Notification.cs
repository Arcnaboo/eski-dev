using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
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
        /// True if notification is delivered
        /// </summary>
        public bool Delivered { get; set; }
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
        /// photo to be seen in the notification
        /// </summary>
        public string Photo { get; set; }
        /// <summary>
        /// user related to notification
        /// </summary>
        public virtual User User { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Notification() { }
        /// <summary>
        /// Creates new notification
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="message">The message</param>
        /// <param name="inter">is it interactable</param>
        /// <param name="type">what type of notification</param>
        /// <param name="id">related id if interactable</param>
        /// <param name="photo">photo of the notification</param>
        public Notification(Guid userId, string message, bool inter, string type, string id, string photo)
        {
            Seen = false;
            Delivered = false;
            UserId = userId;
            Message = message;
            Interactable = inter;
            Type = type;
            RelatedId = id;
            NotificationDateTime = DateTime.Now;
            Photo = photo;
        }

        /// <summary>
        /// to be called when user sees notification
        /// </summary>
        public void SeeNotification()
        {
            Seen = true;
            SeenDateTime = DateTime.Now;
        }

        /// <summary>
        /// to be called when notification delivered but not seen yet
        /// </summary>
        public void DeliverNotification()
        {
            Delivered = true;

        }

    }
}
