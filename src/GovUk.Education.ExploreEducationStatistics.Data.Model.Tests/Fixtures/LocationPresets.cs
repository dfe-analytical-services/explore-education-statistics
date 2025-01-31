#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Tests.Fixtures;

public static class CountryPresets
{
    public static Country England => new("E92000001", "England");
}

public static class RegionPresets
{
    public static Region EastMidlands => new("E12000004", "East Midlands");
    public static Region EastOfEngland => new("E12000006", "East of England");
    public static Region InnerLondon => new("E13000001", "Inner London");
    public static Region NorthEast => new("E12000001", "North East");
    public static Region NorthWest => new("E12000002", "North West");
    public static Region OuterLondon => new("E13000002", "Outer London");
    public static Region SouthEast => new("E12000008", "South East");
    public static Region SouthWest => new("E12000009", "South West");
    public static Region WestMidlands => new("E12000005", "West Midlands");
    public static Region YorkshireAndTheHumber => new("E12000003", "Yorkshire and the Humber");
}

public static class LocalAuthorityPresets
{
    public static List<LocalAuthority> EastMidlands => new()
    {
        new LocalAuthority("E06000015", "831", "Derby"),
        new LocalAuthority("E06000016", "856", "Leicester"),
        new LocalAuthority("E06000017", "857", "Rutland"),
        new LocalAuthority("E06000018", "892", "Nottingham"),
        new LocalAuthority("E10000007", "830", "Derbyshire"),
        new LocalAuthority("E10000018", "855", "Leicestershire"),
        new LocalAuthority("E10000019", "925", "Lincolnshire"),
        new LocalAuthority("E10000021", "928", "Northamptonshire"),
        new LocalAuthority("E10000024", "891", "Nottinghamshire"),
    };

    public static List<LocalAuthority> EastOfEngland => new()
    {
        new LocalAuthority("E06000031", "874", "Peterborough"),
        new LocalAuthority("E06000032", "821", "Luton"),
        new LocalAuthority("E06000033", "882", "Southend-on-Sea"),
        new LocalAuthority("E06000034", "883", "Thurrock"),
        new LocalAuthority("E06000055", "822", "Bedford"),
        new LocalAuthority("E06000056", "823", "Central Bedfordshire"),
        new LocalAuthority("E10000003", "873", "Cambridgeshire"),
        new LocalAuthority("E10000012", "881", "Essex"),
        new LocalAuthority("E10000015", "919", "Hertfordshire"),
        new LocalAuthority("E10000020", "926", "Norfolk"),
        new LocalAuthority("E10000029", "935", "Suffolk"),
    };

    public static List<LocalAuthority> InnerLondon => new()
    {
        new LocalAuthority("E09000001", "201", "City of London"),
        new LocalAuthority("E09000007", "202", "Camden"),
        new LocalAuthority("E09000012", "204", "Hackney"),
        new LocalAuthority("E09000013", "205", "Hammersmith and Fulham"),
        new LocalAuthority("E09000014", "309", "Haringey"),
        new LocalAuthority("E09000019", "206", "Islington"),
        new LocalAuthority("E09000020", "207", "Kensington and Chelsea"),
        new LocalAuthority("E09000022", "208", "Lambeth"),
        new LocalAuthority("E09000023", "209", "Lewisham"),
        new LocalAuthority("E09000025", "316", "Newham"),
        new LocalAuthority("E09000028", "210", "Southwark"),
        new LocalAuthority("E09000030", "211", "Tower Hamlets"),
        new LocalAuthority("E09000032", "212", "Wandsworth"),
        new LocalAuthority("E09000033", "213", "Westminster"),
    };

    public static List<LocalAuthority> NorthEast => new()
    {
        new LocalAuthority("E06000001", "805", "Hartlepool"),
        new LocalAuthority("E06000002", "806", "Middlesbrough"),
        new LocalAuthority("E06000003", "807", "Redcar and Cleveland"),
        new LocalAuthority("E06000004", "808", "Stockton-on-Tees"),
        new LocalAuthority("E06000005", "841", "Darlington"),
        new LocalAuthority("E06000047", "840", "Durham"),
        new LocalAuthority("E06000048", "929", "Northumberland"),
        new LocalAuthority("E06000057", "929", "Northumberland"),
        new LocalAuthority("E08000020", "390", "Gateshead"),
        new LocalAuthority("E08000021", "391", "Newcastle upon Tyne"),
        new LocalAuthority("E08000022", "392", "North Tyneside"),
        new LocalAuthority("E08000023", "393", "South Tyneside"),
        new LocalAuthority("E08000024", "394", "Sunderland"),
        new LocalAuthority("E08000037", "390", "Gateshead"),
    };

