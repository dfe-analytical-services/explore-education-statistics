export type TenantKey = 'ees';

export interface TenantRelatedService {
  title: string;
  url: string;
  description: string;
}

export interface TenantOrganisation {
  name: string;
  url: string;
  publicEnquiries?: {
    description: string;
    telephone: string;
    openingHours: string;
  };
}

export interface TenantPressOffice {
  description: string;
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
  serviceName: string;
  organisation: TenantOrganisation;
  contactEmail: string;
  relatedServices?: TenantRelatedService[];
  userResearchBanner?: TenantUserResearchBanner;
  pressOffice?: TenantPressOffice;
  headOfProfession?: TenantHeadOfProfession;
  freedomOfInformationUrl?: string;
  officialStatisticsStandardsUrl?: string;
}
