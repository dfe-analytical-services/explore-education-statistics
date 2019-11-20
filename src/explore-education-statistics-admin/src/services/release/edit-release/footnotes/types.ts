export interface FootnoteSubjectMeta {
  subjectId: string;
  subjectName: string;
  indicators: {
    [key: string]: {
      // indicator "group"
      label: string;
      options: {
        [key: string]: {
          // indicator "item"
          label: string;
          unit: string;
          value: string;
        };
      };
    };
  };
  filters: {
    [key: string]: {
      // filter
      hint: string;
      legend: string;
      options: {
        [key: string]: {
          label: string;
          // filterGroup
          options: {
            [key: string]: {
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
  [key: string /* subjectId */]: FootnoteSubjectMeta;
}

export interface FootnoteMetaMap {
  filterItemsToFilterGroups: { [key: string]: string };
  filterGroupsToFilters: { [key: string]: string };
  filtersToSubject: { [key: string]: string };
  indicatorsToSubject: { [key: string]: string };
}

export interface FootnoteMetaGetters {
  getSubject: (id: string) => { label: string; value: string };
  getIndicatorGroup: (id: string) => { label: string; value: string };
  getIndicator: (id: string) => { label: string; value: string };
  getFilter: (id: string) => { label: string; value: string };
  getFilterGroup: (id: string) => { label: string; value: string };
  getFilterItem: (id: string) => { label: string; value: string };
}

export interface FootnoteProps {
  content: string;
  subjects: {
    [key: string]: {
      selected: boolean;
      indicatorGroups: {
        [key: string]: {
          selected: boolean;
          indicators: string[];
        };
      };
      filters: {
        [key: string]: {
          selected: boolean;
          filterGroups: {
            [key: string]: {
              selected: boolean;
              filterItems: string[];
            };
          };
        };
      };
    };
  };
}

export interface Footnote extends FootnoteProps {
  id: string;
}

export interface FlatFootnoteProps {
  content: string;
  subjects: string[];
  indicatorGroups: string[];
  indicators: string[];
  filters: string[];
  filterGroups: string[];
  filterItems: string[];
}

export interface FlatFootnote extends FlatFootnoteProps {
  id: string;
}
