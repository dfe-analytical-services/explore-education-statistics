import { dataApi } from '@common/services/api';
import TimePeriod, { TimePeriodCode } from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import { Feature, Geometry } from 'geojson';

import LocationService from './temporaryLocationService';

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

export interface DataBlockLocation {
  country: Country;
  region: Region;
  localAuthority: LocalAuthority;
  localAuthorityDistrict: LocalAuthorityDistrict;
}

export interface Result {
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
  value: string;
}

interface LabelValueUnitMetadata extends LabelValueMetadata {
  unit: string;
}

interface OptionListMetadata<T extends LabelValueMetadata> {
  options: T[];
}

interface OptionMetadata extends OptionListMetadata<LabelValueMetadata> {
  label: string;
}

interface TimePeriodOptionMetadata {
  label: string;
  code: TimePeriodCode;
  year: number;
}

interface FilterMetadata {
  hint: string;
  legend: string;
  options: Dictionary<OptionMetadata>;
}

interface TimePeriodMetadata {
  hint: string;
  legend: string;
  options: TimePeriodOptionMetadata[];
}

interface LocationOptionsMetaData {
  hint: string;
  legend: string;
  options: LabelValueMetadata[];
}

interface IndicatorMetadata extends OptionListMetadata<LabelValueUnitMetadata> {
  label: string;
}

export interface ResponseMetaData {
  subject: {
    id: number;
    label: string;
  };
  filters: Dictionary<FilterMetadata>;
  indicators: Dictionary<IndicatorMetadata>;
  timePeriod: TimePeriodMetadata;
  locations: {
    LocalAuthority: LocationOptionsMetaData;
    LocalAuthorityDistrict: LocationOptionsMetaData;
    Regional: LocationOptionsMetaData;
    National: LocationOptionsMetaData;
  };
}

export interface DataBlockGeoJsonProperties {
  // these are what is required
  code: string;
  name: string;
  long: number;
  lat: number;

  // the following are just named here for easier finding in code completion and not required
  objectid: number;
  ctry17cd?: string | null;
  ctry17nm?: string | null;
  lad17cd?: string | null;
  lad17nm?: string | null;
  // allow anything to come through from the API, but very probably ignored
  [name: string]: unknown;
}

export type DataBlockGeoJSON = Feature<Geometry, DataBlockGeoJsonProperties>;

interface DataBlockLocationMetadata {
  code: string;
  label: string;
  geoJson?: DataBlockGeoJSON;
}

// ------------------------------------------

export interface DataBlockMetadata {
  indicators: Dictionary<LabelValueUnitMetadata>;
  filters: Dictionary<LabelValueMetadata>;
  timePeriods: Dictionary<TimePeriod>;
  locations: Dictionary<DataBlockLocationMetadata>;
}

export interface DataBlockRequest {
  subjectId: number;
  geographicLevel: GeographicLevel;
  countries?: string[];
  localAuthorities?: string[];
  localAuthorityDistricts?: string[];
  regions?: string[];
  startYear: string;
  endYear: string;
  filters: string[];
  indicators: string[];
}

export interface DataBlockResponse {
  metaData: DataBlockMetadata;
  data: DataBlockData;
}

function mapOptions<T extends LabelValueMetadata>(ids: string[], options: T[]) {
  return Object.values(options).reduce((results, option) => {
    if (ids.includes(option.value)) {
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
      return {
        ...results,
        [option.year]: new TimePeriod(option.year, option.code),
      };
    }

    return results;
  }, {});
}

function mapOptionsMap<
  R extends LabelValueMetadata,
  T extends OptionListMetadata<R>
>(ids: string[], options: Dictionary<T>): Dictionary<R> {
  return Object.values(options).reduce(
    (mapped, option) => ({ ...mapped, ...mapOptions(ids, option.options) }),
    {},
  );
}

function remapIndicators(
  indicatorIds: string[],
  { indicators }: ResponseMetaData,
): Dictionary<LabelValueUnitMetadata> {
  return mapOptionsMap(indicatorIds, indicators);
}

function remapFilters(
  filterIds: string[],
  { filters }: ResponseMetaData,
): Dictionary<LabelValueMetadata> {
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
): Dictionary<TimePeriod> {
  return mapTimePeriodOptions(years, timePeriod.options);
}

function getUsedTimeIdentifiers(data: DataBlockData) {
  return Array.from(
    data.result.reduce((timeIdentifiers, result) => {
      return timeIdentifiers.add(result.year);
    }, new Set<number>()),
  );
}

function getUsedLocations(
  data: DataBlockData,
): Dictionary<DataBlockLocationMetadata> {
  return data.result.reduce(
    (locations: Dictionary<DataBlockLocationMetadata>, result) => {
      const geoJson = LocationService.getGeoJSONForLocation(result.location);

      if (geoJson) {
        const code = geoJson.properties.lad17cd || geoJson.properties.ctry17cd;
        const label = geoJson.properties.lad17nm || geoJson.properties.ctry17nm;

        if (code && label) {
          const dbm: DataBlockLocationMetadata = { code, label, geoJson };

          return { ...locations, [code]: dbm };
        }
      }

      return locations;
    },
    {},
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

    const usedLocations = getUsedLocations(data);

    const usedMetadata: DataBlockMetadata = {
      indicators: usedIndicators,
      filters: usedFilters,
      timePeriods: usedTimeIdentifiers,
      locations: usedLocations,
    };

    const response: DataBlockResponse = {
      metaData: usedMetadata,
      data,
    };

    return response;
  },
};

export default DataBlockService;
