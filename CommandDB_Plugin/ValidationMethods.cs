using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Threading.Tasks;
using AtwoodUtils;
using System.Data;
using MySql.Data.MySqlClient;
using MySql.Data;

namespace CommandDB_Plugin
{
    /// <summary>
    /// Provides a number of methods for determing if a given value matches a format.
    /// </summary>
    public static class ValidationMethods
    {
        /// <summary>
        /// Determines if the given string can be parsed into a GUID.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidGuid(object value)
        {
            Guid id;
            return Guid.TryParse(value as string, out id);
        }

        /// <summary>
        /// Determines if the given string matches requirements for a password.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidPassword(object value)
        {
            if (value.ToString().Length < 6 || value.ToString().Length > 40)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a string is considered to be an email address by the .net framework.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidEmailAddress(object emailAddress)
        {
            try
            {
                MailAddress address = new MailAddress(emailAddress.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if all the strings contained in a list of strings are valid change event IDs.  This can take both a JSON serialized list of strings or a list of strings.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool AreValidChangeEventIDs(object value)
        {
            List<string> values = new List<string>();
            if (value.GetType() == typeof(string))
                values = value.ToString().Deserialize<List<string>>();
            else
                if (value.GetType() == typeof(List<string>))
                    values = value as List<string>;
                else
                    return false;

            return values.All(x => ChangeEvents.ChangeEventsCache.ContainsKey(x));
        }

        public static bool IsValidBilletID(object value)
        {
            try
            {
                return new Billets.Billet() { ID = value as string }.DBExists(true).Result;
            }
            catch
            {
                throw;
            }
        }

        public static bool AreValidEmailAddresses(object value)
        {
            List<string> values = new List<string>();
            if (value.GetType() == typeof(string))
                values = value.ToString().Deserialize<List<string>>();
            else
                if (value.GetType() == typeof(List<string>))
                    values = value as List<string>;
                else
                    return false;

            return values.All(x => IsValidEmailAddress(x.ToString()));
        }

        /// <summary>
        /// Determines if a string is considered to be an email address by the .net framework AND if the host is the dod mail provider.
        /// </summary>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public static bool IsValidDODEmailAddress(object emailAddress)
        {
            try
            {
                MailAddress address = new MailAddress(emailAddress.ToString());

                if (!address.Host.SafeEquals(EmailHelper.RequiredDODEmailHost))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Determines if a string matches our rules for a username.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidUsername(object value)
        {
            if (value.ToString().Length < 6 || value.ToString().Length > 20)
                return false;

            if (!value.ToString().All(char.IsLetterOrDigit))
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a string is both a double and [-90,90]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidLatitude(string str)
        {
            double coord;
            if (!double.TryParse(str, out coord))
                return false;

            if (coord < -90 || coord > 90)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a double is [-90,90]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidLatitude(double value)
        {
            if (value < -90 || value > 90)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a string is a double and [-180,180]
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsValidLongtitude(string str)
        {
            double coord;
            if (!double.TryParse(str, out coord))
                return false;

            if (coord < -180 || coord > 180)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a double is [-180,180]
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidLongtitude(double value)
        {
            if (value < -180 || value > 180)
                return false;

            return true;
        }

        /// <summary>
        /// Determines if a phone number is valid.  To be valid, the first number must be 1 and there must be 11 numbers.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static bool IsValidPhoneNumber(object value)
        {
            if (!(value is string))
                return false;

            string number = value as string;

            if (!number.All(char.IsDigit))
                return false;

            if (Convert.ToInt32(number.First().ToString()) != 1)
                return false;

            if (number.Count() != 11)
                return false;

            return true;
        }

        public static bool AreValidPhoneNumbers(object value)
        {
            List<string> values = new List<string>();
            if (value.GetType() == typeof(string))
                values = value.ToString().Deserialize<List<string>>();
            else
                if (value.GetType() == typeof(List<string>))
                    values = value as List<string>;
                else
                    return false;

            return values.All(x => IsValidPhoneNumber(x.ToString()));
        }

        /// <summary>
        /// Determines if a string is a valid URI.
        /// </summary>
        /// <param name="website"></param>
        /// <returns></returns>
        public static bool IsValidWebsite(string website)
        {
            return System.Uri.IsWellFormedUriString(website, UriKind.Absolute);
        }

        /// <summary>
        /// Determines if a string can be parsed into a DateTime object.
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static bool IsValidDateTime(object value)
        {
            DateTime temp;
            return (DateTime.TryParse(value.ToString(), out temp));
        }

        /// <summary>
        /// Determines if a string contains all valid permission group IDs.  The string that is sent must be in the form of a JSON list of strings.
        /// </summary>
        /// <param name="perms">A string that contains a JSON list of strings which are the IDs.</param>
        /// <returns></returns>
        public static bool IsValidPermissionGroupIDs(object value)
        {
            List<string> values = new List<string>();
            if (value.GetType() == typeof(string))
                values = value.ToString().Deserialize<List<string>>();
            else
                if (value.GetType() == typeof(List<string>))
                    values = value as List<string>;
                else
                    return false;

            return UnifiedServiceFramework.Authorization.Permissions.GetAllPermissionGroups().Select(x => x.ID).ContainsAll(values, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Determines if a string can be parsed into a boolean.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsValidBoolean(object value)
        {
            bool temp;
            return Boolean.TryParse(value.ToString(), out temp);
        }

        public static bool IsValidNameProperty(string value)
        {
            if (String.IsNullOrWhiteSpace(value) || value.Length > 50)
                return false;
            return true;
        }

        public static bool IsValidSSN(object value)
        {
            return (Regex.IsMatch(value.ToString(), @"^(?!219-09-9999|078-05-1120)(?!666|000|9\d{2})\d{3}-(?!00)\d{2}-(?!0{4})\d{4}$") ||
                Regex.IsMatch(value.ToString(), @"^(?!219099999|078051120)(?!666|000|9\d{2})\d{3}(?!00)\d{2}(?!0{4})\d{4}$"));
        }

        public static bool IsValidGender(object value)
        {
            return CDBLists.GetList("Genders").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidEthnicity(object value)
        {
            return CDBLists.GetList("Ethnicities").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidReligiousPreference(object value)
        {
            return CDBLists.GetList("ReligiousPreferences").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidSuffix(object value)
        {
            return CDBLists.GetList("Suffixes").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidRank(object value)
        {
            return CDBLists.GetList("Ranks").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidRate(object value)
        {
            return CDBLists.GetList("Rates").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidDutyStatus(object value)
        {
            return CDBLists.GetList("DutyStatuses").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidMusterState(object value)
        {
            return CDBLists.GetList("MusterStates").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidUIC(object value)
        {
            return CDBLists.GetList("UICs").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidPhoneType(object value)
        {
            return CDBLists.GetList("PhoneTypes").Values.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidDivision(object value)
        {
            return Commands.CommandsCache.ToList().Exists(x => x.Value.Departments.Exists(y => y.Divisions.Exists(z => z.Name.SafeEquals(value.ToString()))));
        }

        public static bool IsValidDepartment(object value)
        {
            return Commands.CommandsCache.ToList().Exists(x => x.Value.Departments.Exists(y => y.Name.SafeEquals(value.ToString())));
        }

        public static bool IsValidCommand(object value)
        {
            return Commands.CommandsCache.ToList().Exists(x => x.Value.Name.SafeEquals(value.ToString()));
        }

        public static bool AreValidNECs(object value)
        {
            List<string> values = new List<string>();
            if (value.GetType() == typeof(string))
                values = (value as string).Deserialize<List<string>>();
            else
                if (value.GetType() == typeof(List<string>))
                    values = value as List<string>;
                else
                    return false;

            return CDBLists.GetList("NECs").Values.ContainsAll(values, StringComparer.CurrentCultureIgnoreCase);
        }

        public static bool IsValidPhoneCarrier(object value)
        {
            return TextMessageHelper.PhoneCarrierMailDomainMappings.Keys.Contains(value.ToString(), StringComparer.CurrentCultureIgnoreCase);
        }



    }
}
