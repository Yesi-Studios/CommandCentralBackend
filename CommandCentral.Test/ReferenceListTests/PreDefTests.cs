using CommandCentral.Entities.ReferenceLists;
using CommandCentral.Entities.ReferenceLists.Watchbill;
using CommandCentral.PreDefs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace CommandCentral.Test.ReferenceListTests
{
    public static class PreDefTests
    {

        public static void AddPreDefs()
        {
            PreDefUtility.PersistPreDef<WatchQualification>();
            PreDefUtility.PersistPreDef<Sex>();
            PreDefUtility.PersistPreDef<WatchbillStatus>();
            PreDefUtility.PersistPreDef<WatchShiftType>();
            PreDefUtility.PersistPreDef<WatchEligibilityGroup>();
            PreDefUtility.PersistPreDef<PhoneNumberType>();
            PreDefUtility.PersistPreDef<Paygrade>();
            PreDefUtility.PersistPreDef<MusterStatus>();
            PreDefUtility.PersistPreDef<DutyStatus>();
            PreDefUtility.PersistPreDef<AccountHistoryType>();

            Logging.Log.Info("testing WatchQualification");
            Assert.IsTrue(ReferenceListHelper<WatchQualification>.All().Count ==
                PreDefOf<WatchQualification>.Get().Definitions.Count);

            Logging.Log.Info("testing Sex");
            Assert.IsTrue(ReferenceListHelper<Sex>.All().Count ==
                PreDefOf<Sex>.Get().Definitions.Count);

            Logging.Log.Info("testing WatchbillStatus");
            Assert.IsTrue(ReferenceListHelper<WatchbillStatus>.All().Count ==
                PreDefOf<WatchbillStatus>.Get().Definitions.Count);

            Logging.Log.Info("testing WatchShiftType");
            Assert.IsTrue(ReferenceListHelper<WatchShiftType>.All().Count ==
                PreDefOf<WatchShiftType>.Get().Definitions.Count);

            Logging.Log.Info("testing WatchEligibilityGroup");
            Assert.IsTrue(ReferenceListHelper<WatchEligibilityGroup>.All().Count ==
                PreDefOf<WatchEligibilityGroup>.Get().Definitions.Count);
            
            Logging.Log.Info("testing PhoneNumberType");
            Assert.IsTrue(ReferenceListHelper<PhoneNumberType>.All().Count ==
                PreDefOf<PhoneNumberType>.Get().Definitions.Count);

            Logging.Log.Info("testing Paygrade");
            Assert.IsTrue(ReferenceListHelper<Paygrade>.All().Count ==
                PreDefOf<Paygrade>.Get().Definitions.Count);

            Logging.Log.Info("testing MusterStatus");
            Assert.IsTrue(ReferenceListHelper<MusterStatus>.All().Count ==
                PreDefOf<MusterStatus>.Get().Definitions.Count);

            Logging.Log.Info("testing DutyStatus");
            Assert.IsTrue(ReferenceListHelper<DutyStatus>.All().Count ==
                PreDefOf<DutyStatus>.Get().Definitions.Count);

            Logging.Log.Info("testing AccountHistoryType");
            Assert.IsTrue(ReferenceListHelper<AccountHistoryType>.All().Count ==
                PreDefOf<AccountHistoryType>.Get().Definitions.Count);
        }
    }
}
