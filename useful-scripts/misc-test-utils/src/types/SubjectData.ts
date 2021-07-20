export interface SubjectData {
  id: string;
  filename: string;
  name: string;
  content: string;
  timePeriods: { from: string; to: string };
  geographicLevels: string[];
  variables: Record<string, unknown>;
}
