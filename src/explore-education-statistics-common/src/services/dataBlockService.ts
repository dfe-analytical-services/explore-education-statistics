import { dataApi } from '@common/services/api';

export enum GeographicLevel {
  Establishment = 'Establishment',
  LocalAuthority = 'Local_Authority',
  LocalAuthorityDistrict = 'Local_Authority_District',
  LocalEnterprisePartnership = 'Local_Enterprise_Partnership',
  MATOrSponsor = 'MAT_Or_Sponsor',
  MayoralCombinedAuthorities = 'Mayoral_Combined_Authorities',
  National = 'National',
  OpportunityAreas = 'Opportunity_Areas',
  ParliamentaryConstituency = 'Parliamentary_Constituency',
  Provider = 'Provider',
  Regional = 'Regional',
  RSCRegion = 'RSC_Region',
  School = 'School',
  Ward = 'Ward',
}

interface Country {
  country_code: string;
  country_name: string;
}

interface Region {
  region_code: string;
  region_name: string;
}

interface LocalAuthority {
  new_la_code: string;
  old_la_code: string;
  la_name: string;
}

interface LocalAuthorityDistrict {
  sch_lad_code: string;
  sch_lad_name: string;
}

interface DataBlockLocation {
  country: Country;
  region: Region;
  localAuthority: LocalAuthority;
  localAuthorityDistrict: LocalAuthorityDistrict;
}

interface Result {
  filters: number[];
  location: DataBlockLocation;
  measures: {
    [key: string]: string;
  };
  timeIdentifier: string;
  year: number;
}

export interface DataBlockData {
  publicationId: string;
  releaseId: number;
  subjectId: number;
  releaseDate: Date;
  geographicLevel: GeographicLevel;
  result: Result[];
}

interface LabelValueMetadata {
  label: string;
  value: string;
}

interface LabelValueHintMetadata extends LabelValueMetadata {
  hint: string;
}

interface OptionMetadata {
  label: string;
  options: LabelValueMetadata[];
}

interface FilterMetadata {
  hint: string;
  legend: string;
  options: {
    [name: string]: OptionMetadata;
  };
}

interface IndicatorMetadata {
  label: string;
  options: LabelValueHintMetadata[];
}

export interface DataBlockMetadata {
  subject: {
    id: number;
    label: string;
  };
  filters: {
    [filterName: string]: FilterMetadata;
  };
  indicators: {
    [indicatorName: string]: IndicatorMetadata;
  };
}

export interface DataBlockRequest {
  subjectId: number;
  geographicLevel: GeographicLevel;
  countries?: number[];
  localAuthorities?: number[];
  localAuthorityDistricts?: number[];
  regions?: number[];
  startYear: number;
  endYear: number;
  filters: number[];
  indicators: number[];
}

export interface DataBlockResponse {
  meta: DataBlockMetadata;
  data: DataBlockData;
}

export interface UsedMetadata {
  indicators: LabelValueHintMetadata;
  filters: LabelValueMetadata;
}

function remapIndicators(indicatorIds: number[], meta: DataBlockMetadata) {
  const { indicators } = meta;

  const mapped: { [indicatorName: string]: LabelValueHintMetadata } = {};

  Object.values(indicators).forEach(indicator => {
    indicator.options.forEach(option => {
      if (indicatorIds.includes(+option.value)) {
        if (mapped[option.value])
          console.error(`Existing indicator ${option.value}`);
        mapped[option.value] = option;
      }
    });
  });

  return mapped;
}

function remapFilters(filterIds: number[], meta: DataBlockMetadata) {
  const { filters } = meta;

  const mapped: { [filterName: string]: LabelValueMetadata } = {};

  Object.values(filters).forEach(filter => {
    Object.values(filter.options).forEach(filterOption => {
      filterOption.options.forEach(option => {
        if (filterIds.includes(+option.value)) {
          if (mapped[option.value])
            console.error(`Existing indicator ${option.value}`);
          mapped[option.value] = option;
        }
      });
    });
  });

  return mapped;
}

const DataBlockService = {
  async getDataBlockForSubject(request: DataBlockRequest) {
    const metaData: DataBlockMetadata = await dataApi.get(
      `/meta/subject/${request.subjectId}`,
    );

    const data: DataBlockData = await dataApi.post('/tablebuilder', request);

    const usedIndicators = remapIndicators(request.indicators, metaData);

    console.log(usedIndicators);

    const usedFilters = remapFilters(request.filters, metaData);

    console.log(usedFilters);

    return {
      metaData,
      data,
    };
  },
};

export default DataBlockService;