    public static List<LocalAuthority> NorthWest => new()
    {
        new LocalAuthority("E06000006", "876", "Halton"),
        new LocalAuthority("E06000007", "877", "Warrington"),
        new LocalAuthority("E06000008", "889", "Blackburn with Darwen"),
        new LocalAuthority("E06000009", "890", "Blackpool"),
        new LocalAuthority("E06000049", "895", "Cheshire East"),
        new LocalAuthority("E06000050", "896", "Cheshire West and Chester"),
        new LocalAuthority("E08000001", "350", "Bolton"),
        new LocalAuthority("E08000002", "351", "Bury"),
        new LocalAuthority("E08000003", "352", "Manchester"),
        new LocalAuthority("E08000004", "353", "Oldham"),
        new LocalAuthority("E08000005", "354", "Rochdale"),
        new LocalAuthority("E08000006", "355", "Salford"),
        new LocalAuthority("E08000007", "356", "Stockport"),
        new LocalAuthority("E08000008", "357", "Tameside"),
        new LocalAuthority("E08000009", "358", "Trafford"),
        new LocalAuthority("E08000010", "359", "Wigan"),
        new LocalAuthority("E08000011", "340", "Knowsley"),
        new LocalAuthority("E08000012", "341", "Liverpool"),
        new LocalAuthority("E08000013", "342", "St Helens"),
        new LocalAuthority("E08000013", "342", "St. Helens"),
        new LocalAuthority("E08000014", "343", "Sefton"),
        new LocalAuthority("E08000015", "344", "Wirral"),
        new LocalAuthority("E10000006", "909", "Cumbria"),
        new LocalAuthority("E10000017", "888", "Lancashire"),
    };

    public static List<LocalAuthority> OuterLondon => new()
    {
        new LocalAuthority("E09000002", "301", "Barking and Dagenham"),
        new LocalAuthority("E09000003", "302", "Barnet"),
        new LocalAuthority("E09000004", "303", "Bexley"),
        new LocalAuthority("E09000005", "304", "Brent"),
        new LocalAuthority("E09000006", "305", "Bromley"),
        new LocalAuthority("E09000008", "306", "Croydon"),
        new LocalAuthority("E09000009", "307", "Ealing"),
        new LocalAuthority("E09000010", "308", "Enfield"),
        new LocalAuthority("E09000011", "203", "Greenwich"),
        new LocalAuthority("E09000015", "310", "Harrow"),
        new LocalAuthority("E09000016", "311", "Havering"),
        new LocalAuthority("E09000017", "312", "Hillingdon"),
        new LocalAuthority("E09000018", "313", "Hounslow"),
        new LocalAuthority("E09000021", "314", "Kingston upon Thames"),
        new LocalAuthority("E09000024", "315", "Merton"),
        new LocalAuthority("E09000026", "317", "Redbridge"),
        new LocalAuthority("E09000027", "318", "Richmond upon Thames"),
        new LocalAuthority("E09000029", "319", "Sutton"),
        new LocalAuthority("E09000031", "320", "Waltham Forest"),
    };

    public static List<LocalAuthority> SouthEast => new()
    {
        new LocalAuthority("E06000035", "887", "Medway"),
        new LocalAuthority("E06000036", "867", "Bracknell Forest"),
        new LocalAuthority("E06000037", "869", "West Berkshire"),
        new LocalAuthority("E06000038", "870", "Reading"),
        new LocalAuthority("E06000039", "871", "Slough"),
        new LocalAuthority("E06000040", "868", "Windsor and Maidenhead"),
        new LocalAuthority("E06000041", "872", "Wokingham"),
        new LocalAuthority("E06000042", "826", "Milton Keynes"),
        new LocalAuthority("E06000043", "846", "Brighton and Hove"),
        new LocalAuthority("E06000044", "851", "Portsmouth"),
        new LocalAuthority("E06000045", "852", "Southampton"),
        new LocalAuthority("E06000046", "921", "Isle of Wight"),
        new LocalAuthority("E10000002", "825", "Buckinghamshire"),
        new LocalAuthority("E10000011", "845", "East Sussex"),
        new LocalAuthority("E10000014", "850", "Hampshire"),
        new LocalAuthority("E10000016", "886", "Kent"),
        new LocalAuthority("E10000025", "931", "Oxfordshire"),
        new LocalAuthority("E10000030", "936", "Surrey"),
        new LocalAuthority("E10000032", "938", "West Sussex"),
    };

