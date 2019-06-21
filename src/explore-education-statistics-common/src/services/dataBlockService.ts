import { dataApi } from '@common/services/api';
import TimePeriod, { TimePeriodCode } from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import { Feature, Geometry } from 'geojson';

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

export interface Country {
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
  country?: Country;
  region?: Region;
  localAuthority?: LocalAuthority;
  localAuthorityDistrict?: LocalAuthorityDistrict;
}

export interface Result {
  filters: Set<string>;
  location: DataBlockLocation;
  measures: {
    [key: string]: string;
  };
  timeIdentifier: TimePeriodCode;
  year: number;
}

export interface DataBlockData {
  publicationId: string;
  releaseId: string;
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

export interface LabelValueUnitMetadata extends LabelValueMetadata {
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
  objectid?: number;
  ctry17cd?: string | null;
  ctry17nm?: string | null;
  lad17cd?: string | null;
  lad17nm?: string | null;

  // allow anything as this is an extension of the GeoJsonProperties object at its heart
  [name: string]: unknown;
}

export type DataBlockGeoJSON = Feature<Geometry, DataBlockGeoJsonProperties>;

export interface DataBlockLocationMetadata {
  value: string;
  label: string;
  geoJson: DataBlockGeoJSON[];
}

// ------------------------------------------

export interface DataBlockMetadata {
  indicators: Dictionary<LabelValueUnitMetadata>;
  filters: Dictionary<LabelValueMetadata>;
  timePeriod?: Dictionary<LabelValueMetadata>;
  timePeriods?: Dictionary<LabelValueMetadata>;
  locations?: Dictionary<DataBlockLocationMetadata>;
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

  publicationId: string;
  releaseId: string;
  subjectId: number;
  releaseDate: Date;
  geographicLevel: GeographicLevel;
  result: Result[];
}

const DataBlockService = {
  buildTimePeriodMetadata(result: Result[]) {
    return result.reduce(
      (results: Dictionary<LabelValueMetadata>, { timeIdentifier, year }) => {
        const key = `${year}_${timeIdentifier}`;
        if (results[key]) return results;

        return {
          ...results,
          [key]: new TimePeriod(year, timeIdentifier),
        };
      },
      {},
    );
  },

  async getDataBlockForSubject(request: DataBlockRequest) {
    const response: DataBlockResponse = await dataApi.post('/Data', request);

    response.metaData.timePeriods =
      response.metaData.timePeriod ||
      DataBlockService.buildTimePeriodMetadata(response.result);

    return response;
  },
};

export default DataBlockService;
