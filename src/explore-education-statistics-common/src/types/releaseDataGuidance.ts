export interface DataSetDataGuidance {
  fileId: string;
  filename: string;
  name: string;
  content: string;
  timePeriods: {
    from: string;
    to: string;
  };
  geographicLevels: string[];
  variables: {
    label: string;
    value: string;
  }[];
  footnotes: {
    id: string;
    label: string;
  }[];
}
