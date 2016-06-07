namespace AtwoodUtils
{
    /// <summary>
    /// Describes a single variance between two values and the field name they came from.
    /// </summary>
    public class Variance
    {
        /// <summary>
        /// The name of the property in which the variance occurred.
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// The value from the first object - here, object "A"
        /// </summary>
        public object OldValue { get; set; }
        /// <summary>
        /// The value from the second object - here, object "B"
        /// </summary>
        public object NewValue { get; set; }

        /// <summary>
        /// Returns string.Format("The field '{0}' was changed from '{1}' to '{2}'.", valueA, valueB)
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("The field '{0}' was changed from '{1}' to '{2}'.", PropertyName, OldValue, NewValue);
        }

    }
}
