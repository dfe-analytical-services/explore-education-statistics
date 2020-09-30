begin
    declare @geographicLevel nvarchar(6) = 'SCH'

    declare @timePeriodList TimePeriodListType
    insert @timePeriodList values (2016, 'AY')

    declare @countriesList IdListVarcharType

    declare @institutionsList IdListVarcharType

    declare @localAuthorityList IdListVarcharType

    declare @localAuthorityDistrictList IdListVarcharType

    declare @localAuthorityOldCodeList IdListVarcharType

    declare @localEnterprisePartnershipsList IdListVarcharType

    declare @mayoralCombinedAuthoritiesList IdListVarcharType

    declare @multiAcademyTrustList IdListVarcharType

    declare @opportunityAreasList IdListVarcharType

    declare @parliamentaryConstituenciesList IdListVarcharType

    declare @regionsList IdListVarcharType
    insert @regionsList values ('E13000001')

    declare @rscRegionsList IdListVarcharType

    declare @sponsorList IdListVarcharType

    declare @wardsList IdListVarcharType

    declare @planningAreasList IdListVarcharType

    declare @filterItemList IdListGuidType
    insert @filterItemList values ('C62FD826-00B0-4933-995C-0739FA7CD1FE'), ('5C175038-297F-4B0D-89DD-2F6E9E22DB29'), ('006B1702-3D16-4D64-8D57-9336FBB7C4DA')

    declare @result int
    exec
        @result = GetFilteredObservations
                  @geographicLevel,
                  @timePeriodList,
                  @countriesList,
                  @institutionsList,
                  @localAuthorityList,
                  @localAuthorityDistrictList,
                  @localAuthorityOldCodeList,
                  @localEnterprisePartnershipsList,
                  @mayoralCombinedAuthoritiesList,
                  @multiAcademyTrustList,
                  @opportunityAreasList,
                  @parliamentaryConstituenciesList,
                  @regionsList,
                  @rscRegionsList,
                  @sponsorList,
                  @wardsList,
                  @planningAreasList,
                  @filterItemList
end
