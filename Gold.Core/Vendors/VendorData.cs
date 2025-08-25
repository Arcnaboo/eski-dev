using System;
using System.Collections.Generic;
using System.Text;


namespace Gold.Core.Vendors
{
    /// <summary>
    /// Stores data related to a vendor
    /// </summary>
    public class VendorData
    {
        /// <summary>
        /// db id
        /// </summary>
        public Guid VendorDataId { get; set; }
        /// <summary>
        /// generally vendor id
        /// type string for flexibility
        /// </summary>
        public string RelatedId { get; set; }
        /// <summary>
        /// Vendor fintag account TL suffix
        /// </summary>
        public string TLSuffix { get; set; }
        /// <summary>
        /// Vendor gold siffox
        /// </summary>
        public string GOLDSuffix { get; set; }
        /// <summary>
        /// Vendor platin suffix
        /// </summary>
        public string PLTSuffix { get; set; }
        /// <summary>
        /// Vendor silver suffix
        /// </summary>
        public string SLVSuffix { get; set; }
        /// <summary>
        /// Vendor account number
        /// </summary>
        public string AccountNumber { get; set; }
        /// <summary>
        /// Vendor account suffix
        /// </summary>
        public string AccountSuffix { get; set; }
        /// <summary>
        /// Automatic true iff vendor's buy system should be auto
        /// </summary>
        public bool? Automatic { get; set; }
        /// <summary>
        /// depreciated column
        /// </summary>
        public string VendorHavaleCode { get; set; }
        /// <summary>
        /// TL Suffix's balance at KT
        /// </summary>
        public decimal? KTTLBalance { get; set; }
        /// <summary>
        /// GOLD suffix's balance at KT
        /// </summary>
        public decimal? KTGLDBalance { get; set; }
        /// <summary>
        /// SILVER suffix's balance at KT
        /// </summary>
        public decimal? KTSLVBalance { get; set; }
        /// <summary>
        /// AutomaticSell true iff vendor's sell system should be auto
        /// </summary>
        public bool? AutomaticSell { get; set; }
        /// <summary>
        /// If vendor's KTGOLDBalance is below threshold automatic buys Gold
        /// </summary>
        public decimal? BalanceThresholdGold { get; set; }
        /// <summary>
        /// If true then BalanceThreshold works during automatic buy
        /// </summary>
        public bool? ThresholdGoldActive { get; set; }
        /// <summary>
        /// If vendor's KTSLVBalance is below threshold automatic buys Silver
        /// </summary>
        public decimal? BalanceThresholdSilver { get; set; }
        /// <summary>
        /// If true then BalanceThreshold works during automatic buy
        /// </summary>
        public bool? ThresholdSilverActive { get; set; }

        private VendorData() { }

        /// <summary>
        /// creates new vendor data
        /// </summary>
        /// <param name="rel">vendor id</param>
        /// <param name="tl">tl suffix</param>
        /// <param name="gld">gld suffix</param>
        /// <param name="slv">silver suffix</param>
        /// <param name="num">account number</param>
        /// <param name="suff">account suffix</param>
        public VendorData(string rel, string tl, string gld, string slv, string num, string suff)
        {
            RelatedId = rel;
            Automatic = false;
            AutomaticSell = false;
            KTGLDBalance = 0;
            KTSLVBalance = 0;
            KTTLBalance = 0;
            BalanceThresholdSilver = 1000;
            BalanceThresholdGold = 20;
            ThresholdGoldActive = false;
            ThresholdSilverActive = false;
            UpdateAccounts(tl, gld, slv, num, suff);

        }

        /// <summary>
        /// Updates vendor date
        /// </summary>
        /// <param name="tl">tl suffix</param>
        /// <param name="gld">gld suffix</param>
        /// <param name="slv">silver suffix</param>
        /// <param name="num">account number</param>
        /// <param name="suff">account suffix</param>
        public void UpdateAccounts(string tl, string gld, string slv, string num, string suff)
        {
            if (tl != null)
                TLSuffix = tl;
            if (gld != null)
                GOLDSuffix = gld;
            if (slv != null)
                SLVSuffix = slv;
            if (num != null)
                AccountNumber = num;
            if (suff != null)
                AccountSuffix = slv;
        }

        /// <summary>
        /// Updates Automatic status
        /// </summary>
        /// <param name="status">true or false</param>
        public void SetAutomaticBuy(bool status)
        {
            Automatic = status;
        }

        /// <summary>
        /// Updates Automatic for Sell
        /// </summary>
        /// <param name="status">true or false</param>
        public void SetAutomaticSell(bool status)
        {
            AutomaticSell = status;
        }

        /// <summary>
        /// Activates or deactivates threshold for auto buy gold
        /// </summary>
        /// <param name="activeStatus">True or False</param>
        /// <param name="thresh">must be valid if activeStatus = true</param>
        public void SetThresholdGold(bool activeStatus, decimal? thresh = null)
        {
            ThresholdGoldActive = activeStatus;
            if (ThresholdGoldActive.Value)
            {
                if (thresh == null || !thresh.HasValue)
                {
                    throw new ArgumentException("Threshold can not be null for active status");
                }
                BalanceThresholdGold = thresh;
            }
        }

        /// <summary>
        /// Activates or deactivates threshold for auto buy silver
        /// </summary>
        /// <param name="activeStatus">True or False</param>
        /// <param name="thresh">must be valid if activeStatus = true</param>
        public void SetThresholdSilver(bool activeStatus, decimal? thresh = null)
        {
            ThresholdSilverActive = activeStatus;
            if (ThresholdSilverActive.Value)
            {
                if (thresh == null || !thresh.HasValue)
                {
                    throw new ArgumentException("Threshold can not be null for active status");
                }
                BalanceThresholdSilver = thresh;
            }
        }
    }

}
