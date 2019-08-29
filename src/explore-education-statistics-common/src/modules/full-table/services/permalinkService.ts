import { dataApi } from '@common/services/api';
import { Dictionary } from '@common/types';
import {
  TableData,
  GroupedFilterOptions,
  TableDataQuery,
  FilterOption,
} from '@common/modules/full-table/services/tableBuilderService';
import {
  Indicator,
  LocationFilter,
  CategoryFilter,
  TimePeriodFilter,
} from '@common/modules/full-table/types/filters';
import { transformTableMetaFiltersToCategoryFilters } from '@common/modules/full-table/utils/tableHeaders';

export interface FullTableMeta {
  publicationName: string;
  subjectId: string;
  subjectName: string;
  locations: LocationFilter[];
  timePeriodRange: TimePeriodFilter[];
  filters: Dictionary<{
    legend: string;
    hint?: string;
    options: GroupedFilterOptions;
    totalValue?: string;
  }>;
  indicators: Indicator[];
  footnotes: {
    id: number;
    label: string;
  }[];
}

interface PermalinkCreate extends TableDataQuery {
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

export interface FullTable extends TableData {
  title?: string;
}

export interface Permalink {
  id: string;
  title: string;
  created: string;
  fullTable: FullTable;
  configuration: {
    tableHeadersConfig: UnmappedTableHeadersConfig;
  };
}

interface UnmappedTableHeadersConfig {
  columnGroups: SortableOption[][];
  columns: SortableOption[];
  rowGroups: SortableOption[][];
  rows: SortableOption[];
}

interface SortableOption {
  label: string;
  value: string;
}

const reverseMapTableHeadersConfig = (
  {
    columns,
    rows,
    columnGroups,
    rowGroups,
  }: {
    columns: FilterOption[];
    columnGroups: FilterOption[][];
    rows: FilterOption[];
    rowGroups: FilterOption[][];
  },
  fullTableSubjectMeta: FullTableMeta,
) => {
  /**
   * config filters only contain `values`.
   * This function remaps the config from the filters in the subjectMeta
   */
  let mappedRows = [];
  let mappedColumns = [];

  const mappedRowGroups: (LocationFilter | CategoryFilter | any)[][] = [];
  const mappedColumnGroups: (LocationFilter | CategoryFilter | any)[][] = [];

  const initialValue = columns[0].value;
  // rows/columns can only be TimePeriods / Indicators
  if (Number.isNaN(Number(initialValue))) {
    // if NaN then timePeriod
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      );
    });
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      );
    });
  } else {
    mappedRows = rows.map(({ value }) => {
      return fullTableSubjectMeta.timePeriodRange.find(
        timePeriod => `${timePeriod.year}_${timePeriod.code}` === value,
      );
    });
    mappedColumns = columns.map(({ value }) => {
      return fullTableSubjectMeta.indicators.find(
        indicator => indicator.value === value,
      );
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

    mappedRowGroups[index] = rowGroup.map(({ value }) =>
      locationAndFilterGroups[currentIndex].find(
        element => element.value === value,
      ),
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

    mappedColumnGroups[index] = columnGroup.map(({ value }) =>
      locationAndFilterGroups[currentIndex].find(
        element => element.value === value,
      ),
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

const mapFullTable = (unmappedFullTable: any) => {
  return {
    ...unmappedFullTable,
    subjectMeta: {
      ...unmappedFullTable.subjectMeta,
      indicators: unmappedFullTable.subjectMeta.indicators.map(
        (indicator: { label: string; unit: string; value: string }) =>
          new Indicator(indicator),
      ),
      locations: unmappedFullTable.subjectMeta.locations.map(
        (location: { label: string; level: string; value: string }) =>
          new LocationFilter(location, location.level),
      ),
      timePeriodRange: unmappedFullTable.subjectMeta.timePeriodRange.map(
        (timePeriod: { code: string; label: string; year: number }) =>
          new TimePeriodFilter(timePeriod),
      ),
    },
  };
};

const mapPermalink = (unmappedPermalink: any) => {
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

export default {
  createTablePermalink(query: PermalinkCreate): Promise<Permalink> {
    return dataApi.post('/permalink', query);
  },
  getPermalink(publicationSlug: string): Promise<Permalink> {
    return dataApi.get(`Permalink/${publicationSlug}`);
  },
  mapFullTable,
  mapPermalink,
};
