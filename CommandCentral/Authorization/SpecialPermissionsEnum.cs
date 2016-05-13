using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Authorization
{
    /// <summary>
    /// Defines all special permissions which control access to parts of the API.
    /// </summary>
    public enum SpecialPermissions
    {
        Developer,
        ManageNews,
        SearchPersons,
        EditPerson,
        CreatePerson,
        Triad,
        SubmitMuster
    }

    /// <summary>
    /// Contains extension methods for helping with special permissions.
    /// </summary>
    public static class SpecialPermissonExtensions
    {
        /// <summary>
        /// Casts an IEnumerable collection of strings into a collection of special permissions.
        /// <para/>
        /// Throws a format exception if a given string can not be cast into a special permission.
        /// </summary>
        /// <param name="specialPermissions"></param>
        /// <returns></returns>
        public static IEnumerable<SpecialPermissions> ToEnumList(this IEnumerable<string> specialPermissions)
        {
            foreach (var str in specialPermissions)
            {
                SpecialPermissions specialPerm;
                if (!Enum.TryParse(str, out specialPerm))
                    throw new FormatException("The string, '{0}', could not be parsed into a special permission.".FormatS(str));

                yield return specialPerm;
            }
        }
    }
}
