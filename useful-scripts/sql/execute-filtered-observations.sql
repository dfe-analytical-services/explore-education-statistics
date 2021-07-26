begin
    declare @subjectId uniqueidentifier = '0054a4c1-cb69-4eb2-58b5-08d94cfdb0f5'

    declare @geographicLevel nvarchar(6)

    declare @timePeriodList TimePeriodListType
    insert @timePeriodList values (2013, 'AY'), (2016, 'AY')

    declare @countriesList IdListVarcharType
    insert @countriesList values ('E92000001')

    declare @institutionsList IdListVarcharType

    declare @localAuthorityList IdListVarcharType

    declare @localAuthorityOldCodeList IdListVarcharType

    declare @localAuthorityDistrictList IdListVarcharType

    declare @localEnterprisePartnershipsList IdListVarcharType

    declare @mayoralCombinedAuthoritiesList IdListVarcharType

    declare @multiAcademyTrustList IdListVarcharType

    declare @opportunityAreasList IdListVarcharType

    declare @parliamentaryConstituenciesList IdListVarcharType

    declare @providersList IdListVarcharType

    declare @regionsList IdListVarcharType

    declare @rscRegionsList IdListVarcharType

    declare @schoolsList IdListVarcharType

    declare @sponsorList IdListVarcharType

    declare @wardsList IdListVarcharType

    declare @planningAreasList IdListVarcharType

    declare @filterItemList IdListGuidType
    insert @filterItemList values ('6941b991-2c49-42e1-afa1-787ca80de62e'), ('837884a9-f7b0-4d4e-8530-17e530b808c8'), ('3931b288-7a60-487a-8ab8-8772e5e4cff9'), ('edd73f9e-dd50-4db4-8895-b801638f04da'), ('737e7e85-124a-4015-adbc-d6425496df7b'), ('2be17d8e-440b-43e4-8a53-2238af57acae'), ('dee09d7a-834f-4e00-954c-6417d201724b'), ('93dc6c4a-bed1-44ab-8ab4-89f747696e4f'), ('51937aec-ff86-426d-a6ed-1171699788e4'), ('8eadfda6-ce8c-4a63-ba11-db25c1cd3921'), ('c5992ae7-6917-4846-bd9a-985b326e1f82'), ('91558b4d-06a0-4bde-82df-4a39be692a25'), ('e905e5a9-bcde-4258-a569-4f14d91300d2'), ('247b7126-5a04-4c27-8409-d05262375ee9'), ('11a70356-7838-4ad2-a42e-8bdc703195df'), ('ec06de84-292a-4cf4-8272-edc2c06d3aff');
    declare @result int
    exec
        @result = FilteredObservations
                  @subjectId,
                  @geographicLevel,
                  @timePeriodList,
                  @countriesList,
                  @englishDevolvedAreasList,
                  @institutionsList,
                  @localAuthorityList,
                  @localAuthorityOldCodeList,
                  @localAuthorityDistrictList,
                  @localEnterprisePartnershipsList,
                  @mayoralCombinedAuthoritiesList,
                  @multiAcademyTrustList,
                  @opportunityAreasList,
                  @parliamentaryConstituenciesList,
                  @providersList,
                  @regionsList,
                  @rscRegionsList,
                  @schoolsList,
                  @sponsorList,
                  @wardsList,
                  @planningAreasList,
                  @filterItemList
end
