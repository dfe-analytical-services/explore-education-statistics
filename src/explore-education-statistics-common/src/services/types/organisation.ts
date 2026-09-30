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
