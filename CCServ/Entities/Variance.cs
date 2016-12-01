using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CCServ.Entities;
using System.Collections;
using AtwoodUtils;

namespace CCServ
{
    /// <summary>
    /// Describes a single variance in an object.
    /// </summary>
    public class Variance
    {
        /// <summary>
        /// The name of the property that is in variance.
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// The value of the property from the old object.
        /// </summary>
        public object OldValue { get; set; }
        /// <summary>
        /// The value of the property from the new object.
        /// </summary>
        public object NewValue { get; set; }
    }

    /// <summary>
    /// The class that contains the comparison method.
    /// </summary>
    public static class VarianceExtensions
    {
        /// <summary>
        /// Compares two objects and returns a list of variances.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="oldObject"></param>
        /// <param name="newObject"></param>
        /// <returns></returns>
        public static IEnumerable<Variance> DetailedCompare<T>(this T oldObject, T newObject)
        {
            PropertyInfo[] pi = oldObject.GetType().GetProperties();
            foreach (PropertyInfo p in pi)
            {
                Variance v = new Variance();
                v.PropertyName = p.Name;
                v.OldValue = p.GetValue(oldObject);
                v.NewValue = p.GetValue(newObject);
                if (!Equals(v.OldValue, v.NewValue))
                    yield return v;
            }
        }

        /// <summary>
        /// Calculates the discrete changes that occurred, given a lost of variances.
        /// </summary>
        /// <typeparam name="T">The parent type of all the variances.  Eg. Person</typeparam>
        /// <param name="variances"></param>
        /// <returns></returns>
        public static IEnumerable<Change> CalculateChanges<T>(IEnumerable<Variance> variances)
        {
            foreach (var variance in variances)
            {
                //First we need to know what property we're talking about.
                var propertyInfo = AtwoodUtils.PropertySelector.SelectPropertyFrom<T>(variance.PropertyName);

                //Now we need to know if we're dealing with a collection or not.
                if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    //Ok we have a collection!  Now we need to know what was added, changed, and deleted.
                    var elementType = Utilities.GetBaseTypeOfEnumerable(variance.NewValue as IEnumerable);

                    //Ok, now we have the type of the element.  Let's see if it has a property called "Id".  That will help.
                    var idProperty = elementType.GetProperties().FirstOrDefault(x => x.Name.SafeEquals("id"));

                    //In this case, the elements have no Id property, so we compare hash codes only.
                    if (idProperty == null)
                    {
                        //During this, here's what we're going to do:
                        //Walk through both lists, look for the differences, and then count them up.

                        var newList = variance.NewValue as IEnumerable;
                        var oldList = variance.OldValue as IEnumerable;

                        List<object> removedItems = new List<object>();
                        List<object> addedItems = new List<object>();

                        foreach (var newItem in newList)
                        {
                            var exists = false;
                            foreach (var oldItem in oldList)
                            {
                                if (oldItem.Equals(newItem))
                                {
                                    exists = true;
                                    break;
                                }
                            }

                            if (!exists)
                            {
                                addedItems.Add(newItem);
                            }
                        }

                        foreach (var oldItem in oldList)
                        {
                            var exists = false;
                            foreach (var newItem in newList)
                            {
                                if (newItem.Equals(oldItem))
                                {
                                    exists = true;
                                    break;
                                }

                                if (!exists)
                                {
                                    removedItems.Add(oldItem);
                                }
                            }
                        }
                    }
                    else //In this case, the elements have an Id property, so we'll compare Ids and hashcodes to determine changes.
                    {
                        //During this we'll do something similar to the thing above, but we'll also look for elements to change based on similar Ids

                        var newList = variance.NewValue as IEnumerable;
                        var oldList = variance.OldValue as IEnumerable;

                        List<object> removedItems = new List<object>();
                        List<object> addedItems = new List<object>();
                        List<object> changedItems = new List<object>();

                        //First, let's get the changes out of the way...
                        foreach (var newItem in newList)
                        {
                            foreach (var oldItem in oldList)
                            {
                                //If we find two items whose Id properties match, but the objects don't, then we just found a change.
                                if (idProperty.GetValue(oldItem).Equals(idProperty.GetValue(newItem)) && !newItem.Equals(oldItem))
                                {

                                }
                            }
                        }

                    }
                }
                else
                {
                }
            }

        }
    }
    
    
}
