export interface Release {
  id: string;
  title: string;
  slug: string;
  publicationId: string;
  publicationTitle: string;
  publicationSummary: string;
  publicationSlug: string;
  year: number;
  yearTitle: string;
  publishScheduled: string;
  live: boolean;
  timePeriodCoverage: {
    value: string;
    label: string;
  };
  preReleaseAccessList: string;
  latestRelease: boolean;
  type: string;
  contact: Contact;
  approvalStatus: 'Approved' | 'Draft' | 'HigherLevelReview';
  notifySubscribers: boolean;
  publishMethod?: 'Immediate' | 'Scheduled';
  latestInternalReleaseNote: string;
  amendment: string;
}

export interface Contact {
  id: string;
  teamName: string;
  teamEmail: string;
  contactName: string;
  contactTelNo: string;
}
