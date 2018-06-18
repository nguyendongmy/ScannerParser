// ----------------------------------------------------------------------
// <copyright file="ParserTechWise.cs" company="Global Wireless Solutions, Inc.">
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
    /// This class is used to store the File with technology and band wise.
    /// </summary>
    [Serializable]
    public class ParserTechWise
    {
        /// <summary>
        /// Initializes a new instance of the ParserTechWise class.
        /// </summary>
        public ParserTechWise()
        {
            this.LstParserbandChannel = new List<ParserBandChannel>();
        }

        /// <summary>
        /// Gets or sets the value of Technology
        /// </summary>
        public string Technology { get; set; }

        /// <summary>
        /// Gets or sets the value of List of parser band channel.
        /// </summary>
        public List<ParserBandChannel> LstParserbandChannel { get; set; }
    }
}
