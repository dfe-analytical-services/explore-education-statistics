import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types/util';
import { Feature, Geometry } from 'geojson';

export enum GeographicLevel {
  Establishment = 'Establishment',
  LocalAuthority = 'Local_Authority',
  LocalAuthorityDistrict = 'Local_Authority_District',
  LocalEnterprisePartnership = 'Local_Enterprise_Partnership',
  MATOrSponsor = 'MAT_Or_Sponsor',
  MayoralCombinedAuthority = 'Mayoral_Combined_Authority',
  MultiAcademyTrust = 'Multi_Academy_Trust',
  Country = 'Country',
  OpportunityArea = 'OpportunityArea',
  ParliamentaryConstituency = 'Parliamentary_Constituency',
  Provider = 'Provider',
  Region = 'Region',
  RSCRegion = 'RSC_Region',
  School = 'School',
  Ward = 'Ward',
}

type TimeIdentifier =
  | 'AY'
  | 'AYQ1'
  | 'AYQ1Q2'
  | 'AYQ1Q3'
  | 'AYQ1Q4'
  | 'AYQ2'
  | 'AYQ2Q3'
  | 'AYQ2Q4'
  | 'AYQ3'
  | 'AYQ3Q4'
  | 'AYQ4'
  | 'CY'
  | 'CYQ1'
  | 'CYQ1Q2'
  | 'CYQ1Q3'
  | 'CYQ1Q4'
  | 'CYQ2'
  | 'CYQ2Q3'
  | 'CYQ2Q4'
  | 'CYQ3'
  | 'CYQ3Q4'
  | 'CYQ4'
  | 'FY'
  | 'FYQ1'
  | 'FYQ1Q2'
  | 'FYQ1Q3'
  | 'FYQ1Q4'
  | 'FYQ2'
  | 'FYQ2Q3'
  | 'FYQ2Q4'
  | 'FYQ3'
  | 'FYQ3Q4'
  | 'FYQ4'
  | 'TY'
  | 'TYQ1'
  | 'TYQ1Q2'
  | 'TYQ1Q3'
  | 'TYQ1Q4'
  | 'TYQ2'
  | 'TYQ2Q3'
  | 'TYQ2Q4'
  | 'TYQ3'
  | 'TYQ3Q4'
  | 'TYQ4'
  | 'HT5'
  | 'HT6'
  | 'EOM'
  | 'T1'
  | 'T1T2'
  | 'T2'
  | 'T3'
  | 'M1'
  | 'M2'
  | 'M3'
  | 'M4'
  | 'M5'
  | 'M6'
  | 'M7'
  | 'M8'
  | 'M9'
  | 'M10'
  | 'M11'
  | 'M12';

export interface Location {
  code: string;
  name: string;
}

export type Country = Location;
export type Region = Location;

export type LocalAuthorityDistrict = Location;

export interface LocalAuthority extends Location {
  old_code: string;
}

export interface DataBlockLocation {
  country?: Country;
  region?: Region;
  localAuthority?: LocalAuthority;
  localAuthorityDistrict?: LocalAuthorityDistrict;
}

export interface Result {
  filters: string[];
  location: DataBlockLocation;
  measures: {
    [key: string]: string;
  };
  timePeriod: string;
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
  code: TimeIdentifier;
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

export interface BoundaryLevel {
  id: number;
  label: string;
}
export interface DataBlockMetadata {
  indicators: Dictionary<LabelValueUnitMetadata>;
  filters: Dictionary<LabelValueMetadata>;
  timePeriods: Dictionary<LabelValueMetadata>;
  locations: Dictionary<DataBlockLocationMetadata>;
  boundaryLevels?: BoundaryLevel[];
}

interface DataBlockTimePeriod {
  startYear: string;
  startCode: TimeIdentifier;
  endYear: string;
  endCode: TimeIdentifier;
}

export interface DataBlockRequest {
  subjectId: number;
  timePeriod: DataBlockTimePeriod;
  filters: string[];
  geographicLevel: GeographicLevel;
  boundaryLevel?: number;
  indicators: string[];

  country?: string[];
  localAuthority?: string[];
  localAuthorityDistrict?: string[];
  localEnterprisePartnership?: string[];
  multiAcademyTrust?: string[];
  mayoralCombinedAuthority?: string[];
  opportunityArea?: string[];
  parliamentaryConstituency?: string[];
  region?: string[];
  rscRegion?: string[];
  sponsor?: string[];
  ward?: string[];
}

export interface DataBlockRerequest {
  boundaryLevel?: number;
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
  async getDataBlockForSubject(
    request: DataBlockRequest,
  ): Promise<DataBlockResponse> {
    const response: DataBlockResponse = await dataApi.post('/Data', request);

    return response;
  },
};

export default DataBlockService;
