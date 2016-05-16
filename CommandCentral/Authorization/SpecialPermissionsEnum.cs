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
        /// <summary>
        /// Indicates a developer.  This permission should act as a key to all parts of the application.
        /// </summary>
        Developer,
        /// <summary>
        /// Indicates that a person is allowed to update/delete/create news items.
        /// </summary>
        ManageNews,
        /// <summary>
        /// Indicates that a person is allowed to use the search functionality.  This does not affect the single person loading endpoints.
        /// </summary>
        SearchPersons,
        /// <summary>
        /// Indicates that a person has the authority to edit at least one field on another person's profile.  This permission should not prevent a user from editing his/her own profile.
        /// </summary>
        EditPerson,
        /// <summary>
        /// Indicates that a person is allowed to create new persons.
        /// </summary>
        CreatePerson,
        /// <summary>
        /// Indicates that a person is a member of the triad.  This should give access to almost all parts of the application and allow the person to bypass the IsInChainOfCommand check as the Triad is in their own chain of command.
        /// </summary>
        Triad,
        /// <summary>
        /// Indicates that a person is allowed to submit the muster for other persons.
        /// </summary>
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
