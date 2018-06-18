// ----------------------------------------------------------------------
// <copyright file="ParserHelper.cs" company="Global Wireless Solutions, Inc.">
//     Copyright (c) 2012, All Right Reserved, http://www.gwsolutions.com/
// </copyright>
//
// -----------------------------------------------------------------------

namespace GWS.ProASPP.DTO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text;

    /// <summary>
    /// This class is used to help for the parser object for serialize and deserialize
    /// </summary>
    public class ParserHelper
    {
        /// <summary>
        /// This function is used to convert parser object to binary file.
        /// </summary>
        /// <param name="fullFilePath">Full path of file.</param>
        /// <param name="mffileName">MF file name.</param>
        /// <param name="gsmString">GSM channel string</param>
        /// <param name="wcdmaString">WCDMA channel string</param>
        /// <param name="cdmaString">CDMA channel string</param>
        /// <param name="lteString">LTE channel string</param>
        public void ParserTxtBinaryFile(string fullFilePath, string mffileName, string gsmString, string wcdmaString, string cdmaString, string lteString)
        {
            List<ParserFilenameWise> lstParserFilenameWise = new List<ParserFilenameWise>();
            BinaryFormatter formatter = new BinaryFormatter();

            if (File.Exists(fullFilePath))
            {
                FileStream fileRead = new FileStream(fullFilePath, FileMode.Open);
                if (fileRead.Length > 0)
                {
                    lstParserFilenameWise = (List<ParserFilenameWise>)formatter.Deserialize(fileRead);
                }

                fileRead.Close();
            }
            else
            {
                FileStream fileCreate = File.Create(fullFilePath);
                fileCreate.Close();
            }

            ParserFilenameWise scnrParseTXT = new ParserFilenameWise();
            scnrParseTXT.FileName = mffileName;
            this.ParserTechWiseObject(scnrParseTXT, lteString, "LTE");
            this.ParserTechWiseObject(scnrParseTXT, wcdmaString, "WCDMA");
            this.ParserTechWiseObject(scnrParseTXT, cdmaString, "CDMA");
            this.ParserTechWiseObject(scnrParseTXT, gsmString, "GSM");

            lstParserFilenameWise.Add(scnrParseTXT);

            FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate);
            formatter.Serialize(fs, lstParserFilenameWise);
            fs.Close();
        }

        /// <summary>
        /// This function is used to cover the string into technique.
        /// </summary>
        /// <param name="parserFilenameWise">Parser file name wise object.</param>
        /// <param name="parserString">Parser value string.</param>
        /// <param name="technolgyName">Technology name.</param>
        public void ParserTechWiseObject(ParserFilenameWise parserFilenameWise, string parserString, string technolgyName)
        {
            if (string.IsNullOrEmpty(parserString))
            {
                return;
            }

            ParserTechWise scnrTech = new ParserTechWise();
            scnrTech.Technology = technolgyName;
            parserFilenameWise.LstParserTechWise.Add(scnrTech);
            parserString = parserString.Replace("(:", string.Empty).Replace("(", string.Empty);
            string[] spliter = new string[] { ")" };
            List<string> lstBandCahnnel = parserString.Split(spliter, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstBandCahnnel.Any())
            {
                char[] ch = new char[] { ':' };
                foreach (string bankChannel in lstBandCahnnel)
                {
                    string[] final = bankChannel.Split(ch);
                    if (final.Length > 1)
                    {
                        ParserBandChannel scnrParserBandChannel = new ParserBandChannel();
                        scnrParserBandChannel.Band = final[0];
                        scnrParserBandChannel.Channel = final[1];
                        scnrTech.LstParserbandChannel.Add(scnrParserBandChannel);
                    }
                    
                }
            }
        }

        /// <summary>
        /// This function is used to deserialize Parser filename.
        /// </summary>
        /// <param name="fullFilePath">Full file path.</param>
        /// <returns>List of ParserFilenameWise</returns>
        public List<ParserFilenameWise> DeserializeObject(string fullFilePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            List<ParserFilenameWise> lstParserFilenameWise = new List<ParserFilenameWise>();
            if (File.Exists(fullFilePath))
            {
                FileStream fileRead = new FileStream(fullFilePath, FileMode.Open);
                if (fileRead.Length > 0)
                {
                    lstParserFilenameWise = (List<ParserFilenameWise>)formatter.Deserialize(fileRead);
                }

                fileRead.Close();
            }

            return lstParserFilenameWise;
        }
    }
}
