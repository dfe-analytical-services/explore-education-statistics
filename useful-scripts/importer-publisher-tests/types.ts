/* eslint-disable */

export interface SubjectArrayT {
  id: string;
  filename: string;
  name: string;
  content: string;
  timePeriods: { from: string; to: string };
  geographicLevels: string[];
  variables: object[];
}

export interface ReleaseDataProps {
  sub: () => void;
  id: string;
  title: string;
  slug: string;
  publicationId: string;
  publicationTitle: string;
  publicationSlug: string;
  releaseName: string;
  yearTitle: string;
  typeId: string;
  live: boolean;
  timePeriodCoverage: { value: string; label: string };
  latestRelease: boolean;
  type: { id: string; title: string };
  contact: {
    id: string;
    teamName: string;
    teamEmail: string;
    contactName: string;
    contactTelNo: string;
  };
  status: string;
  amendment: boolean;
}
