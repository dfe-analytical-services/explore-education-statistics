const departmentForEducationTitle = 'Department for Education';

export default function sortPublishingOrganisationTitles(
  publishingOrganisations: string[],
): string[] {
  return [...publishingOrganisations].sort((a, b) => {
    // DfE should always be first
    if (a === departmentForEducationTitle) return -1;
    if (b === departmentForEducationTitle) return 1;

    // Sort remaining alphabetically
    return a.localeCompare(b);
  });
}
