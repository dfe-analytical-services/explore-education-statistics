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

// --- Meta data

interface LabelValueMetadata {
  label: string;
  value: number;
}

interface LabelValueHintMetadata extends LabelValueMetadata {
  hint: string;
}

interface ObjectMap<T> {
  [name: string]: T;
}

interface OptionListMetadata<T extends LabelValueMetadata> {
  options: T[];
}

interface OptionMetadata extends OptionListMetadata<LabelValueMetadata> {
  label: string;
}

interface TimePeriodOptionMetadata {
  label: string;
  code: string;
  year: number;
}

interface FilterMetadata {
  hint: string;
  legend: string;
  options: ObjectMap<OptionMetadata>;
}

interface TimePeriodMetadata {
  hint: string;
  legend: string;
  options: TimePeriodOptionMetadata[];
}

interface IndicatorMetadata extends OptionListMetadata<LabelValueHintMetadata> {
  label: string;
}

interface ResponseMetaData {
  subject: {
    id: number;
    label: string;
  };
  filters: ObjectMap<FilterMetadata>;
  indicators: ObjectMap<IndicatorMetadata>;
  timePeriod: TimePeriodMetadata;
}

// ------------------------------------------

export interface DataBlockMetadata {
  indicators: ObjectMap<LabelValueHintMetadata>;
  filters: ObjectMap<LabelValueMetadata>;
  timePeriods: ObjectMap<LabelValueMetadata>;
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
  metaData: DataBlockMetadata;
  data: DataBlockData;
}

function mapOptions<T extends LabelValueMetadata>(ids: number[], options: T[]) {
  return Object.values(options).reduce((results, option) => {
    if (ids.includes(+option.value)) {
      return { ...results, [option.value]: option };
    }

    return results;
  }, {});
}

function mapTimePeriodOptions(
  years: number[],
  options: TimePeriodOptionMetadata[],
) {
  return Object.values(options).reduce((results, option) => {
    if (years.includes(option.year)) {
      return { ...results, [option.year]: option };
    }

    return results;
  }, {});
}

function mapOptionsMap<
  R extends LabelValueMetadata,
  T extends OptionListMetadata<R>
>(ids: number[], options: ObjectMap<T>): ObjectMap<R> {
  return Object.values(options).reduce(
    (mapped, option) => ({ ...mapped, ...mapOptions(ids, option.options) }),
    {},
  );
}

function remapIndicators(
  indicatorIds: number[],
  { indicators }: ResponseMetaData,
): ObjectMap<LabelValueHintMetadata> {
  return mapOptionsMap(indicatorIds, indicators);
}

function remapFilters(
  filterIds: number[],
  { filters }: ResponseMetaData,
): ObjectMap<LabelValueMetadata> {
  return Object.values(filters).reduce(
    (mapped, filter) => ({
      ...mapped,
      ...mapOptionsMap(filterIds, filter.options),
    }),
    {},
  );
}

function remapTimePeriod(
  years: number[],
  { timePeriod }: ResponseMetaData,
): ObjectMap<LabelValueMetadata> {
  return mapTimePeriodOptions(years, timePeriod.options);

  /*
  return Object.values(timePeriod)
    .reduce((mapped, option) => mapIo
        ({...mapped, ...mapOptions(timeIds, option.options)}),
      {});


   */
}

function getUsedTimeIdentifiers(data: DataBlockData) {
  return Array.from(
    data.result.reduce((timeIdentifiers, result) => {
      console.log(result);
      return timeIdentifiers.add(result.year);
    }, new Set<number>()),
  );
}

const DataBlockService = {
  async getDataBlockForSubject(request: DataBlockRequest) {
    // TODO: move all this into the API
    const metaData: ResponseMetaData = await dataApi.get(
      `/meta/subject/${request.subjectId}`,
    );

    const data: DataBlockData = await dataApi.post('/tablebuilder', request);

    const usedIndicators = remapIndicators(request.indicators, metaData);
    const usedFilters = remapFilters(request.filters, metaData);

    const times = getUsedTimeIdentifiers(data);

    const usedTimeIdentifiers = remapTimePeriod(times, metaData);

    const usedMetadata: DataBlockMetadata = {
      indicators: usedIndicators,
      filters: usedFilters,
      timePeriods: usedTimeIdentifiers,
    };

    const response: DataBlockResponse = {
      metaData: usedMetadata,
      data,
    };

    console.log(usedMetadata);

    return response;
  },
};

export default DataBlockService;
