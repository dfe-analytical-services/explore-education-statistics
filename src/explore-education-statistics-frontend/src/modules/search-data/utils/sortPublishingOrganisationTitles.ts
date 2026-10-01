import { DEPARTMENT_FOR_EDUCATION_TITLE } from '@common/services/types/organisation';

export default function sortPublishingOrganisationTitles(
  publishingOrganisations: string[],
): string[] {
  return [...publishingOrganisations].sort((a, b) => {
    // DfE should always be first
    if (a === DEPARTMENT_FOR_EDUCATION_TITLE) return -1;
    if (b === DEPARTMENT_FOR_EDUCATION_TITLE) return 1;

    // Sort remaining alphabetically
    return a.localeCompare(b);
  });
}
