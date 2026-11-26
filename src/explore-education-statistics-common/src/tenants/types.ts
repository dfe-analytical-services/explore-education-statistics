export type TenantKey = 'ees';

export interface TenantService {
  /** Used standalone, or at the start of a sentence/heading, e.g. "Explore education statistics". */
  nameStartSentence: string;
  /** Used mid-sentence, e.g. "...use explore education statistics to find...". */
  nameMidSentence: string;
}

export interface TenantRelatedService {
  title: string;
  url: string;
  description: string;
}

export interface TenantOrganisation {
  name: string;
  /** Optional short acronym, e.g. "DfE". Rendered as "(acronym)" only when supplied. */
  acronym?: string;
  url: string;
  publicEnquiries?: {
    /**
     * The full sentence introducing the contact details below, e.g.
     * "If you have a general enquiry about the Department for Education or
     * education:". Kept as one sentence, rather than built from
     * `organisation.name` plus static wording, because what counts as a
     * general enquiry (e.g. "or education") is tenant-specific, not a fact
     * about the organisation itself.
     */
    description: string;
    /** The line directly introducing the phone number, e.g. "Call the DfE public enquiries line:". */
    telephoneCallToAction: string;
    telephone: string;
    openingHours: string;
  };
}

export interface TenantPressOffice {
  telephone: string;
}

export interface TenantHeadOfProfession {
  title: string;
  email: string;
}

export interface TenantUserResearchBanner {
  title: string;
  linkText: string;
  url: string;
}

/**
 * Public-facing brand content for a tenant.
 *
 * Only fields that are safe to publish belong here. Any future Admin-only
 * fields go in the Admin package, never in common.
 */
export interface Tenant {
  key: TenantKey;
  service: TenantService;
  organisation: TenantOrganisation;
  contactEmail: string;
  relatedServices?: TenantRelatedService[];
  userResearchBanner?: TenantUserResearchBanner;
  pressOffice?: TenantPressOffice;
  headOfProfession?: TenantHeadOfProfession;
  freedomOfInformationUrl?: string;
  officialStatisticsStandardsUrl?: string;
}
