export interface FootnoteMeta {
  [key: number /* subjectId */]: {
    subjectId: number;
    subjectName: string;
    indicators: {
      [key: number]: {
        //indicator props
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
  };
}

export interface FootnoteProps {
  content: string;
  subjects?: number[];
  indicators?: number[];
  filterGroups?: number[];
  filters?: number[];
  filterItems?: number[];
}

export interface Footnote extends FootnoteProps {
  id: number;
}