    public static List<LocalAuthority> SouthWest => new()
    {
        new LocalAuthority("E06000022", "800", "Bath and North East Somerset"),
        new LocalAuthority("E06000023", "801", "Bristol City of"),
        new LocalAuthority("E06000024", "802", "North Somerset"),
        new LocalAuthority("E06000025", "803", "South Gloucestershire"),
        new LocalAuthority("E06000026", "879", "Plymouth"),
        new LocalAuthority("E06000027", "880", "Torbay"),
        new LocalAuthority("E06000028", "837", "Bournemouth"),
        new LocalAuthority("E06000029", "836", "Poole"),
        new LocalAuthority("E06000030", "866", "Swindon"),
        new LocalAuthority("E06000052", "908", "Cornwall"),
        new LocalAuthority("E06000053", "420", "Isles Of Scilly"),
        new LocalAuthority("E06000054", "865", "Wiltshire"),
        new LocalAuthority("E10000008", "878", "Devon"),
        new LocalAuthority("E10000009", "835", "Dorset"),
        new LocalAuthority("E10000013", "916", "Gloucestershire"),
        new LocalAuthority("E10000027", "933", "Somerset"),
    };

    public static List<LocalAuthority> WestMidlands => new()
    {
        new LocalAuthority("E06000019", "884", "Herefordshire"),
        new LocalAuthority("E06000020", "894", "Telford and Wrekin"),
        new LocalAuthority("E06000021", "861", "Stoke-on-Trent"),
        new LocalAuthority("E06000051", "893", "Shropshire"),
        new LocalAuthority("E08000025", "330", "Birmingham"),
        new LocalAuthority("E08000026", "331", "Coventry"),
        new LocalAuthority("E08000027", "332", "Dudley"),
        new LocalAuthority("E08000028", "333", "Sandwell"),
        new LocalAuthority("E08000029", "334", "Solihull"),
        new LocalAuthority("E08000030", "335", "Walsall"),
        new LocalAuthority("E08000031", "336", "Wolverhampton"),
        new LocalAuthority("E10000028", "860", "Staffordshire"),
        new LocalAuthority("E10000031", "937", "Warwickshire"),
        new LocalAuthority("E10000034", "885", "Worcestershire"),
    };

    public static List<LocalAuthority> YorkshireAndTheHumber => new()
    {
        new LocalAuthority("E06000010", "810", "Kingston upon Hull City of"),
        new LocalAuthority("E06000011", "811", "East Riding of Yorkshire"),
        new LocalAuthority("E06000012", "812", "North East Lincolnshire"),
        new LocalAuthority("E06000013", "813", "North Lincolnshire"),
        new LocalAuthority("E06000014", "816", "York"),
        new LocalAuthority("E08000016", "370", "Barnsley"),
        new LocalAuthority("E08000017", "371", "Doncaster"),
        new LocalAuthority("E08000018", "372", "Rotherham"),
        new LocalAuthority("E08000019", "373", "Sheffield"),
        new LocalAuthority("E08000032", "380", "Bradford"),
        new LocalAuthority("E08000033", "381", "Calderdale"),
        new LocalAuthority("E08000034", "382", "Kirklees"),
        new LocalAuthority("E08000035", "383", "Leeds"),
        new LocalAuthority("E08000036", "384", "Wakefield"),
        new LocalAuthority("E10000023", "815", "North Yorkshire"),
    };
}

public static class LocationHierarchyPresets
{
    public static readonly Lazy<Dictionary<Region, List<LocalAuthority>>> RegionLocalAuthorities = new(
        () =>
            new Dictionary<Region, List<LocalAuthority>>
            {
                {
                    RegionPresets.EastMidlands,
                    LocalAuthorityPresets.EastMidlands
                },
                {
                    RegionPresets.EastOfEngland,
                    LocalAuthorityPresets.EastOfEngland
                },
                {
                    RegionPresets.InnerLondon,
                    LocalAuthorityPresets.InnerLondon
                },
                {
                    RegionPresets.NorthEast,
                    LocalAuthorityPresets.NorthEast
                },
                {
                    RegionPresets.NorthWest,
                    LocalAuthorityPresets.NorthWest
                },
                {
                    RegionPresets.OuterLondon,
                    LocalAuthorityPresets.OuterLondon
                },
                {
                    RegionPresets.SouthEast,
                    LocalAuthorityPresets.SouthEast
                },
                {
                    RegionPresets.SouthWest,
                    LocalAuthorityPresets.SouthWest
                },
                {
                    RegionPresets.WestMidlands,
                    LocalAuthorityPresets.WestMidlands
                },
                {
                    RegionPresets.YorkshireAndTheHumber,
                    LocalAuthorityPresets.YorkshireAndTheHumber
                },
            }
    );
}
