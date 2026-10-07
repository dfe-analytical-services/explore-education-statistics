import { Organisation } from '@common/services/types/organisation';

export const testOrganisations: Organisation[] = [
  {
    id: 'organisation-id-1',
    title: 'Department for Education',
    url: 'https://www.gov.uk/government/organisations/department-for-education',
    useGISLogo: true,
    logoFileName: 'govuk-crest.svg',
    gisLogoHexCode: '#003764',
  },
  {
    id: 'organisation-id-2',
    title: 'Ofsted',
    url: 'https://www.gov.uk/government/organisations/ofsted',
    useGISLogo: false,
    logoFileName: 'ofsted-logo.svg',
  },
];

export default testOrganisations;
