/* WARN: This SQL file wasn't used in the 20210512112804 migration. It was created in case we need
         to reverse the 20210719084148 migration. The original LocationType was created  as
         part of 20210512112804_TableTypes.sql 
 */
create type LocationType as table
(
    Id                              uniqueidentifier not null,
    Country_Code                    varchar(max),
    Country_Name                    varchar(max),
    EnglishDevolvedArea_Code        varchar(max),
    EnglishDevolvedArea_Name        varchar(max),
    Institution_Code                varchar(max),
    Institution_Name                varchar(max),
    LocalAuthority_Code             varchar(max),
    LocalAuthority_OldCode          varchar(max),
    LocalAuthority_Name             varchar(max),
    LocalAuthorityDistrict_Code     varchar(max),
    LocalAuthorityDistrict_Name     varchar(max),
    LocalEnterprisePartnership_Code varchar(max),
    LocalEnterprisePartnership_Name varchar(max),
    MayoralCombinedAuthority_Code   varchar(max),
    MayoralCombinedAuthority_Name   varchar(max),
    MultiAcademyTrust_Code          varchar(max),
    MultiAcademyTrust_Name          varchar(max),
    OpportunityArea_Code            varchar(max),
    OpportunityArea_Name            varchar(max),
    ParliamentaryConstituency_Code  varchar(max),
    ParliamentaryConstituency_Name  varchar(max),
    Region_Code                     varchar(max),
    Region_Name                     varchar(max),
    RscRegion_Code                  varchar(max),
    Sponsor_Code                    varchar(max),
    Sponsor_Name                    varchar(max),
    Ward_Code                       varchar(max),
    Ward_Name                       varchar(max),
    PlanningArea_Code               varchar(max),
    PlanningArea_Name               varchar(max)
);
