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
                //If both the new and old value is null, then that's not good.
                if (variance.NewValue == null && variance.OldValue == null)
                    throw new ArgumentException("Both values in the variance equalled null.");

                //First we need to know what property we're talking about.
                var propertyInfo = AtwoodUtils.PropertySelector.SelectPropertyFrom<T>(variance.PropertyName);

                //Now we need to know if we're dealing with a collection or not.
                if (typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    //We're going to make the extremely unsafe assumption that collections are not null.

                    //Ok we have a collection!  Now we need to know what was added, changed, and deleted.
                    var elementType = Utilities.GetBaseTypeOfEnumerable(variance.NewValue as IEnumerable);

                    //Ok, now we have the type of the element.  Let's see if it has a property called "Id".  That will help.
                    var idProperty = elementType.GetProperties().FirstOrDefault(x => x.Name.SafeEquals("id"));

                    var newList = variance.NewValue as IEnumerable;
                    var oldList = variance.OldValue as IEnumerable;

                    List<object> removedItems = new List<object>();
                    List<object> addedItems = new List<object>();
                    //The key is the new value, the value is the old value.
                    List<KeyValuePair<object, object>> changedItems = new List<KeyValuePair<object, object>>();

                    //In this case, the elements have no Id property, so we compare hash codes only.
                    if (idProperty == null)
                    {
                        //During this, here's what we're going to do:
                        //Walk through both lists, look for the differences, and then count them up.

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


                        //First, let's get the changes out of the way...
                        foreach (var newItem in newList)
                        {

                            //If we never find two items whose Id properties match, then we found an object that was added.
                            bool exists = false;

                            foreach (var oldItem in oldList)
                            {
                                //If we find two items whose Id properties match, but the objects don't, then we just found a change.
                                if (idProperty.GetValue(oldItem).Equals(idProperty.GetValue(newItem)) && !newItem.Equals(oldItem))
                                {
                                    changedItems.Add(new KeyValuePair<object, object>(newItem, oldItem));
                                    exists = true;
                                    break;
                                }
                                else
                                    if (newItem.Equals(oldItem)) //If we find two items that are exactly equal, then the item was unchanged.
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

                        //Now we iterate backwards in order to look for removed items.
                        foreach (var oldItem in oldList)
                        {
                            bool exists = false;

                            foreach (var newItem in newList)
                            {
                                if (oldItem.Equals(newItem))
                                {
                                    exists = true;
                                    break;
                                }
                            }

                            if (!exists)
                            {
                                removedItems.Add(oldItem);
                            }
                        }

                    }

                    //Ok, at this point our three lists (removed, added, changed) have been populated.
                    //So we just need to generate the changes list and then return it.

                    //First, here's all of the added items.
                    foreach (var addedItem in addedItems)
                    {
                        yield return new Change
                        {
                            NewValue = addedItem.ToString(),
                            PropertyName = variance.PropertyName,
                            Remarks = "The item was added.",
                            Id = Guid.NewGuid()
                        };
                    }

                    //Now the removed items.
                    foreach (var removedItem in removedItems)
                    {
                        yield return new Change
                        {
                            OldValue = removedItem.ToString(),
                            PropertyName = variance.PropertyName,
                            Remarks = "The item was removed.",
                            Id = Guid.NewGuid()
                        };
                    }

                    //Finally, the changed items.
                    foreach (var changedItem in changedItems)
                    {
                        yield return new Change
                        {
                            NewValue = changedItem.Key.ToString(),
                            OldValue = changedItem.Value.ToString(),
                            PropertyName = variance.PropertyName,
                            Id = Guid.NewGuid()
                        };
                    }

                }
                else //This case is if we have an object that DOES NOT implement IEnumerable.  Meaning that it's a single object... hopefully.
                {
                    //First, if one or the other is null, then we have an addition or a deletion.
                    if (variance.NewValue == null)
                    {
                        yield return new Change
                        {
                            OldValue = variance.OldValue.ToString(),
                            PropertyName = variance.PropertyName,
                            Id = Guid.NewGuid()
                        };
                    }
                    else
                        if (variance.OldValue == null)
                        {
                            yield return new Change
                            {
                                NewValue = variance.NewValue.ToString(),
                                PropertyName = variance.PropertyName,
                                Id = Guid.NewGuid()
                            };
                        }
                        else //In this case, neither value is null.
                        {
                            //We need to know if our object has an Id property.
                            var idProperty = variance.NewValue.GetType().GetProperties().FirstOrDefault(x => x.Name.SafeEquals("id"));

                            //Yes! We have an Id property.
                            if (idProperty != null)
                            {
                                //If the Ids are the same but the objects are not, we have a change.
                                if (idProperty.GetValue(variance.OldValue).Equals(idProperty.GetValue(variance.NewValue))
                                    && !variance.NewValue.Equals(variance.OldValue))
                                {
                                    yield return new Change
                                    {
                                        NewValue = variance.NewValue.ToString(),
                                        OldValue = variance.OldValue.ToString(),
                                        PropertyName = variance.PropertyName,
                                        Id = Guid.NewGuid()
                                    };
                                }
                            }
                            else
                            {
                                //Since we have no Id property, all we can really do is compare the two values.
                                if (!variance.NewValue.Equals(variance.OldValue))
                                {
                                    yield return new Change
                                    {
                                        NewValue = variance.NewValue.ToString(),
                                        OldValue = variance.OldValue.ToString(),
                                        PropertyName = variance.PropertyName,
                                        Id = Guid.NewGuid()
                                    };
                                }
                            }
                        }
                }
            }

        }
    }
    
    
}
