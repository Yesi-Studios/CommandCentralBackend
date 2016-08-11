namespace CCServ
{
    /// <summary>
    /// Enumerates levels of importance for change events.
    /// </summary>
    public enum ChangeEventLevels
    {
        /// <summary>
        /// A high change event level should be broadcast to the widest audience possible.
        /// </summary>
        High,
        /// <summary>
        /// A medium change event level should be broadcast to the smallest audience possible.
        /// </summary>
        Medium,
        /// <summary>
        /// A low change event level should be broadcast to only those persons who want it.
        /// </summary>
        Low
    }
}
