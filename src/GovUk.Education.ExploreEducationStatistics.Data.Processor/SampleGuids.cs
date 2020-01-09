using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor
{
    public static class SampleGuids
    {
        private static readonly string Absence_By_Characteristic = "803fbf56-600f-490f-8409-6413a891720d";
        private static readonly string Absence_By_Geographic_Level = "568576e5-d386-450e-a8db-307b7061d0d8";
        private static readonly string Absence_By_Term = "b7bc537b-0c04-4b15-9eb6-4f0e8cc2e70a";
        private static readonly string Absence_For_Four_Years_Olds = "353db5ea-befd-488b-ad16-2ce7963c9bc9";
        private static readonly string Absence_In_Prus = "95c7f584-907e-4756-bbf0-4905ceae57df";
        private static readonly string Absence_Number_Missing_At_Least_One_Session_By_Reason = "faf2152e-0a6c-4e97-af02-e9a89d48c47a";
        private static readonly string Absence_Rate_Percent_Bands = "666cd878-87bb-4f77-9a3f-f5c75078e112";
        private static readonly string Exclusions_By_Geographic_Level = "3c0fbe56-0a4b-4caa-82f2-ab696cd96090";
        private static readonly string EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018 = "8e3d1bc0-2beb-4dc6-9db7-3d27d0608042";
        private static readonly string School_Applications_And_Offers = "fa0d7f1d-d181-43fb-955b-fc327da86f2c";

        private static readonly Dictionary<string, Guid> FilterGroupGuids =
            new Dictionary<string, Guid>
            {
                {$"{Absence_By_Characteristic}:school_type:Default", new Guid("4941583b-1f6c-4586-a71f-c144f7298e18")},

                {$"{Absence_Rate_Percent_Bands}:school_type:Default", new Guid("c93635d2-83ad-4199-bd0d-0e740775c4ed")},
            };

        private static readonly Dictionary<string, Guid> FilterGuids =
            new Dictionary<string, Guid>
            {
                {$"{Absence_By_Characteristic}:school_type", new Guid("67a19370-6251-4678-84b3-5d2d379b903b")},

                {$"{Absence_Rate_Percent_Bands}:school_type", new Guid("51e645c1-4a37-4938-8b20-1244b15048f9")},
            };

        private static readonly Dictionary<string, Guid> FilterItemGuids =
            new Dictionary<string, Guid>
            {
                {$"{Absence_By_Characteristic}:school_type:Default:Total", new Guid("cb9b57e8-9965-4cb6-b61a-acc6d34b32be")},
                {$"{Absence_By_Characteristic}:characteristic:Total:Total", new Guid("183f94c3-b5d7-4868-892d-c948e256744d")},
                {$"{Absence_By_Characteristic}:characteristic:Gender:Gender male", new Guid("eb6013a7-6e69-4ab6-b51e-b7a3e68256ae")},
                {$"{Absence_By_Characteristic}:characteristic:Gender:Gender female", new Guid("ab381336-5e81-4caa-9e59-6e9067e8fb04")},
                {$"{Absence_By_Characteristic}:school_type:Default:State-funded primary", new Guid("d7e7e412-f462-444f-84ac-3454fa471cb8")},
                {$"{Absence_By_Characteristic}:school_type:Default:State-funded secondary", new Guid("a9fe9fa6-e91f-460b-a0b1-66877b97c581")},
                {$"{Absence_By_Characteristic}:school_type:Default:Special", new Guid("b3207d77-143b-43d5-8b48-32d29727e96f")},
                {$"{Absence_By_Characteristic}:characteristic:FSM:FSM eligible", new Guid("278ce0cb-f1d2-4e8f-84d1-ad794352c925")},
                {$"{Absence_By_Characteristic}:characteristic:FSM:FSM not eligible", new Guid("bc6e41d5-5078-4207-b0ae-69261ac192e0")},
                {$"{Absence_By_Characteristic}:characteristic:FSM:FSM unclassified", new Guid("c649015c-8203-43aa-b424-dfb897766391")},
                {$"{Absence_By_Characteristic}:characteristic:Ethnic group minor:Ethnicity Minor Irish", new Guid("5a18e770-73b9-4b4f-8c3c-b3130b810422")},
                
                {$"{Absence_By_Geographic_Level}:school_type:Default:Total", new Guid("0b0daf53-3dd5-4b9b-913e-f4518f4afb96")},
                {$"{Absence_By_Geographic_Level}:school_type:Default:State-funded primary", new Guid("006b1702-3d16-4d64-8d57-9336fbb7c4da")},
                {$"{Absence_By_Geographic_Level}:school_type:Default:State-funded secondary", new Guid("5c175038-297f-4b0d-89dd-2f6e9e22db29")},
                {$"{Absence_By_Geographic_Level}:school_type:Default:Special", new Guid("c62fd826-00b0-4933-995c-0739fa7cd1fe")},
                
                {$"{Absence_By_Term}:school_type:Default:State-funded primary", new Guid("514b404f-ca0c-4568-9539-9dea79d43bc8")},
                {$"{Absence_By_Term}:school_type:Default:State-funded secondary", new Guid("df0506d1-456a-4b10-b0a6-268d59c1924e")},
                {$"{Absence_By_Term}:school_type:Default:Special", new Guid("23896559-d8cc-4c0d-b839-1a60d43e5928")},
                
                {$"{Absence_For_Four_Years_Olds}:school_type:Default:Total", new Guid("26f7f3d0-b7ad-4517-8815-e0c70295ff3b")},
                {$"{Absence_For_Four_Years_Olds}:school_type:Default:State-funded secondary", new Guid("442710bd-5fc6-4096-a4ae-05cb092ede15")},
                {$"{Absence_For_Four_Years_Olds}:school_type:Default:State-funded primary", new Guid("2f219985-8900-4a84-b096-89d133ef8bc6")},
                {$"{Absence_For_Four_Years_Olds}:school_type:Default:Special", new Guid("d2cbfaa7-9768-4845-9d86-48e352635754")},

                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:school_type:Default:Total", new Guid("0089b1b3-626c-4326-a2ec-a4b99b4735bc")},
                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:school_type:Default:State-funded secondary", new Guid("d2b2e7af-d7b7-4526-817c-cc4fc1d010c3")},
                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:school_type:Default:Special", new Guid("4bd075b2-d4de-48db-a007-dc2f9226d206")},
                
                {$"{Absence_Rate_Percent_Bands}:school_type:Default:Total", new Guid("0af225c6-c70b-4053-b7e7-4e719e2b751f")},
                {$"{Absence_Rate_Percent_Bands}:school_type:Default:State-funded secondary", new Guid("c306ff42-ddea-4cd0-82af-770df078fd94")},
                {$"{Absence_Rate_Percent_Bands}:school_type:Default:State-funded primary", new Guid("26db426f-fb4f-439c-9e37-360451305013")},
                {$"{Absence_Rate_Percent_Bands}:school_type:Default:Special", new Guid("a7a7a691-a49e-422e-839e-53f1f545fa76")},
                
                {$"{Exclusions_By_Geographic_Level}:school_type:Default:Total", new Guid("1f3f86a4-de9f-43d7-5bfd-08d78f900a85")},
                
                {$"{EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018}:characteristic:Total:Total", new Guid("beeaa217-3233-48df-bc1d-11f066a26efe")},
                {$"{EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018}:characteristic:Gender:Gender Male", new Guid("2cf47ea3-1891-4bba-9381-81a0305a7581")},
                {$"{EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018}:characteristic:Gender:Gender Female", new Guid("edcc7822-d88e-490d-8446-baca8b6ccca4")},
                
                {$"{School_Applications_And_Offers}:nc_year_admission:Primary:All primary", new Guid("e957db0c-3bf8-4e4b-5c6f-08d78f900a85")},
                {$"{School_Applications_And_Offers}:nc_year_admission:Primary:R", new Guid("f5ad9114-14b8-4102-89a1-3ab76801ecde")},
                {$"{School_Applications_And_Offers}:nc_year_admission:Secondary:All secondary", new Guid("5a7b4e97-7794-4037-5c71-08d78f900a85")},
                {$"{School_Applications_And_Offers}:nc_year_admission:Secondary:9", new Guid("3f101896-1c4a-4153-bb22-1d3888eb61ea")},
                {$"{School_Applications_And_Offers}:nc_year_admission:Secondary:7", new Guid("ff8614ba-ec1c-4012-a5e3-2d788a4f5460")},
            };
        
        private static readonly Dictionary<string, Guid> IndicatorGuids =
            new Dictionary<string, Guid>
            {
                {$"{Absence_By_Characteristic}:Absence fields:enrolments", new Guid("1293b484-93f5-4177-a451-fcf4467b26a2")},
                {$"{Absence_By_Characteristic}:Absence fields:sess_possible", new Guid("74c8ab3a-0ec3-4348-852c-9055f354f86f")},
                {$"{Absence_By_Characteristic}:Absence fields:sess_unauthorised_percent", new Guid("ccfe716a-6976-4dc3-8fde-a026cd30f3ae")},
                {$"{Absence_By_Characteristic}:Absence fields:sess_overall_percent", new Guid("92d3437a-0a62-4cd7-8dfb-bcceba7eef61")},
                {$"{Absence_By_Characteristic}:Absence fields:sess_authorised_percent", new Guid("f9ae4976-7cd3-4718-834a-09349b6eb377")},
                {$"{Absence_By_Characteristic}:Absence fields:enrolments_pa_10_exact_percent", new Guid("a93b664a-c537-4bb0-8d06-b4ce9bf60ff9")},
                {$"{Absence_By_Characteristic}:Absence fields:enrolments_pa_10_exact", new Guid("5a59cfeb-19e7-486c-906e-f2ad45f896f6")},
                {$"{Absence_By_Characteristic}:Absence for persistent absentees:sess_authorised_pa_10_exact", new Guid("71c77c7d-dec5-4c31-baa5-b59f3c8c8f2e")},
                {$"{Absence_By_Characteristic}:Absence by reason:sess_unauth_totalreasons", new Guid("a3b1afa4-b3de-44c6-b8b2-0ef59f211a2a")},
                {$"{Absence_By_Characteristic}:Absence by reason:sess_unauth_other", new Guid("71761e61-6a80-400f-8778-a8761306eb77")},
                {$"{Absence_By_Characteristic}:Absence by reason:sess_unauth_noyet", new Guid("b4cc7de4-30f3-495b-a967-0d2a9473583f")},
                {$"{Absence_By_Characteristic}:Absence by reason:sess_unauth_late", new Guid("57452cb9-6bda-495a-a012-7fda71e4bf90")},
                
                {$"{Absence_By_Geographic_Level}:Absence fields:sess_overall_percent", new Guid("c5358a0e-50be-4de9-9a7a-366b96c21d2a")},
                {$"{Absence_By_Geographic_Level}:Absence fields:sess_authorised_percent", new Guid("af786942-5ddc-4e41-8f98-61ca95931985")},
                {$"{Absence_By_Geographic_Level}:Absence fields:sess_unauthorised_percent", new Guid("2d08d922-d57e-404d-971c-18fb386d3183")},
                {$"{Absence_By_Geographic_Level}:Absence for persistent absentees:sess_authorised_percent_pa_10_exact", new Guid("1c3cb6ab-1917-448d-b67c-6b7d8438b7ce")},
                {$"{Absence_By_Geographic_Level}:Absence by reason:sess_auth_illness", new Guid("94f73d1c-dc74-4cae-a2c0-1534c634a4ef")},
                {$"{Absence_By_Geographic_Level}:Absence by reason:sess_auth_ext_holiday", new Guid("1aa8b098-356d-4b19-942d-e6bc0fded7d8")},

                {$"{Absence_By_Term}:Absence fields:sess_possible", new Guid("cb1fc409-b8fd-482a-aee0-627860cde918")},
                {$"{Absence_By_Term}:Absence fields:sess_unauthorised", new Guid("1199a5e5-eed7-4261-98b9-0a3727104176")},
                {$"{Absence_By_Term}:Absence fields:sess_authorised", new Guid("7a690779-08f3-40c3-80a4-ef5ab1fc0995")},
                {$"{Absence_By_Term}:Absence fields:sess_overall", new Guid("a648c8b6-268e-4781-8fb7-801426a270ac")},

                {$"{Absence_For_Four_Years_Olds}:Absence fields:enrolments", new Guid("f48c43ca-e6a2-4f16-a5ee-85899536f0a7")},
                {$"{Absence_For_Four_Years_Olds}:Absence fields:sess_overall_percent", new Guid("d7113560-e535-421e-9a5b-b6cd7528f4d4")},
                {$"{Absence_For_Four_Years_Olds}:Absence fields:sess_overall", new Guid("2550def1-9732-458e-812d-c70d034ec51d")},

                {$"{Absence_In_Prus}:Absence fields:num_schools", new Guid("9472758e-6c4a-4cb2-8270-d0551ce91494")},
                {$"{Absence_In_Prus}:Absence fields:sess_unauthorised_percent", new Guid("6d2de8c1-15d7-4cc4-a60a-d512b7876e4d")},
                {$"{Absence_In_Prus}:Absence fields:sess_authorised_percent", new Guid("6160c4f8-4c9f-40f0-a623-2a4f742860af")},
                {$"{Absence_In_Prus}:Absence fields:sess_overall_percent", new Guid("ee11e1cb-2d9a-4d6d-8e6c-8d32f24fa08f")},
                
                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:Absence fields:enrol_unauthorised", new Guid("a07cb02d-a07b-4928-a93a-fe1192011998")},
                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:Absence fields:enrol_authorised", new Guid("037e2587-2c2c-4bec-83e5-12e3cc35c836")},
                {$"{Absence_Number_Missing_At_Least_One_Session_By_Reason}:Absence fields:enrol_overall", new Guid("ff1ccb6e-5faa-43bc-bd6d-be52f754bde6")},

                {$"{Absence_Rate_Percent_Bands}:Enrolments by absence percentage band:enrolments_overall", new Guid("fe313349-0438-41b7-8944-109690ee5158")},
                {$"{Absence_Rate_Percent_Bands}:Enrolments by absence percentage band:enrolments_authorised", new Guid("f3014e60-534a-4667-b90f-80b1fee6b08e")},
                {$"{Absence_Rate_Percent_Bands}:Enrolments by absence percentage band:enrolments_unauthorised", new Guid("cd2711ff-3dba-4452-858a-d55c5cfd04fb")},

                {$"{Exclusions_By_Geographic_Level}:Default:num_schools", new Guid("b3df4fb1-dae3-4c16-4c01-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:headcount", new Guid("a5a58f92-aba1-4955-4c02-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:perm_excl", new Guid("167f4807-4fdd-461a-4c03-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:perm_excl_rate", new Guid("be3b765b-005f-4279-4c04-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:fixed_excl", new Guid("f045bc8d-8dd1-4f16-4c05-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:fixed_excl_rate", new Guid("68aeda43-2b6a-433a-4c06-08d78f90080f")},
                {$"{Exclusions_By_Geographic_Level}:Default:one_plus_fixed_rate", new Guid("732f0d7b-dcd3-4bf8-4c08-08d78f90080f")},
                
                {$"{School_Applications_And_Offers}:Applications:applications_received", new Guid("020a4da6-1111-443d-af80-3a425c558d14")},
                {$"{School_Applications_And_Offers}:Applications:online_applications", new Guid("f472e6cc-9e25-401b-9fca-9dc3755bab2d")},
                {$"{School_Applications_And_Offers}:Applications:online_apps_percent", new Guid("0af5ea39-828f-4afe-9a9f-643dce0112cf")},
                {$"{School_Applications_And_Offers}:Admissions:admission_numbers", new Guid("49d2a1f4-e4a9-4f25-4c24-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:first_preference_offers", new Guid("94f9b11c-df82-4eef-4c29-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:second_preference_offers", new Guid("d22e1104-de56-4617-4c2a-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:third_preference_offers", new Guid("319dd956-a714-40fd-4c2b-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:one_of_the_three_preference_offers", new Guid("a9211c9d-b467-48d7-4c2c-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:preferred_school_offer", new Guid("be1e1643-f7c8-40b0-4c2d-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:non_preferred_offer", new Guid("16cdfc0a-f66f-496b-4c2e-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:no_offer", new Guid("2c63589e-b5d4-4922-4c2f-08d78f90080f")},
                {$"{School_Applications_And_Offers}:Preferences breakdowns:schools_in_la_offer", new Guid("d10d4f10-c2f8-4120-4c30-08d78f90080f")},

                {$"{EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018}:Overall rating:point_score", new Guid("7d250efe-afad-4117-8ccf-debf689c4efc")},
                {$"{EYFSP_APS_GLD_ELG_Underlying_Data_2013_2018}:Overall rating:average_point_score", new Guid("37faaef4-5b5e-473e-9585-8580c1df967a")},
            };
        
        public static void GenerateIndicatorGuids(StatisticsDbContext dbContext) {
             foreach (var indicator in dbContext.Indicator.Local.ToList())
             {
                 var key = $"{indicator.IndicatorGroup.SubjectId}:{indicator.IndicatorGroup.Label}:{indicator.Name}";
                 var id = GetGuid(IndicatorGuids, key);
                 if (id != Guid.Empty)
                 {
                     indicator.Id = id;
                 }
             }
        }

        public static void GenerateFilterGuids(StatisticsDbContext dbContext) {
            foreach (var filter in dbContext.Filter.Local.ToList())
            {
                var key = $"{filter.SubjectId}:{filter.Name}";
                var id = GetGuid(FilterGuids, key);
                if (id != Guid.Empty)
                {
                    filter.Id = id;
                }
            }
        }

        public static void GenerateFilterGroupGuids(StatisticsDbContext dbContext) {
            foreach (var filterGroup in dbContext.FilterGroup.Local.ToList())
            {
                var key = $"{filterGroup.Filter.SubjectId}:{filterGroup.Filter.Name}:{filterGroup.Label}";
                var id = GetGuid(FilterGroupGuids, key);
                if (id != Guid.Empty)
                {
                    filterGroup.Id = id;
                }
            }
        }

        public static void GenerateFilterItemGuids(StatisticsDbContext dbContext)
        {
            foreach (var filterItem in dbContext.FilterItem.Local.ToList())
            {
                var key = $"{filterItem.FilterGroup.Filter.SubjectId}:{filterItem.FilterGroup.Filter.Name}:{filterItem.FilterGroup.Label}:{filterItem.Label}";
                var filterItemId = GetGuid(FilterItemGuids, key);
                if (filterItemId != Guid.Empty)
                {
                    filterItem.Id = filterItemId;
                }
            }
        }
        private static Guid GetGuid(Dictionary<string, Guid> d, string key)
        {
            d.TryGetValue(key, out Guid id);
            return id;
        }
    }
}