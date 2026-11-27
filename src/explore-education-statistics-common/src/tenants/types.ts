export type TenantKey = 'ees';

/**
 * Service names leave out the word "service", which templates add where
 * needed, e.g. "The explore education statistics service is operated by...".
 */
export interface TenantService {
  /** Used standalone, or at the start of a sentence/heading, e.g. "Explore education statistics". */
  nameStartSentence: string;
  /** Used mid-sentence, e.g. "...use explore education statistics to find...". */
  nameMidSentence: string;
  /** The subject the service's statistics cover, used mid-sentence, e.g. "education" in "a repository for education statistics". */
  subjectMidSentence: string;
  /** The production site's address, with no trailing slash, used in dataset metadata (e.g. CSVW/schema.org). */
  productionUrl: string;
}

export interface TenantRelatedService {
  title: string;
  url: string;
  description: string;
}

export interface TenantOrganisation {
  /** Bare name, for labels, modifiers and metadata, e.g. "official Department for Education statistics". */
  name: string;
  /**
   * Name preceded by "the" where the organisation takes one, e.g. "operated by
   * the Department for Education". Same as `name` for an organisation that
   * doesn't. Use `upperFirst()` at the start of a sentence.
   */
  nameWithArticle: string;
  /** Optional short acronym, e.g. "DfE". Rendered as "(acronym)" only when supplied; a blank acronym is treated as absent. */
  acronym?: string;
  url: string;
  /** A short description of the organisation's remit, used in dataset metadata (e.g. CSVW/schema.org). */
  description: string;
  /** Optional Research Organization Registry (ROR) record, used as `sameAs` in dataset metadata, e.g. "https://ror.org/0320bge18". */
  rorUrl?: string;
  /**
   * Kept as whole sentences, rather than built from `organisation.name` plus
   * static wording, because what counts as a general enquiry (e.g. "or
   * education") is tenant-specific, not a fact about the organisation itself.
   */
  publicEnquiries?: {
    /**
     * The question shown under the "Have a general enquiry?" heading on the
     * home, contact and help pages, e.g. "Have a general question about
     * education or the Department for Education?".
     */
    question: string;
    /**
     * The sentence introducing the phone number in a release's contact
     * section, e.g. "If you have a general enquiry about the Department for
     * Education (DfE) or education:".
     */
    description: string;
    /**
     * The line introducing the phone number under `question` on the home,
     * contact and help pages, e.g. "Call the DfE public enquiries line:".
     */
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
