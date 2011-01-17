﻿using System;
using System.Resources;
using System.Threading;
using Vts.SiteVisit.Resources;

namespace Vts.SiteVisit.Extensions
{
    /// <summary>
    /// Class to retrieve strings from a resources file
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Method to retrieve the correct language string for the VTS GUI
        /// </summary>
        /// <param name="stringType">Type of string in the interface(Tooltip, label, title etc)</param>
        /// <param name="stringName">Name of the string</param>
        /// <returns></returns>
        public static string GetLocalizedString(string stringType, string stringName)
        {
            string baseString = stringType;
            string name = stringName;

            ResourceManager rm = new ResourceManager("Vts.SiteVisit.Resources.Strings", typeof(Strings).Assembly);

            string s = rm.GetString(baseString + "_" + name, Thread.CurrentThread.CurrentCulture);
            if (s != null)
            {
                return s;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Uses the Enums from the VTS to look up strings for the GUI
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string GetLocalizedString(this Enum enumType)
        {
            string baseString = enumType.GetType().ToString();
            string type = baseString.Substring(baseString.IndexOf('.') + 1);
            string name = enumType.ToString();

            ResourceManager rm = new ResourceManager("Vts.SiteVisit.Resources.Strings", typeof(Strings).Assembly);

            string s = rm.GetString(type + "_" + name, Thread.CurrentThread.CurrentCulture);
            if (s != null)
                return s;
            else
                return "";
        }
    }
}