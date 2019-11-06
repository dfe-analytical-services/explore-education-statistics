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
        //filter props
        options: {
          [key: number]: {
            //filterGroup props
            options: {
              [key: number]: {
                //filterItem props
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
