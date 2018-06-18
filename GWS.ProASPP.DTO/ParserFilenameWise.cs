// ----------------------------------------------------------------------
// <copyright file="ParserFilenameWise.cs" company="Global Wireless Solutions, Inc.">
//     Copyright (c) 2012, All Right Reserved, http://www.gwsolutions.com/
// </copyright>
//
// -----------------------------------------------------------------------

namespace GWS.ProASPP.DTO
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// This class is used to store the parser result by file name wise.
    /// </summary>
    [Serializable]
    public class ParserFilenameWise
    {
        /// <summary>
        /// Initializes a new instance of the ParserFilenameWise class.
        /// </summary>
        public ParserFilenameWise()
        {
            this.LstParserTechWise = new List<ParserTechWise>();
        }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the list of parser technology.
        /// </summary>
        public List<ParserTechWise> LstParserTechWise { get; set; }
    }
}
