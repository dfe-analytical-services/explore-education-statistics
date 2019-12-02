import { AxesConfiguration } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { TableHeadersFormValues } from '@common/modules/table-tool/components/TableHeadersForm';
import {
  DataBlockLocation,
  DataBlockRequest,
} from '@common/services/dataBlockService';
import { Dictionary } from '@common/types';
import { PositionType } from 'recharts';
import { contentApi } from './api';

export type ReleaseStatus = 'Draft' | 'HigherLevelReview' | 'Approved';

export interface Publication {
  id: string;
  slug: string;
  title: string;
  description: string;
  dataSource: string;
  summary: string;
  nextUpdate: string;
  releases: {
    id: string;
    releaseName: string;
    slug: string;
    title: string;
  }[];
  legacyReleases: {
    id: string;
    description: string;
    url: string;
  }[];
  topic: {
    theme: {
      title: string;
    };
  };
  contact: PublicationContact;
  methodology: {
    id: string;
    title: string;
  };
}

export interface PublicationContact {
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}

export interface DataQuery {
  method: string;
  path: string;
  body: string;
}

export interface ReferenceLine {
  label: string;
  position: number | string;
}

export interface Axis {
  title: string;
  key?: string[];
  min?: number;
  max?: number;
  size?: number;
}

export type ChartType =
  | 'line'
  | 'verticalbar'
  | 'horizontalbar'
  | 'map'
  | 'infographic';

export interface ChartDataSet {
  indicator: string;
  filters: string[];
  location?: DataBlockLocation;
  timePeriod?: string;
}

export interface OptionalChartDataSet {
  indicator?: string;
  filters?: string[];
  location?: string[];
  timePeriod?: string;
}

export type ChartSymbol =
  | 'circle'
  | 'cross'
  | 'diamond'
  | 'square'
  | 'star'
  | 'triangle'
  | 'wye';

export type LineStyle = 'solid' | 'dashed' | 'dotted';

export interface LabelConfiguration {
  label: string;
}

export interface DataSetConfiguration extends LabelConfiguration {
  value: string;
  name?: string;
  unit?: string;
  colour?: string;
  symbol?: ChartSymbol;
  lineStyle?: LineStyle;
}

export type AxisGroupBy = 'timePeriod' | 'locations' | 'filters' | 'indicators';

export type AxisType = 'major' | 'minor';

export type LabelPosition = 'axis' | 'graph' | PositionType;

export interface AxisConfiguration {
  name: string;
  type: AxisType;
  groupBy?: AxisGroupBy;
  sortBy?: string;
  sortAsc?: boolean;
  dataSets: ChartDataSet[];
  dataRange?: [number | undefined, number | undefined];

  referenceLines?: ReferenceLine[];

  visible?: boolean;
  title?: string;
  unit?: string;
  showGrid?: boolean;
  labelPosition?: LabelPosition;
  size?: string;

  min?: string;
  max?: string;

  tickConfig?: 'default' | 'startEnd' | 'custom';
  tickSpacing?: string;
}

export interface Chart {
  type?: ChartType;

  title?: string;
  height?: number;
  width?: number;

  labels?: Dictionary<DataSetConfiguration>;
  axes?: AxesConfiguration;

  legend?: 'none' | 'top' | 'bottom';
  legendHeight?: string;

  stacked?: boolean;
  fileId?: string;
  geographicId?: string;
}

export interface Table {
  indicators: string[];
  tableHeaders: TableHeadersFormValues;
}

export interface Summary {
  dataKeys: string[];
  dataSummary: string[];
  dataDefinition: string[];
  description: { type: string; body: string };
}

export type ContentBlockType =
  | 'MarkDownBlock'
  | 'InsetTextBlock'
  | 'DataBlock'
  | 'HtmlBlock';

export interface ContentBlock {
  type: ContentBlockType;
  body: string;
  heading?: string;
  dataBlockRequest?: DataBlockRequest;
  charts?: Chart[];
  tables?: Table[];
  summary?: Summary;
}

export interface BasicLink {
  id: string;
  description: string;
  url: string;
}

