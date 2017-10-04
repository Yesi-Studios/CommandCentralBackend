using CommandCentral.Test.ReferenceListTests;
using NUnit.Framework;

namespace CommandCentral.Test
{
    [TestFixture]
    public class MainTests
    {
        [TestCase]
        public void MainTestOnDisk()
        {
            EmailTests.SetupEmail();
            LoggingTests.InitializeLogger();
            SchedulerTests.InitializeFluentScheduler();
            PermissionsTests.EnsureNoDuplicatePermissions();
            DatabaseTests.SetupRealDatabase();
            DatabaseTests.UpdateForeignKeyRuleForWatchAssignment();
            PreDefTests.AddPreDefs();
            APIKeyTests.EnsureDefaultAPIKeyExistsAndAddIfItDoesnt();
            UICTests.CreateUICs();
            CommandDepartmentDivisionTests.CreateCommands();
            CommandDepartmentDivisionTests.CreateDepartments();
            CommandDepartmentDivisionTests.CreateDivisions();
            PersonTests.CreateDeveloper();
            PersonTests.CreateUsers();
        }
    }
}