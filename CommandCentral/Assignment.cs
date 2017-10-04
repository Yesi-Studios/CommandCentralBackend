﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandCentral.Entities.ReferenceLists;
using AtwoodUtils;

namespace CommandCentral
{
    /// <summary>
    /// A wrapper around a division/department/command.
    /// </summary>
    public class Assignment
    {

        #region Properties

        /// <summary>
        /// The division.
        /// </summary>
        public Division Division { get; private set; }

        /// <summary>
        /// The department.
        /// </summary>
        public Department Department { get; private set; }

        /// <summary>
        /// The command.
        /// </summary>
        public Command Command { get; private set; }

        #endregion

        #region ctors

        /// <summary>
        /// Builds a new assignment from arbitrary div/dep/command.
        /// </summary>
        /// <param name="div"></param>
        /// <param name="dep"></param>
        /// <param name="com"></param>
        public Assignment(Division div, Department dep, Command com)
        {
            Division = div;
            Department = dep;
            Command = com;
        }

        /// <summary>
        /// Builds an assignment from the div and walks up the line to build the rest.
        /// </summary>
        /// <param name="div"></param>
        public Assignment(Division div)
        {
            Division = div;
            Department = div?.Department;
            Command = Department?.Command;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns Div - Dept - Command
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "{0} - {1} - {2}".With(Division, Department, Command);
        }

        #endregion

    }
}
