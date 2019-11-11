export interface FootnoteSubjectMeta {
  subjectId: number;
  subjectName: string;
  indicators: {
    [key: number]: {
      //indicator "group"
      label: string;
      options: {
        [key: number]: {
          //indicator "item"
          label: string;
          unit: string;
          value: string;
        };
      };
    };
  };
  filters: {
    [key: number]: {
      //filter
      hint: string;
      legend: string;
      options: {
        [key: number]: {
          label: string;
          //filterGroup
          options: {
            [key: number]: {
              //filterItem
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
  getFilterItem: (id: number) => { label: string; value: number };
  getFilterGroup: (id: number) => { label: string; value: number };
  getFilter: (id: number) => { label: string; value: number };
  getIndicator: (id: number) => { label: string; value: number };
  getSubject: (id: number) => { label: string; value: number };
}

export interface FootnoteProps {
  content: string;
  subjects: number[];
  indicators: number[];
  filterGroups: number[];
  filters: number[];
  filterItems: number[];
}

export interface Footnote extends FootnoteProps {
  id: number;
}
