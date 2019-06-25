/* eslint-disable @typescript-eslint/camelcase */
import { Dictionary } from '@common/types';
import { ChartType as PublicationChartType } from '@common/services/publicationService';
import LineChartBlock from '@common/modules/find-statistics/components/charts/LineChartBlock';
import {ChartDefinition} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import HorizontalBarBlock from '@common/modules/find-statistics/components/charts/HorizontalBarBlock';
import VerticalBarBlock from '@common/modules/find-statistics/components/charts/VerticalBarBlock';
import MapBlock from '@common/modules/find-statistics/components/charts/MapBlock';


const chartTypes: ChartDefinition[] = [
  LineChartBlock.definition,
  HorizontalBarBlock.definition,
  VerticalBarBlock.definition,
  MapBlock.definition
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
