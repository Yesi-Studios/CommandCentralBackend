using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;


namespace CommandCentralHost.Editors
{
    /// <summary>
    /// Provides methods for an interface to manage the muster.  Things like Forcing the muster to rollover and finalize and generate its report.
    /// </summary>
    public static class MusterManager
    {
        public static void MusterManagerEntryPoint()
        {
            bool keepLooping = true;

            while (keepLooping)
            {
                Console.Clear();

                "Welcome to the muster manager!".WriteLine();
                "".WriteLine();

                "1. Finalize Muster".WriteLine();
                "2. Create Muster Report".WriteLine();
                "3. Rollover Muster".WriteLine();
                "4. Initialize Muster".WriteLine();
                "5. Return".WriteLine();

                int option;
                if (int.TryParse(Console.ReadLine(), out option) && option >= 1 && option <= 5)
                {
                    switch (option)
                    {
                        case 1:
                            {
                                break;
                            }
                        case 2:
                            {
                                break;
                            }
                        case 3:
                            {
                                break;
                            }
                        case 4:
                            {
                                break;
                            }
                        case 5:
                            {
                                keepLooping = false;
                                break;
                            }
                        default:
                            {
                                throw new NotImplementedException("The default case was hit in the option switch of the muster manager entry point.  Case: {0}".FormatS(option));
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Interface for finalizing the muster.
        /// </summary>
        private static void FinalizeMuster()
        {
            Console.Clear();

            //Before we even start let's make sure the muster hasn't already been finalized.
            if (CommandCentral.Entities.MusterRecord.IsMusterFinalized)
            {
                "You can't finalize the muster because it already has been.  A rollover must occur now.".WriteLine();
                return;
            }
            

            "You are about to finalize the muster; are you sure you want to do that?  More checks will follow. (y)".WriteLine();

            if (Console.ReadLine().ToLower() == "y")
            {
                var persons = CommandCentral.Entities.MusterRecord.GetMusterablePersons();

                //If there are any unmustered people let's ask the client if they're sure about finalizing the muster.
                int unmustered = persons.Where(x => !x.CurrentMusterStatus.HasBeenSubmitted).Count();

                bool continueWithFinalization = true;

                if (unmustered != 0)
                {
                    "{0} persons have yet to be mustered.  Are you sure you want to continue with finalization? (y)".FormatS(unmustered).WriteLine();

                    if (Console.ReadLine().ToLower() != "y")
                    {
                        continueWithFinalization = false;
                    }
                }

                //So if the unmustered number is 0 or whatever, we only need to check the flag.
                if (continueWithFinalization)
                {
                    //Ok we should be ready for finalization now.  Fire off the method.  The null argument tells it that the system is initiating the finalization.
                    CommandCentral.Entities.MusterRecord.FinalizeMuster(null);

                    "Muster has been finalized!  A rollover must now occur before muster can be submitted for persons again.".WriteLine();
                }
                else
                {
                    "Canceled...".WriteLine();
                }
            }
            else
            {
                "Canceled...".WriteLine();
            }
        }

        /// <summary>
        /// Interface for create a new muster report.
        /// </summary>
        private static void CreateMusterReport()
        {
            Console.Clear();

            "Generating muster report...".WriteLine();
            var musterReport = CommandCentral.Entities.MusterReport.GenerateCurrentMusterReport();
            "Done".WriteLine();


            "The report will now be copied as JSON to your clipboard.".WriteLine();
            System.Windows.Forms.Clipboard.SetText(musterReport.Serialize());
            "It's on your clipboard.".WriteLine();
        }
    }
}
