using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;
using AtwoodUtils;
using CCServ.Logging;
using System.Globalization;

namespace CCServ.DataAccess
{
    /// <summary>
    /// Contains the ingest old database script.
    /// </summary>
    public static class Importer
    {
        /// <summary>
        /// Ingests the old database into the new database.  NHibernate must be configured prior to calling this method.
        /// </summary>
        public static void IngestOldDatabase()
        {

            DataSet oldDatabase = new DataSet();

            using (var session = NHibernateHelper.CreateStatefulSession())
            using (var transaction = session.BeginTransaction())
            {
                try
                {
                    var connectionString = String.Format("server={0};uid={1};pwd={2};database={3}", "gord14ec204", "dkatwoo", "dkatwoo987", "sullitest");

                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        List<string> tableNames = new List<string>();

                        //First we need all of the tables.
                        using (var command = new MySqlCommand("SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = 'sullitest'", connection))
                        {
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    tableNames.Add(reader["TABLE_NAME"] as string);
                                }
                            }
                        }

                        //Throw out the sql logs.  They're too big to hold in memory.
                        foreach (var tableName in tableNames.Where(x => x != "sqllog" && !x.Contains("muster")))
                        {
                            Log.Info("Loading table: {0}".FormatS(tableName));
                            using (var dataAdapter = new MySqlDataAdapter("SELECT * FROM `{0}`".FormatS(tableName), connection))
                            {
                                dataAdapter.FillSchema(oldDatabase, SchemaType.Source, tableName);
                                dataAdapter.Fill(oldDatabase, tableName);
                            }
                        }

                        //Add guid based ids to each row in each table.
                        foreach (var table in oldDatabase.Tables.Cast<DataTable>())
                        {
                            Log.Info("Adding Ids to table: {0}".FormatS(table.TableName));
                            table.Columns.Add("NewId", typeof(string));
                            table.Rows.Cast<DataRow>().ToList().ForEach(x =>
                                {
                                    x["NewId"] = Guid.NewGuid().ToString();
                                });
                        }


                        //Ok, commands departments and divisions first.
                        Log.Info("Populating commands, departments, and divisions...");

                        var niocgaRow = oldDatabase.Tables["commands"].Rows.Cast<DataRow>().First(y => Convert.ToInt32(y["CMD_id"]) == 4);

                        List<Entities.ReferenceLists.Command> commands = new List<Entities.ReferenceLists.Command>
                        {
                            new Entities.ReferenceLists.Command
                            {
                                Departments = new List<Entities.ReferenceLists.Department>(),
                                Description = niocgaRow["CMD_name"] as string,
                                Id = Guid.Parse(niocgaRow["NewId"] as string),
                                Value = niocgaRow["CMD_pla"] as string
                            }
                        };

                        commands.First().Departments = new List<Entities.ReferenceLists.Department>(
                            oldDatabase.Tables["dept"].Rows.Cast<DataRow>().Where(y => Convert.ToBoolean(y["DEPT_actv"])).Select(y =>
                                new Entities.ReferenceLists.Department
                                {
                                    Command = commands.First(),
                                    Description = "",
                                    Id = Guid.Parse(y["NewId"] as string),
                                    Value = y["DEPT_dept"] as string
                                })
                        );

                        foreach (var department in commands.First().Departments)
                        {
                            department.Divisions = oldDatabase.Tables["div"].Rows.Cast<DataRow>()
                                .Where(x => Convert.ToBoolean(x["DIV_actv"]) &&
                                    Convert.ToInt32(x["DIV_dept"]) == Convert.ToInt32(oldDatabase.Tables["dept"].AsEnumerable().First(y => y["NewId"] as string == department.Id.ToString())["DEPT_id"]))
                                .Select(x =>
                                    new Entities.ReferenceLists.Division
                                    {
                                        Department = department,
                                        Description = "",
                                        Id = Guid.Parse(x["NewId"] as string),
                                        Value = x["DIV_div"] as string
                                    }).ToList();
                        }

                        //And now that we have all of these, let's go ahead and save them.
                        Log.Info("Persisting commands...");
                        commands.ForEach(x => session.Save(x));
                        session.Flush();

                        //Now let's do NECs
                        Log.Info("Populating NECs...");

                        //Some NECs don't have a type :(  Let's add an option for them.
                        oldDatabase.Tables["nec"].AsEnumerable().ToList().ForEach(x =>
                            {
                                if (String.IsNullOrEmpty(x["NEC_type"] as string))
                                {
                                    x["NEC_type"] = 0;
                                }
                            });

                        oldDatabase.Tables["types"].Rows.Add(new object[] { 0, "None" });

                        List<Entities.ReferenceLists.NEC> necs = oldDatabase.Tables["nec"].AsEnumerable().Select(x =>
                            new Entities.ReferenceLists.NEC
                            {
                                Description = "",
                                Id = Guid.Parse(x["NewId"] as string),
                                NECType = (PersonTypes)Enum.Parse((typeof(PersonTypes)), oldDatabase.Tables["types"].AsEnumerable().First(y => Convert.ToInt32(y["TYPE_id"]) == Convert.ToInt32(x["NEC_type"]))["TYPE_type"] as string),
                                Value = x["NEC_nec"] as string
                            }).ToList();

                        Log.Info("Persisting NECs...");
                        necs.ForEach(x => session.Save(x));
                        session.Flush();

                        //Now the designations
                        Log.Info("Populating designations...");

                        List<Entities.ReferenceLists.Designation> designations = oldDatabase.Tables["rate"].AsEnumerable().Select(x =>
                            new Entities.ReferenceLists.Designation
                            {
                                Description = "",
                                Id = Guid.Parse(x["NewId"] as string),
                                Value = x["RATE_rate"] as string
                            }).ToList();
                        
                        Log.Info("Persisting designations...");
                        
                        designations.ForEach(x => session.Save(x));
                        session.Flush();

                        //Now UICs
                        Log.Info("Populating UICs...");

                        List<Entities.ReferenceLists.UIC> uics = oldDatabase.Tables["uic"].AsEnumerable().Select(x =>
                            new Entities.ReferenceLists.UIC
                            {
                                Description = "",
                                Value = x["UIC_uic"] as string,
                                Id = Guid.Parse(x["NewId"] as string)
                            }).ToList();

                        Log.Info("Persisting UICs...");

                        uics.ForEach(x => session.Save(x));
                        session.Flush();

                        //Now the ethnicities.  Yes, they were really being stored as a file.
                        Log.Info("Populating ethnicities...");
                        string rawEthnicities = @"
[{'Id':'0', 'Value':'African American', 'NewId':'CB5795E4-5AA8-43DD-B6B9-6BEB051424C6'},
{'Id':'1', 'Value':'Native American', 'NewId':'D5200010-6A3B-4A67-B6C5-68613ACAA307'},
{'Id':'2', 'Value':'Hispanic', 'NewId':'41388161-5D5B-4AE1-A3A2-7E08CCAA62B4'},
{'Id':'3', 'Value':'Caucasian', 'NewId':'A00E006A-E030-407F-BC1C-4F223377A5DD'},
{'Id':'4', 'Value':'Other/Unknown', 'NewId':'1A3800A5-4195-4C76-B115-9294978AF62D'},
{'Id':'5', 'Value':'Asian/Pacific Islander', 'NewId':'7C8F6A04-B4F9-4F02-9C13-8360AACB76DD'},
{'Id':'6', 'Value':'<None>', 'NewId':'84C5BB43-F52A-4AE2-9F82-5C324D6ADD80'}]";

                        var parsedEthnicities = Newtonsoft.Json.JsonConvert.DeserializeObject(rawEthnicities);

                        List<Entities.ReferenceLists.Ethnicity> ethnicities = ((Newtonsoft.Json.Linq.JArray)parsedEthnicities).Select(x =>
                            new Entities.ReferenceLists.Ethnicity
                            {
                                Description = "",
                                Value = x.Value<string>("Value"),
                                Id = Guid.Parse(x.Value<string>("NewId"))
                            }
                        ).ToList();

                        Log.Info("Persisting ethnicities...");
                        ethnicities.ForEach(x => session.Save(x));
                        session.Flush();

                        //Now the rel preferences.  THese really are in a file too.
                        Log.Info("Populating religious preferences...");

                        string rawRelPreferences = @"[
	{
		'Id':'0',
		'Value':'Christian Non-Denominational',
		'NewId' : ''
	},
	{
		'Id':'1',
		'Value':'Catholic',
		'NewId' : ''
	},
	{
		'Id':'2',
		'Value':'<None>',
		'NewId' : ''
	},
	{
		'Id':'3',
		'Value':'Mormon',
		'NewId' : ''
	},
	{
		'Id':'4',
		'Value':'Muslim',
		'NewId' : ''
	},
	{
		'Id':'5',
		'Value':'Baptist',
		'NewId' : ''
	},
	{
		'Id':'6',
		'Value':'Protestant',
		'NewId' : ''
	},
	{
		'Id':'7',
		'Value':'LDS',
		'NewId' : ''
	},
	{
		'Id':'8',
		'Value':'Methodist',
		'NewId' : ''
	},
	{
		'Id':'9',
		'Value':'Lutheran',
		'NewId' : ''
	},
	{
		'Id':'10',
		'Value':'Episcopal',
		'NewId' : ''
	},
	{
		'Id':'11',
		'Value':'Wiccan',
		'NewId' : ''
	},
	{
		'Id':'12',
		'Value':'Roman Catholic',
		'NewId' : ''
	},
	{
		'Id':'13',
		'Value':'Jewish',
		'NewId' : ''
	},
	{
		'Id':'14',
		'Value':'Orthodox Christian',
		'NewId' : ''
	},
	{
		'Id':'15',
		'Value':'Atheist',
		'NewId' : ''
	},
	{
		'Id':'16',
		'Value':'Greek Orthodox',
		'NewId' : ''
	},
	{
		'Id':'17',
		'Value':'Seventh Day Adventist',
		'NewId' : ''
	},
	{
		'Id':'18',
		'Value':'Hindu',
		'NewId' : ''
	},
	{
		'Id':'19',
		'Value':'Presbyterian',
		'NewId' : ''
	},
	{
		'Id':'20',
		'Value':'Buddist',
		'NewId' : ''
	},
	{
		'Id':'21',
		'Value':'Christian',
		'NewId' : ''
	},
	{
		'Id':'22',
		'Value':'Southern Baptist',
		'NewId' : ''
	},
	{
		'Id':'23',
		'Value':'Agnostic',
		'NewId' : ''
	},
	{
		'Id':'24',
		'Value':'Pentecostal',
		'NewId' : ''
	},
	{
		'Id':'25',
		'Value':'Pagan',
		'NewId' : ''
	},
	{
		'Id':'26',
		'Value':'Messianic Jew',
		'NewId' : ''
	},
	{
		'Id':'27',
		'Value':'Calvinist',
		'NewId' : ''
	},
	{
		'Id':'28',
		'Value':'Unitarian',
		'NewId' : ''
	},
	{
		'Id':'29',
		'Value':'Jedi',
		'NewId' : ''
	},
	{
		'Id':'29',
		'Value':'Sith',
		'NewId' : ''
	}
]";

