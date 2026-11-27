import { TenantOrganisation } from '@common/tenants/types';

/**
 * The organisation's possessive form for use mid-sentence, e.g. "DfE's", or
 * "the Test Organisation's" when it has no acronym.
 */
export default function formatOrganisationPossessive(
  organisation: Pick<TenantOrganisation, 'acronym' | 'nameWithArticle'>,
): string {
  // `||` rather than `??`, so a blank acronym is treated as absent.
  return `${organisation.acronym || organisation.nameWithArticle}'s`;
}
