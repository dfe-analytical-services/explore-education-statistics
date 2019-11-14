export interface FootnoteSubjectMeta {
  subjectId: number;
  subjectName: string;
  indicators: {
    [key: number]: {
      // indicator "group"
      label: string;
      options: {
        [key: number]: {
          // indicator "item"
          label: string;
          unit: string;
          value: string;
        };
      };
    };
  };
  filters: {
    [key: number]: {
      // filter
      hint: string;
      legend: string;
      options: {
        [key: number]: {
          label: string;
          // filterGroup
          options: {
            [key: number]: {
              // filterItem
              label: string;
              value: string;
            };
          };
        };
      };
    };
  };
}

export interface FootnoteMeta {
  [key: number /* subjectId */]: FootnoteSubjectMeta;
}

export interface FootnoteMetaMap {
  filterItemsToFilterGroups: { [key: number]: number };
  filterGroupsToFilters: { [key: number]: number };
  filtersToSubject: { [key: number]: number };
  indicatorsToSubject: { [key: number]: number };
}

export interface FootnoteMetaGetters {
  getSubject: (id: number) => { label: string; value: number };
  getIndicatorGroup: (id: number) => { label: string; value: number };
  getIndicator: (id: number) => { label: string; value: number };
  getFilter: (id: number) => { label: string; value: number };
  getFilterGroup: (id: number) => { label: string; value: number };
  getFilterItem: (id: number) => { label: string; value: number };
}

export interface FootnoteProps {
  content: string;
  subjects: {
    [key: number]: {
      selected: boolean;
      indicatorGroups: {
        [key: number]: {
          selected: boolean;
          indicators: number[];
        };
      };
      filters: {
        [key: number]: {
          selected: boolean;
          filterGroups: {
            [key: number]: {
              selected: boolean;
              filterItems: number[];
            };
          };
        };
      };
    };
  };
}

export interface Footnote extends FootnoteProps {
  id: number;
}
