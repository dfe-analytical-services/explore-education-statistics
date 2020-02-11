import { UnmappedFullTable } from '@common/modules/table-tool/services/tableBuilderService';
import {
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '@common/modules/table-tool/types/filters';
import { FullTable } from '@common/modules/table-tool/types/fullTable';

export default function mapFullTable(
  unmappedFullTable: UnmappedFullTable,
): FullTable {
  const subjectMeta = unmappedFullTable.subjectMeta || {
    indicators: [],
    locations: [],
    timePeriodRange: [],
  };

  return {
    ...unmappedFullTable,
    subjectMeta: {
      filters: {},
      ...unmappedFullTable.subjectMeta,
      indicators: subjectMeta.indicators.map(
        indicator => new Indicator(indicator),
      ),
      locations: subjectMeta.locations.map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: subjectMeta.timePeriodRange.map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
}
