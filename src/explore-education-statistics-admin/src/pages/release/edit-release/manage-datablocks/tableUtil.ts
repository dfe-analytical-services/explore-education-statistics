import { FullTable } from '@common/modules/full-table/types/fullTable';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import { DataBlockResponse } from '@common/services/dataBlockService';

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

export default {};
