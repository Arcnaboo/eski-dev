using System;
using System.Collections.Generic;
using System.Text;

namespace Gold.Core.Vendors
{
    /// <summary>
    /// Represents a goldtag code
    /// Some vendor actions requires valid gcode
    /// </summary>
    public class Gcode
    {
        /// <summary>
        /// DB id
        /// </summary>
        public Guid GcodeId { get; set; }
        /// <summary>
        /// Code value
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// True iff code is used already
        /// </summary>
        public bool Used { get; set; }
        /// <summary>
        /// Vendor id
        /// </summary>
        public Guid GeneratedFor { get; set; }
        /// <summary>
        /// Datetime code generated
        /// </summary>
        public DateTime GeneratedDateTime { get; set; }
        /// <summary>
        /// Datetime code is valid
        /// </summary>
        public DateTime ValidUntil { get; set; }
        /// <summary>
        /// Private Constructor Required for EFCore Database framework
        /// </summary>
        private Gcode() { }
        /// <summary>
        /// Creates new gcode
        /// </summary>
        /// <param name="code">gcode</param>
        /// <param name="generatedFor">vendor id</param>
        public Gcode(string code, Guid generatedFor)
        {
            Code = code;
            Used = false;
            GeneratedFor = generatedFor;
            GeneratedDateTime = DateTime.Now;
            ValidUntil = GeneratedDateTime.AddSeconds(300);
        }

        /// <summary>
        /// String representation
        /// </summary>
        /// <returns>a string representation of this class</returns>
        public override string ToString()
        {
            return string.Format("Gcode: {0} - Valid Until: {1}", Code, ValidUntil.ToString());
        }
    }
}
