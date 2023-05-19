export interface TableQuery {
  subjectId: string;
  filters: string[];
  indicators: string[];
  locationIds: string[];
  timePeriod: {
    startYear: number;
    startCode: string;
    endYear: number;
    endCode: string;
  };
}

export interface SubjectMeta {
  filters: {
    [filter: string]: {
      options: {
        [filterGroup: string]: {
          options: {
            value: string;
          }[];
        };
      };
    };
  };
  indicators: {
    [indicatorGroup: string]: {
      options: {
        value: string;
      }[];
    };
  };
  timePeriod: {
    options: {
      code: string;
      year: number;
    }[];
  };
  locations: {
    [geographicLevel: string]: {
      options: {
        id?: string;
        level?: string;
        options?: {
          id: string;
        }[];
      }[];
    };
  };
}

export type OverallStage =
  | 'Validating'
  | 'Complete'
  | 'Failed'
  | 'Invalid'
  | 'Scheduled'
  | 'Started'
  | 'Superseded';

export interface Topic {
  id: string;
  title: string;
  themeId: string;
}

export interface ThemeAndTopics {
  id: string;
  title: string;
  topics: Topic[];
}

export interface Release {
  id: string;
  year: number;
  approvalStatus: string;
}

export interface Publication {
  id: string;
  title: string;
}

export default {};
