namespace CommandCentral
{
    /// <summary>
    /// Enumerates the possible duty statuses.
    /// </summary>
    public enum DutyStatuses
    {
        /// <summary>
        /// Indicates that a person is a member of the Active Duty component of the US armed forces.
        /// </summary>
        Active,
        /// <summary>
        /// Indicates that a person is a member of the Reserve component of the US armed forces.
        /// </summary>
        Reserves,
        /// <summary>
        /// Indicates that a person is a contractor.
        /// </summary>
        Contractor,
        /// <summary>
        /// Indicates that a person is a civilian.
        /// </summary>
        Civilian,
        /// <summary>
        /// Indicates a person has left the scope of the application either by leaving the Navy or moving to an unsupported command.
        /// </summary>
        Loss
    }
}
