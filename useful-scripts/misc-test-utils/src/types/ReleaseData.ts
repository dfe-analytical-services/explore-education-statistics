export interface ReleaseData {
  id: string;
  title: string;
  slug: string;
  publicationId: string;
  publicationTitle: string;
  publicationSlug: string;
  releaseName: string;
  yearTitle: string;
  live: boolean;
  timePeriodCoverage: { value: string; label: string };
  latestRelease: boolean;
  type: string;
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
