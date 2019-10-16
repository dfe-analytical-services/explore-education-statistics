/* eslint-disable */
import { SortableOption } from '@common/modules/table-tool/components/FormSortableList';
import {
  FullTable,
  FullTableMeta,
} from '@common/modules/full-table/types/fullTable';
import { TableHeadersConfig } from '@common/modules/full-table/utils/tableHeaders';
import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import {
  DataBlockResponse,
  DataBlockMetadata,
  LabelValueUnitMetadata,
} from '@common/services/dataBlockService';
import mapValuesWithKeys from '@common/lib/utils/mapValuesWithKeys';
import { Dictionary } from '@common/types/util';



export const mapFullTable = (
  unmappedFullTable: DataBlockResponse,
): FullTable => {
  const subjectMeta = unmappedFullTable.metaData || {
    indicators: {},
    locations: {},
    timePeriodRange: {},
  };

  return {
    results: unmappedFullTable.result,
    subjectMeta: {
      subjectName: '',
      publicationName: 'Test',
      footnotes: [],
      filters: {},
      ...unmappedFullTable.metaData,
      indicators: Object.values(subjectMeta.indicators).map(
        indicator => new Indicator(indicator),
      ),
      locations: Object.values(subjectMeta.locations).map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: Object.values(subjectMeta.timePeriods).map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
};
