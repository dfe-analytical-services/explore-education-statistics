begin
    declare @subjectId uniqueidentifier = '568576e5-d386-450e-a8db-307b7061d0d8'

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

    declare @result int
    exec
        @result = FilteredObservations
                  @subjectId,
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
