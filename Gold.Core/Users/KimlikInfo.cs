using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Users
{
    /// <summary>
    /// Represents an Identification
    /// </summary>
    public class KimlikInfo
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid KimlikInfoId { get; set; }
        /// <summary>
        /// User id
        /// </summary>
        public Guid UserId { get; set; }
        /// <summary>
        /// Image photo of the Identification (passport or national id)
        /// </summary>
        public string KimlikImageLink { get; set; }
        /// <summary>
        /// DateTime of the upload
        /// </summary>
        public DateTime UploadDateTime { get; set; }
        /// <summary>
        /// True iff identification is validated by an admin
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private KimlikInfo() { }
        /// <summary>
        /// Creates new kimlikinfo
        /// </summary>
        /// <param name="userId">user id</param>
        /// <param name="imageLink">identification photo link</param>
        public KimlikInfo(Guid userId, string imageLink)
        {
            UserId = userId;
            KimlikImageLink = imageLink;
            Confirmed = false;
            UploadDateTime = DateTime.Now;
        }

        /// <summary>
        /// Confirms the identification
        /// </summary>
        public void Confirm()
        {
            Confirmed = true;
        }
    }
}
