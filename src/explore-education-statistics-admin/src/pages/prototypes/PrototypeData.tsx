/* eslint-disable @typescript-eslint/camelcase */
import { Dictionary } from '@common/types';
import DataBlockService, {
  DataBlockResponse,
  GeographicLevel,
  DataBlockLocation,
  Result,
  Country,
  LabelValueUnitMetadata,
} from '@common/services/dataBlockService';

import { ChartType as PublicationChartType } from '@common/services/publicationService';

export interface ChartType {
  type: PublicationChartType;
  title: string;

  dataOptions: string[];
  dataMapping: {
    mapTo: string;
  }[];
  hasLegend: boolean;
}

const chartTypes: ChartType[] = [
  {
    type: 'line',
    title: 'Line Chart',
    dataOptions: ['X Axis'],
    dataMapping: [{ mapTo: 'xaxis' }],
    hasLegend: true,
  },
  {
    type: 'horizontalbar',
    title: 'Horizontal bar',
    dataOptions: ['Y Axis', 'Group fields'],
    dataMapping: [{ mapTo: 'yaxis' }],
    hasLegend: true,
  },
  {
    type: 'verticalbar',
    title: 'Vertical bar',
    dataOptions: ['X Axis', 'Group fields'],
    dataMapping: [{ mapTo: 'xaxis' }],
    hasLegend: true,
  },
  {
    type: 'map',
    title: 'Map',
    dataOptions: [],
    dataMapping: [{ mapTo: 'geojson' }],
    hasLegend: false,
  },
];

export interface PrototypeTable {
  label: string;
  value: string;

  title?: string;
}

const tables: Dictionary<PrototypeTable> = {
  absence_highlights_panel: {
    value: 'absence_highlights_panel',
    label: 'Table for absence highlights panel',

    title:
      "Table showing 'Absence by characteristic' from 'Pupil absence' in England between 2012/13 and 2016/17",
  },
  example_table_2: {
    value: 'example_table_2',
    label: 'Example Table 2',

    title:
      "Table showing 'Example 2' from 'Example table 2' in England between 2012/13 and 2016/17",
  },
  example_table_3: {
    value: 'example_table_3',
    label: 'Example Table 3',

    title:
      "Table showing 'Example 3' from 'Example table 3' in England between 2012/13 and 2016/17",
  },
  example_table_4: {
    value: 'example_table_4',
    label: 'Example Table 4',

    title:
      "Table showing 'Example 4' from 'Example table 4' in England between 2012/13 and 2016/17",
  },
};

export default {
  chartTypes,
  tables,
};
