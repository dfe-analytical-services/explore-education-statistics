import { dataApi } from '@common/services/api';
import { Dictionary, PartialRecord } from '@common/types/util';
import { Feature, Geometry } from 'geojson';
import {
  FilterOption,
  IndicatorOption,
  LocationLevelKeys,
  PublicationSubjectMeta,
  TableDataQuery,
  TimePeriodOption,
} from '@common/modules/table-tool/services/tableBuilderService';
import { Table, Chart, Summary } from '@common/services/publicationService';

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
  Instituation = 'institution',
}

export interface Location {
  code: string;
  name: string;
}

type LocalAuthorityDistrict = Location;

export interface LocalAuthority extends Location {
  old_code: string;
}

export interface DataBlockLocation
  extends PartialRecord<LocationLevelKeys, Location> {
  localAuthority?: LocalAuthority;

  // I don't like using any, but it's required here to simplify mapping to the Table Tool, for now
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  [key: string]: any;
}

export interface Result {
  filters: string[];
  location: DataBlockLocation;
  geographicLevel: GeographicLevel;
  measures: Dictionary<string>;
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

export type LabelValueMetadata = FilterOption;
export type LabelValueUnitMetadata = IndicatorOption;
type TimePeriodOptionMetadata = TimePeriodOption;

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
  level: GeographicLevel;
  geoJson: DataBlockGeoJSON[];
  label: string;
  value: string;
}

// ------------------------------------------

export interface BoundaryLevel {
  id: number;
  label: string;
}

export interface FootnoteMetadata {
  id: string;
  label: string;
}

export interface DataBlockMetadata {
  filters: PublicationSubjectMeta['filters'];
  indicators: Dictionary<LabelValueUnitMetadata>;
  locations: Dictionary<DataBlockLocationMetadata>;
  boundaryLevels?: BoundaryLevel[];
  timePeriod: Dictionary<TimePeriodOptionMetadata>;
  publicationName: string;
  subjectName: string;
  footnotes: FootnoteMetadata[];
}

export interface DataBlockRerequest {
  boundaryLevel?: number;
}

export interface DataBlock {
  id?: string;
  order?: number;
  type?: 'DataBlock';

  heading?: string;
  customFootnotes?: string;
  name?: string;
  source?: string;

  dataBlockRequest: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}

export type DataBlockRequest = TableDataQuery;

export interface DataBlockResponse extends DataBlockData {
  metaData: DataBlockMetadata;
}

const DataBlockService = {
  async getDataBlockForSubject(
    request: DataBlockRequest,
  ): Promise<DataBlockResponse | undefined> {
    try {
      return await dataApi.post('/Data', request);
    } catch (_) {
      return undefined;
    }
  },
};

export default DataBlockService;