                        var parsedRelPreferences = Newtonsoft.Json.JsonConvert.DeserializeObject(rawRelPreferences);


                        List<Entities.ReferenceLists.ReligiousPreference> religiousPreferences = ((Newtonsoft.Json.Linq.JArray)parsedRelPreferences).Select(x =>
                            {
                                x["NewId"] = Guid.NewGuid().ToString();

                                return new Entities.ReferenceLists.ReligiousPreference
                                {
                                    Description = "",
                                    Value = x.Value<string>("Value"),
                                    Id = Guid.Parse(x.Value<string>("NewId"))
                                };
                            }
                        ).ToList();

                        Log.Info("Persisting religious preferences...");
                        religiousPreferences.ForEach(x => session.Save(x));
                        session.Flush();


                        //Start the person load.  First we need only those persons that are active at the command.
                        Log.Info("Loading all members of the command...");
                        string idsResourcePath = "CCServ.DataAccess.ids.json";
                        List<int> ids = new List<int>();
                        using (var stream = System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream(idsResourcePath))
                        {
                            if (stream == null)
                            {
                                Log.Critical("The Ids file was not loaded: {0}.".FormatS(idsResourcePath));
                                return;

                            }
                            else
                            {
                                using (var reader = new System.IO.StreamReader(stream))
                                {
                                    var obj = Newtonsoft.Json.JsonConvert.DeserializeObject(reader.ReadToEnd());
                                    ids = ((Newtonsoft.Json.Linq.JArray)obj).Select(x => x.Value<int>("PERS_id")).ToList();
                                }
                            }
                        }

