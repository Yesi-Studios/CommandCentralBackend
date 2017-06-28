using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtwoodUtils;

namespace CommandCentral.Entities.ReferenceLists
{
    public static class Paygrades
    {

        static Paygrades()
        {
            var paygrades = typeof(Paygrades).GetFields().Where(x => x.FieldType == typeof(Paygrade)).Select(x => (Paygrade)x.GetValue(null)).ToList();

            AllPaygrades = new ConcurrentBag<Paygrade>(paygrades);
        }

        public static ConcurrentBag<Paygrade> AllPaygrades;

        public static Paygrade E1 = new Paygrade { Id = Guid.Parse("3d3df167-3104-4081-9ba7-2de7a9d81a82"), Value = "E1", Description = "" };
        public static Paygrade E2 = new Paygrade { Id = Guid.Parse("ba31b52a-b686-41cd-8ec8-6a4071107e10"), Value = "E2", Description = "" };
        public static Paygrade E3 = new Paygrade { Id = Guid.Parse("cc2ea298-d879-45d3-a044-d3b62aecf50c"), Value = "E3", Description = "" };
        public static Paygrade E4 = new Paygrade { Id = Guid.Parse("49295129-c4d2-47a5-ac4f-001205a6621e"), Value = "E4", Description = "" };
        public static Paygrade E5 = new Paygrade { Id = Guid.Parse("3254953d-1e46-4e45-97cd-b8e0cb855a05"), Value = "E5", Description = "" };
        public static Paygrade E6 = new Paygrade { Id = Guid.Parse("55088fb5-0940-4aa6-81fb-afa925fb33e6"), Value = "E6", Description = "" };
        public static Paygrade E7 = new Paygrade { Id = Guid.Parse("d4ba97ca-6ebb-4809-a79b-3e4c625c2081"), Value = "E7", Description = "" };
        public static Paygrade E8 = new Paygrade { Id = Guid.Parse("a450639e-791e-4931-b331-08dc282a2a12"), Value = "E8", Description = "" };
        public static Paygrade E9 = new Paygrade { Id = Guid.Parse("a8bc40e4-3ae3-435f-9bde-080a7aff7985"), Value = "E9", Description = "" };
        public static Paygrade CWO2 = new Paygrade { Id = Guid.Parse("74e484ad-52d4-4051-af0e-cf4babb7b419"), Value = "CWO2", Description = "" };
        public static Paygrade CWO3 = new Paygrade { Id = Guid.Parse("70e0d857-34a3-4fdc-8a68-f0e077211a8d"), Value = "CWO3", Description = "" };
        public static Paygrade CWO4 = new Paygrade { Id = Guid.Parse("722665f9-f5ff-4bd7-862d-5099bc6844bf"), Value = "CWO4", Description = "" };
        public static Paygrade CWO5 = new Paygrade { Id = Guid.Parse("b3ddd459-d9b1-42c4-9c05-abdb888ed10e"), Value = "CWO5", Description = "" };
        public static Paygrade O1 = new Paygrade { Id = Guid.Parse("eac6717b-e92a-42db-9b7e-269e48f79ecd"), Value = "O1", Description = "" };
        public static Paygrade O1E = new Paygrade { Id = Guid.Parse("4f87b229-f0a9-411e-bcbe-695c58391f32"), Value = "O1E", Description = "" };
        public static Paygrade O2E = new Paygrade { Id = Guid.Parse("74cafaa5-75a7-4a33-ad61-ccad95727e65"), Value = "O2E", Description = "" };
        public static Paygrade O3E = new Paygrade { Id = Guid.Parse("e350c636-ed91-485b-b6ec-beb004f65d20"), Value = "O3E", Description = "" };
        public static Paygrade O2 = new Paygrade { Id = Guid.Parse("95a97565-0e8f-4565-b045-da7cac72bde8"), Value = "O2", Description = "" };
        public static Paygrade O3 = new Paygrade { Id = Guid.Parse("e169f89b-576e-43a2-85b8-1ee4a79697c3"), Value = "O3", Description = "" };
        public static Paygrade O4 = new Paygrade { Id = Guid.Parse("b7a8cdff-d435-4539-b68d-7afc08818950"), Value = "O4", Description = "" };
        public static Paygrade O5 = new Paygrade { Id = Guid.Parse("cb888211-68bf-4a05-b04a-ed4f70b1e64b"), Value = "O5", Description = "" };
        public static Paygrade O6 = new Paygrade { Id = Guid.Parse("531b75df-bfcc-4f6d-9131-b85d2a724c4e"), Value = "O6", Description = "" };
        public static Paygrade O7 = new Paygrade { Id = Guid.Parse("9cc80876-a9fd-46ef-a523-1493e20da0e9"), Value = "O7", Description = "" };
        public static Paygrade O8 = new Paygrade { Id = Guid.Parse("47386a36-3034-4426-8cc6-6d7c51c43c72"), Value = "O8", Description = "" };
        public static Paygrade O9 = new Paygrade { Id = Guid.Parse("1d9664e4-e670-4cce-843d-67f825b94f3c"), Value = "O9", Description = "" };
        public static Paygrade O10 = new Paygrade { Id = Guid.Parse("3d697112-a7f6-4a64-92e8-edfcb8dc6bd9"), Value = "O10", Description = "" };
        public static Paygrade GG1 = new Paygrade { Id = Guid.Parse("c7f0bf29-6e87-4de0-ab0b-25cafb34bb44"), Value = "GG1", Description = "" };
        public static Paygrade GG2 = new Paygrade { Id = Guid.Parse("6f59ba99-dcfe-4ad7-832a-e2948ab3dc8a"), Value = "GG2", Description = "" };
        public static Paygrade GG3 = new Paygrade { Id = Guid.Parse("5d1db4da-e9f3-4584-a482-04b1136def98"), Value = "GG3", Description = "" };
        public static Paygrade GG4 = new Paygrade { Id = Guid.Parse("c7498972-9412-4b70-bba4-f9b7dfcaba2d"), Value = "GG4", Description = "" };
        public static Paygrade GG5 = new Paygrade { Id = Guid.Parse("862c9d87-7d2e-49c1-a48e-f0819bf86a99"), Value = "GG5", Description = "" };
        public static Paygrade GG6 = new Paygrade { Id = Guid.Parse("ca3ef1dd-a07a-44da-8a19-317120082ea9"), Value = "GG6", Description = "" };
        public static Paygrade GG7 = new Paygrade { Id = Guid.Parse("505f281e-ae9e-4457-9d8e-556ff9de142f"), Value = "GG7", Description = "" };
        public static Paygrade GG8 = new Paygrade { Id = Guid.Parse("6e9ef1ce-46c0-4a9d-935f-641b57798104"), Value = "GG8", Description = "" };
        public static Paygrade GG9 = new Paygrade { Id = Guid.Parse("96d73299-4514-46d4-ab40-babf1ad6572c"), Value = "GG9", Description = "" };
        public static Paygrade GG10 = new Paygrade { Id = Guid.Parse("38b8db25-be02-40f9-b763-84eaad302d0d"), Value = "GG10", Description = "" };
        public static Paygrade GG11 = new Paygrade { Id = Guid.Parse("15a0b386-c002-420f-9c14-bf650381d50a"), Value = "GG11", Description = "" };
        public static Paygrade GG12 = new Paygrade { Id = Guid.Parse("21be1be3-7305-4d74-87df-9d5c5e4ecf94"), Value = "GG12", Description = "" };
        public static Paygrade GG13 = new Paygrade { Id = Guid.Parse("0240e7c7-2483-4d69-8a80-297669cc4af6"), Value = "GG13", Description = "" };
        public static Paygrade GG14 = new Paygrade { Id = Guid.Parse("69cc3474-5c26-4423-a3b1-3e8c1537d6f9"), Value = "GG14", Description = "" };
        public static Paygrade GG15 = new Paygrade { Id = Guid.Parse("9fe6a868-80e9-4a51-88d3-2271c77288cd"), Value = "GG15", Description = "" };
        public static Paygrade CON = new Paygrade { Id = Guid.Parse("e34d48a3-a08d-47e7-8ccf-995ab8996156"), Value = "CON", Description = "" };

        /// <summary>
        /// Ensures that all paygrades are persisted in the database and that they look the same as they do here.
        /// </summary>
        /// <param name="options"></param>
        [ServiceManagement.StartMethod(Priority = 9)]
        private static void EnsurePaygradesPersistence(CLI.Options.LaunchOptions options)
        {
            Logging.Log.Info("Checking paygrades...");

            using (var session = DataAccess.NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                var currentPaygrades = session.QueryOver<Paygrade>().List();

                var missingPaygrades = AllPaygrades.Except(currentPaygrades).ToList();

                Logging.Log.Info("Persisting {0} missing paygrade(s)...".FormatS(missingPaygrades.Count));
                foreach (var paygrade in missingPaygrades)
                {
                    session.Save(paygrade);
                }

                transaction.Commit();
            }
        }


    }
}
