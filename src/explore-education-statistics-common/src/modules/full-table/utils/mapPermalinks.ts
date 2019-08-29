import {
  CategoryFilter,
  Indicator,
  LocationFilter,
  TimePeriodFilter,
} from '../types/filters';
import {
  UnmappedTableHeadersConfig,
  UnmappedFullTable,
  UnmappedPermalink,
  Permalink,
} from '../services/permalinkService';
import {
  transformTableMetaFiltersToCategoryFilters,
  TableHeadersConfig,
} from './tableHeaders';
import { FullTableMeta, FullTable } from '../types/fullTable';

const reverseMapTableHeadersConfig = (
  { columns, rows, columnGroups, rowGroups }: UnmappedTableHeadersConfig,
  fullTableSubjectMeta: FullTableMeta,
): TableHeadersConfig => {
  /**
   * config filters only contain `values`.
   * This function remaps the config from the filters in the subjectMeta
   */
  let mappedRows: (TimePeriodFilter | Indicator)[] = [];
  let mappedColumns: (TimePeriodFilter | Indicator)[] = [];

  const mappedRowGroups: (LocationFilter | CategoryFilter)[][] = [];
  const mappedColumnGroups: (LocationFilter | CategoryFilter)[][] = [];

  const initialValue = columns[0].value;
  // rows/columns can only be TimePeriods / Indicators
  if (Number.isNaN(Number(initialValue))) {
    // if NaN then timePeriod
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      ) as TimePeriodFilter;
    });
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      ) as Indicator;
    });
  } else {
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      ) as TimePeriodFilter;
    });
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      ) as Indicator;
    });
  }

  // rowGroups/columnGroups can only be filters and locations
  const locationAndFilterGroups: (LocationFilter | CategoryFilter)[][] = [
    ...Object.values(
      transformTableMetaFiltersToCategoryFilters(fullTableSubjectMeta.filters),
    ),
    fullTableSubjectMeta.locations,
  ];

  rowGroups.forEach((rowGroup, index) => {
    let currentIndex = 0;
    for (let i = 0; i < locationAndFilterGroups.length; i += 1) {
      if (
        locationAndFilterGroups[i].find(
          element => element.value === rowGroup[0].value,
        )
      ) {
        currentIndex = i;
        break;
      }
    }

    mappedRowGroups[index] = rowGroup.map(
      ({ value }) =>
        locationAndFilterGroups[currentIndex].find(
          element => element.value === value,
        ) as LocationFilter | CategoryFilter,
    );

    locationAndFilterGroups.splice(currentIndex, 1);
  });

  columnGroups.forEach((columnGroup, index) => {
    let currentIndex = 0;
    for (let i = 0; i < locationAndFilterGroups.length; i += 1) {
      if (
        locationAndFilterGroups[i].find(
          element => element.value === columnGroup[0].value,
        )
      ) {
        currentIndex = i;
        break;
      }
    }

    mappedColumnGroups[index] = columnGroup.map(
      ({ value }) =>
        locationAndFilterGroups[currentIndex].find(
          element => element.value === value,
        ) as LocationFilter | CategoryFilter,
    );
    locationAndFilterGroups.splice(currentIndex, 1);
  });

  return {
    columns: mappedColumns,
    columnGroups: mappedColumnGroups,
    rows: mappedRows,
    rowGroups: mappedRowGroups,
  };
};

export const mapFullTable = (
  unmappedFullTable: UnmappedFullTable,
): FullTable => {
  return {
    ...unmappedFullTable,
    subjectMeta: {
      ...unmappedFullTable.subjectMeta,
      indicators: unmappedFullTable.subjectMeta.indicators.map(
        indicator => new Indicator(indicator),
      ),
      locations: unmappedFullTable.subjectMeta.locations.map(
        location => new LocationFilter(location, location.level),
      ),
      timePeriodRange: unmappedFullTable.subjectMeta.timePeriodRange.map(
        timePeriod => new TimePeriodFilter(timePeriod),
      ),
    },
  };
};

export const mapPermalink = (
  unmappedPermalink: UnmappedPermalink,
): Permalink => {
  const mappedFullTable = mapFullTable(unmappedPermalink.fullTable);

  return {
    ...unmappedPermalink,
    fullTable: mappedFullTable,
    configuration: {
      tableHeadersConfig: reverseMapTableHeadersConfig(
        unmappedPermalink.configuration.tableHeadersConfig,
        mappedFullTable.subjectMeta,
      ),
    },
  };
};
