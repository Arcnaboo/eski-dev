using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gold.Core.Users;
using Newtonsoft.Json;

namespace Gold.Api.Models.Users
{
    public class NotificationModel
    {
//        [JsonProperty("notificationid")]
        public Guid NotificationId { get; set; }

        //[JsonProperty("userid")]
        public Guid UserId { get; set; }

        //[JsonProperty("message")]
        public string Message { get; set; }

        //[JsonProperty("notificationdatetime")]
        public DateTime NotificationDateTime { get; set; }

        //[JsonProperty("seendatetime")]
        public DateTime? SeenDateTime { get; set; }

        //[JsonProperty("delivered")]
        public bool Delivered { get; set; }

        //[JsonProperty("seen")]
        public bool Seen { get; set; }

       //[JsonProperty("interactable")]
        public bool Interactable { get; set; }

        //[JsonProperty("type")]
        public string Type { get; set; }

        //[JsonProperty("relatedid")]
        public string RelatedId { get; set; }

       // [JsonProperty("photo")]
        public string Photo { get; set; }

        public NotificationModel(Notification notification)
        {
            NotificationId = notification.NotificationId;
            UserId = notification.UserId;
            Message = notification.Message;
            NotificationDateTime = notification.NotificationDateTime;
            SeenDateTime = notification.SeenDateTime;
            Delivered = notification.Delivered;
            Seen = notification.Seen;
            Interactable = notification.Interactable;
            Type = notification.Type;
            RelatedId = notification.RelatedId;

        }

        public void SetPhoto(string uri)
        {
            Photo = uri;
        }
    }
}
