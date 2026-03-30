export interface Organisation {
  id: string;
  title: OrganisationTitle;
  url: string;
  useGISLogo: boolean;
  gisLogoHexCode?: string;
  logoFileName: string;
}

export type OrganisationTitle =
  | 'Department for Education'
  | 'Department for Work & Pensions'
  | 'Ofsted'
  | 'Ofqual'
  | 'Skills England';

export const defaultOrganisation: Organisation = {
  id: '5E089801-CF1A-B375-ACD3-88E9D8AECE66',
  title: 'Department for Education',
  url: 'https://www.gov.uk/government/organisations/department-for-education',
  useGISLogo: true,
  logoFileName: 'govuk-crest.svg',
  gisLogoHexCode: '#003764',
};
