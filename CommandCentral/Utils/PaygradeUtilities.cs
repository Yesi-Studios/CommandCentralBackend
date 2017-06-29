using CommandCentral.Entities.ReferenceLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandCentral.Utils
{
    /// <summary>
    /// Provides methods for dealing with paygrades.
    /// </summary>
    public static class PaygradeUtilities
    {

        /// <summary>
        /// Returns a boolean indicating if this paygrade is that of an officer or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsOfficerPaygrade(this Paygrade paygrade)
        {
            return paygrade.Value.StartsWith("CWO") || (paygrade.Value.Contains("O") && !paygrade.Value.Contains("C"));
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is an enlisted paygrade or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsEnlistedPaygrade(this Paygrade paygrade)
        {
            return paygrade.Value.Contains("E") && !paygrade.Value.Contains("O");
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a civilian paygrade or not.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsCivilianPaygrade(this Paygrade paygrade)
        {
            return paygrade.Value.StartsWith("GG") || paygrade.Value.Equals("CON");
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a chief paygrade.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsChief(this Paygrade paygrade)
        {
            return paygrade.IsEnlistedPaygrade() && new[] { 7, 8, 9 }.Contains(Int32.Parse(paygrade.Value.Where(char.IsNumber).First().ToString()));
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a petty officer.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsPettyOfficer(this Paygrade paygrade)
        {
            return paygrade.IsEnlistedPaygrade() && new[] { 4, 5, 6 }.Contains(Int32.Parse(paygrade.Value.Where(char.IsNumber).First().ToString()));
        }

        /// <summary>
        /// Returns a boolean indicating if this paygrade is a seaman.
        /// </summary>
        /// <param name="paygrade"></param>
        /// <returns></returns>
        public static bool IsSeaman(this Paygrade paygrade)
        {
            return paygrade.IsEnlistedPaygrade() && new[] { 1, 2, 3 }.Contains(Int32.Parse(paygrade.Value.Where(char.IsNumber).First().ToString()));
        }

    }
}
