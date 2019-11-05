begin
	declare @subjectId int = 2

	declare @geographicLevel nvarchar(6) = 'SCH'

	declare @timePeriodList TimePeriodListType
	insert @timePeriodList values (2016, 'AY')

	declare @countriesList IdListVarcharType

	declare @institutionsList IdListVarcharType

	declare @localAuthorityList IdListVarcharType

	declare @localAuthorityDistrictList IdListVarcharType

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

	declare @filterItemList IdListIntegerType
	insert @filterItemList values (83), (86)

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
			@localEnterprisePartnershipsList,
			@mayoralCombinedAuthoritiesList,
			@multiAcademyTrustList,
			@opportunityAreasList,
			@parliamentaryConstituenciesList,
			@regionsList,
			@rscRegionsList,
			@sponsorList,
			@wardsList,
			@filterItemList
end
