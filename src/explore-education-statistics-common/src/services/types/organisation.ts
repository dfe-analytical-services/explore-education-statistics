export interface Organisation {
  id: string;
  title: OrganisationTitle;
  url: string;
  useGISLogo: boolean;
  gisLogoHexCode?: string;
  logoFileName: string;
}

export const DEPARTMENT_FOR_EDUCATION_TITLE = 'Department for Education';

export type OrganisationTitle =
  | typeof DEPARTMENT_FOR_EDUCATION_TITLE
  | 'Department for Work & Pensions'
  | 'Ofsted'
  | 'Ofqual'
  | 'Skills England';