export interface ReleaseNote {
  id: string;
  releaseId: string;
  on: string;
  reason: string;
}

export enum ReleaseType {
  AdHoc = 'Ad Hoc',
  NationalStatistics = 'National Statistics',
  OfficialStatistics = 'Official Statistics',
}

export interface ContentSection<ContentBlockType> {
  id?: string;
  order: number;
  heading: string;
  caption: string;
  content: ContentBlockType[];
}

export interface AbstractRelease<
  ContentBlockType,
  PublicationType = Publication
> {
  id: string;
  title: string;
  yearTitle: string;
  coverageTitle: string;
  releaseName: string;
  published: string;
  slug: string;
  summarySection: ContentSection<ContentBlockType>;
  headlinesSection: ContentSection<ContentBlockType>;
  keyStatisticsSection: ContentSection<ContentBlockType>;
  publicationId: string;
  publication: PublicationType;
  latestRelease: boolean;
  publishScheduled?: string;
  nextReleaseDate: DayMonthYearValues;
  status: ReleaseStatus;
  relatedInformation: BasicLink[];
  type: {
    id: string;
    title: ReleaseType;
  };
  updates: ReleaseNote[];
  content: ContentSection<ContentBlockType>[];
  dataFiles?: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
  downloadFiles: {
    extension: string;
    name: string;
    path: string;
    size: string;
  }[];
}

export interface DayMonthYearValues {
  day?: number;
  month?: number;
  year?: number;
}

export interface DayMonthYearInputs {
  day: string;
  month: string;
  year: string;
}

export const dayMonthYearValuesToInputs = (
  dmy?: DayMonthYearValues,
): DayMonthYearInputs => ({
  day: dmy && dmy.day ? dmy.day.toString() : '',
  month: dmy && dmy.month ? dmy.month.toString() : '',
  year: dmy && dmy.year ? dmy.year.toString() : '',
});

export const dayMonthYearInputsToValues = (
  dmy: DayMonthYearInputs,
): DayMonthYearValues => ({
  day: dmy.day ? parseInt(dmy.day, 10) : undefined,
  month: dmy.month ? parseInt(dmy.month, 10) : undefined,
  year: dmy.year ? parseInt(dmy.year, 10) : undefined,
});

export const dateToDayMonthYear = (date?: Date) => {
  return {
    day: date && date.getDate(),
    month: date && date.getMonth() + 1,
    year: date && date.getFullYear(),
  };
};

export const emptyDayMonthYear = (): DayMonthYearInputs => ({
  day: '',
  month: '',
  year: '',
});

export const dayMonthYearIsComplete = (dmy?: DayMonthYearValues) => {
  return dmy && dmy.day && dmy.month && dmy.year;
};

export const dayMonthYearIsEmpty = (dmy?: DayMonthYearValues) => {
  return !dmy || (!dmy.day && !dmy.month && !dmy.year);
};

export const dayMonthYearToDate = (dmy?: DayMonthYearValues) => {
  if (!dmy) {
    throw Error(`Couldn't convert undefined DayMonthYearValues to Date`);
  }
  if (!dayMonthYearIsComplete(dmy)) {
    throw Error(
      `Couldn't convert DayMonthYearValues ${JSON.stringify(
        dmy,
      )} to Date - missing required value`,
    );
  }
  return new Date(Date.UTC(dmy.year || 0, (dmy.month || 0) - 1, dmy.day));
};

export const dayMonthYearInputsToDate = (dmy: DayMonthYearInputs): Date =>
  dayMonthYearToDate(dayMonthYearInputsToValues(dmy));

export type Release = AbstractRelease<ContentBlock>;

export default {
  getPublication(publicationSlug: string): Promise<Release> {
    return contentApi.get(`content/publication/${publicationSlug}`);
  },
  getLatestPublicationRelease(publicationSlug: string): Promise<Release> {
    return contentApi.get(`content/publication/${publicationSlug}/latest`);
  },
  getPublicationRelease(
    publicationSlug: string,
    releaseSlug: string,
  ): Promise<Release> {
    return contentApi.get(
      `content/publication/${publicationSlug}/${releaseSlug}`,
    );
  },
};
