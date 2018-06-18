// ----------------------------------------------------------------------
// <copyright file="ParserBandChannel.cs" company="Global Wireless Solutions, Inc.">
//     Copyright (c) 2012, All Right Reserved, http://www.gwsolutions.com/
// </copyright>
//
// ------------------------------------------------------------------------

namespace GWS.ProASPP.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is used to store parser Band Channel
    /// </summary>
    [Serializable]
    public class ParserBandChannel
    {
        /// <summary>
        /// Gets or sets the value of Band.
        /// </summary>
        public string Band { get; set; }

        /// <summary>
        /// Gets or sets the value of channel.
        /// </summary>
        public string Channel { get; set; }
    }
}