                        var memberRows = oldDatabase.Tables["person"].AsEnumerable().Where(x => ids.Contains(Convert.ToInt32(x["PERS_id"]))).ToList();



                        List<Entities.Person> persons = memberRows.Select(persRow =>
                            {
                                var person = new Entities.Person();

                                var adminRow = oldDatabase.Tables["admin_info"].AsEnumerable().First(x => Convert.ToInt32(x["PERS_id"]) == Convert.ToInt32(persRow["PERS_id"]));
                                var workRow = oldDatabase.Tables["work_info"].AsEnumerable().First(x => Convert.ToInt32(x["PERS_id"]) == Convert.ToInt32(persRow["PERS_id"]));


                                person.AccountHistory = new List<Entities.AccountHistoryEvent>();
                                person.Changes = new List<Entities.Change>();
                                person.Command = commands.First();
                                person.ContactRemarks = "";
                                person.CurrentMusterStatus = Entities.Muster.MusterRecord.CreateDefaultMusterRecordForPerson(person, DateTime.Now);

                                var doa = adminRow["ADM_cmddoa"] as string;
                                if (string.IsNullOrEmpty(doa))
                                {
                                    person.DateOfArrival = DateTime.Now;
                                    //TODO LOG ME
                                }
                                else
                                {
                                    DateTime temp;
                                    if (DateTime.TryParseExact(doa, "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out temp))
                                        person.DateOfArrival = temp;
                                    else
                                    {
                                        person.DateOfArrival = DateTime.Now;
                                        //TODO LOG ME
                                    }
                                }

                                var dob = persRow["PERS_dob"] as string;
                                if (string.IsNullOrEmpty(dob))
                                {
                                    person.DateOfBirth = new DateTime(1775, 10, 13);
                                    //TODO LOG ME
                                }
                                else
                                {
                                    DateTime temp;
                                    if (DateTime.TryParseExact(dob, "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out temp))
                                        person.DateOfBirth = temp;
                                    else
                                    {
                                        person.DateOfBirth = new DateTime(1775, 10, 13);
                                        //TODO LOG ME
                                    }
                                }

                                var dod = adminRow["ADM_cmddod"] as string;
                                if (string.IsNullOrEmpty(dod))
                                {
                                    person.DateOfDeparture = null;
                                }
                                else
                                {
                                    DateTime temp;
                                    if (DateTime.TryParseExact(dod, "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out temp))
                                        person.DateOfDeparture = temp;
                                }

                                if (Convert.ToInt32(workRow["RATE_id"]) != 0)
                                {
                                    var designationNewId = oldDatabase.Tables["rate"].AsEnumerable().First(x => Convert.ToInt32(x["RATE_id"]) == Convert.ToInt32(workRow["RATE_id"]))["NewId"] as string;
                                    person.Designation = designations.First(x => x.Id.ToString() == designationNewId);
                                }

                                var divisionNewId = oldDatabase.Tables["div"].AsEnumerable().First(x => Convert.ToInt32(x["DIV_id"]) == Convert.ToInt32(workRow["DIV_id"]))["NewId"] as string;
                                person.Department = person.Command.Departments.FirstOrDefault(x => x.Divisions.Any(y => y.Id.ToString() == divisionNewId));
                                if (person.Department != null)
                                {
                                    person.Division = person.Department.Divisions.First(x => x.Id.ToString() == divisionNewId);
                                }

                                var typeId = Convert.ToInt32(workRow["TYPE_id"]);

                                //We also need to do UIC at this point.
                                var uicId = oldDatabase.Tables["uic"].AsEnumerable().First(x => Convert.ToInt32(x["UIC_id"]) == Convert.ToInt32(adminRow["UIC_id"]))["NewId"] as string;
                                person.UIC = uics.First(x => x.Id.ToString() == uicId);

                                switch (typeId)
                                {
                                    case 1:
                                        {

                                            if (person.UIC.Value == "01234")
                                            {
                                                person.DutyStatus = DutyStatuses.SecondParty;
                                                break;
                                            }

                                            if (person.UIC.Value == "22222")
                                            {
                                                person.DutyStatus = DutyStatuses.TADToCommand;
                                                break;
                                            }

                                            //mil
                                            //Now we need to find out if the person is active or reserves.
                                            if (person.UIC.Value == "00000")
                                            {
                                                person.DutyStatus = DutyStatuses.Reserves;
                                            }
                                            else
                                            {
                                                person.DutyStatus = DutyStatuses.Active;
                                            }
                                            break;
                                        }
                                    case 2:
                                        {
                                            if (person.UIC.Value == "01234")
                                            {
                                                person.DutyStatus = DutyStatuses.SecondParty;
                                                break;
                                            }

                                            if (person.UIC.Value == "22222")
                                            {
                                                person.DutyStatus = DutyStatuses.TADToCommand;
                                                break;
                                            }

                                            //civ
                                            //Now we need to find out what kind of civilian.
                                            if (person.UIC.Value == "11111")
                                            {
                                                person.DutyStatus = DutyStatuses.Contractor;
                                            }
                                            else
                                            {
                                                person.DutyStatus = DutyStatuses.Civilian;
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            throw new NotImplementedException("oh fuck me.");
                                        }
                                }

                                var eaos = adminRow["ADM_eaos"] as string;
                                if (string.IsNullOrEmpty(eaos))
                                {
                                    person.EAOS = null;
                                }
                                else
                                {
                                    DateTime temp;
                                    if (DateTime.TryParseExact(eaos, "yyyy-MM-dd",
                                        CultureInfo.InvariantCulture,
                                        DateTimeStyles.None,
                                        out temp))
                                        person.EAOS = temp;
                                }

                                //email type 0 = work, 1 = home - but I'm not using it. lol.
                                var emailRows = oldDatabase.Tables["email"].AsEnumerable().Where(x => Convert.ToInt32(x["PERS_id"]) == Convert.ToInt32(persRow["PERS_id"])).ToList();
                                person.EmailAddresses = emailRows.Select(email =>
                                    new Entities.EmailAddress
                                    {
                                        Address = email["EMAIL_address"] as string,
                                        Id = Guid.Parse(email["NewId"] as string),
                                        IsContactable = false,
                                        IsPreferred = false
                                    }).ToList();

                                person.EmergencyContactInstructions = "";

                                if (!String.IsNullOrEmpty(persRow["PERS_ethnicity"] as string))
                                {
                                    var ethnicityNewId = ((Newtonsoft.Json.Linq.JArray)parsedEthnicities).First(x => Convert.ToInt32(x.Value<string>("Id")) == Convert.ToInt32(persRow["PERS_ethnicity"])).Value<string>("NewId");
                                    person.Ethnicity = ethnicities.First(x => x.Id.ToString().SafeEquals(ethnicityNewId));
                                }
                                

                                person.FirstName = Utilities.FirstCharacterToUpper((persRow["PERS_fname"] as string).ToLower());
                                person.Id = Guid.Parse(persRow["NewId"] as string);

                                person.IsClaimed = false;

                                person.JobTitle = workRow["WORK_title"] as string;
                                person.LastName = Utilities.FirstCharacterToUpper((persRow["PERS_lname"] as string).ToLower());

                                person.MiddleName = Utilities.FirstCharacterToUpper((persRow["PERS_mi"] as string).ToLower());

                                //Primary is 0, secondary is 1
                                var necRows = oldDatabase.Tables["pers_necs"].AsEnumerable().Where(x => Convert.ToInt32(x["WORK_id"]) == Convert.ToInt32(workRow["WORK_id"])).ToList();

                                if (Convert.ToInt32(workRow["RANK_id"]) != 0)
                                {
                                    var rank = oldDatabase.Tables["rank"].AsEnumerable().First(x => Convert.ToInt32(x["RANK_id"]) == Convert.ToInt32(workRow["RANK_id"]))["RANK_rank"] as string;

                                    if (rank == "CWO")
                                    {
                                        person.Paygrade = Paygrades.CWO2;
                                    }
                                    else
                                    {
                                        person.Paygrade = (Paygrades)Enum.Parse(typeof(Paygrades), rank);
                                    }
                                }

                                //home is 0, work is 1, cell is 2
                                var phoneRows = oldDatabase.Tables["phone"].AsEnumerable().Where(x => Convert.ToInt32(x["PERS_id"]) == Convert.ToInt32(persRow["PERS_id"])).ToList();

                                necRows.ForEach(nec =>
                                   {
                                       var necNewId = oldDatabase.Tables["nec"].AsEnumerable().First(x => Convert.ToInt32(x["NEC_id"]) == Convert.ToInt32(nec["NEC_id"]))["NewId"] as string;

                                       //If is primary
                                       if ((nec["PNEC_type"] as string) == "Primary" || (nec["PNEC_type"] as string) == "0")
                                       {
                                           person.PrimaryNEC = necs.First(x => x.Id.ToString().SafeEquals(necNewId));
                                       }
                                       else
                                       {
                                           if (person.SecondaryNECs == null)
                                           {
                                               person.SecondaryNECs = new List<Entities.ReferenceLists.NEC>();
                                           }
                                           person.SecondaryNECs.Add(necs.First(x => x.Id.ToString().SafeEquals(necNewId)));
                                       }
                                   });

                               

                                person.PhoneNumbers = phoneRows.Select(x =>
                                {
                                    var phone = new Entities.PhoneNumber
                                    {
                                        Id = Guid.Parse(x["NewId"] as string),
                                        IsContactable = false,
                                        IsPreferred = false,
                                        Number = new String((x["PH_number"] as string).Where(Char.IsNumber).ToArray())
                                    };

                                    switch (Convert.ToInt32(x["PH_type"]))
                                    {
                                        case 0:
                                            {
                                                phone.PhoneType = PhoneNumberTypes.Home;
                                                break;
                                            }
                                        case 1:
                                            {
                                                phone.PhoneType = PhoneNumberTypes.Work;
                                                break;
                                            }
                                        case 2:
                                            {
                                                phone.PhoneType = PhoneNumberTypes.Mobile;
                                                break;
                                            }
                                        default:
                                            {
                                                throw new NotImplementedException("fucking shit");
                                            }

                                    }

                                    return phone;

                                }).ToList();

                               

                                //TODO
                                //Home is 0
                                var addressRows = oldDatabase.Tables["address"].AsEnumerable().Where(x => Convert.ToInt32(x["PERS_id"]) == Convert.ToInt32(persRow["PERS_id"])).ToList();

                                //TODO LOG ME
                                var badRows = addressRows.Where(x => String.IsNullOrEmpty(x["ADD_line1"] as string));

                                person.PhysicalAddresses = addressRows.Except(badRows).Select(x =>
                                    {
                                        string streetNumber = null;
                                        string route = null;
                                        if ((x["ADD_line1"] as string).ToLower().Contains("bldg"))
                                        {
                                            route = (x["ADD_line1"] as string);
                                            streetNumber = " ";
                                        }
                                        else
                                        {
                                            streetNumber = (x["ADD_line1"] as string).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).First();
                                            route = String.Join(" ", (x["ADD_line1"] as string).Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).Skip(1));
                                        }
                                        
                                        var address = new Entities.PhysicalAddress
                                        {
                                            City = x["ADD_city"] as string,
                                            Country = "United States of America",
                                            Id = Guid.Parse(x["NewId"] as string),
                                            IsHomeAddress = Convert.ToInt32(x["ADD_type"]) == 0,
                                            StreetNumber = streetNumber,
                                            Route = route,
                                            State = x["ADD_st"] as string,
                                            ZipCode = x["ADD_zip"] as string
                                        };

                                        return address;
                                    }).ToList();

                                if (!String.IsNullOrEmpty(persRow["PERS_relpref"] as string))
                                {
                                    var relPreferenceNewId = ((Newtonsoft.Json.Linq.JArray)parsedRelPreferences).First(x => Convert.ToInt32(x.Value<string>("Id")) == Convert.ToInt32(persRow["PERS_relpref"])).Value<string>("NewId");
                                    person.ReligiousPreference = religiousPreferences.First(x => x.Id.ToString().SafeEquals(relPreferenceNewId));
                                }

                                person.Remarks = persRow["PERS_rmks"] as string;

                                if (String.IsNullOrEmpty(persRow["PERS_sex"] as string))
                                {
                                    person.Sex = Sexes.Female;
                                }
                                else
                                {
                                    person.Sex = (Sexes)Enum.Parse(typeof(Sexes), persRow["PERS_sex"] as string);
                                }
                                

                                person.Shift = workRow["WORK_shift"] as string;

                                person.SSN = persRow["PERS_ssn"] as string;

                                person.Suffix = persRow["PERS_suffix"] as string;

                                person.Supervisor = workRow["WORK_supe"] as string;

                                person.WorkCenter = workRow["WORK_wkcenter"] as string;

                                person.WorkRemarks = workRow["WORK_rmks"] as string;

                                person.WorkRoom = workRow["WORK_room"] as string;

                                return person;
                            }).ToList();

                        //Finally, we're going to make some decisions here.


                        

                        int fails = 0;

                        //Persist the persons.
                        Log.Info("Persisting all members of the command...");
                        persons.ForEach(x =>
                            {
                                try
                                {
                                    session.SaveOrUpdate(x);
                                    session.Flush();
                                }
                                catch
                                {
                                    fails++;
                                    Log.Info("{0} have failed so far.".FormatS(fails));
                                }
                                
                            });

                    }

                    transaction.Commit();
                }
                catch (MySqlException ex)
                {
                    transaction.Rollback();

                    switch (ex.Number)
                    {
                        case 0:
                            {
                                Log.Warning("Old database could not be contacted!");
                                break;
                            }
                        case 1045:
                            {
                                Log.Warning("The Username/password combination for the old database was invalid!");
                                break;
                            }
                        case 1042:
                            {
                                Log.Warning("The old database was either offline or otherwise non-contactable.");
                                break;
                            }
                        default:
                            {
                                Log.Exception(ex, "While attempting to connect to the old database an exception occurred.");
                                break;
                            }
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }


        }
    }
}
