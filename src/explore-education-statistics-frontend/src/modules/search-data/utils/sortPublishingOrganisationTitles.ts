import { defaultOrganisation } from '@common/services/types/organisation';

// TODO EES-7673 - remove fallback for DfE once all releases have an organisation
export default function sortPublishingOrganisationTitles(
  publishingOrganisations?: string[],
): string[] {
  let sortedOrganisations: string[];
  if (!publishingOrganisations || publishingOrganisations?.length === 0) {
    sortedOrganisations = [defaultOrganisation.title];
  } else {
    sortedOrganisations = publishingOrganisations.sort((a, b) => {
      // DfE should always be first
      if (a === defaultOrganisation.title) return -1;
      if (b === defaultOrganisation.title) return 1;

      // Sort remaining alphabetically
      return a.localeCompare(b);
    });
  }
  return sortedOrganisations;
}
