import { Dictionary } from '@common/types';

const locationLevelsMap: Dictionary<{
  label: string;
  plural: string;
  prefix: string;
}> = {
  country: {
    label: 'Country',
    plural: 'Countries',
    prefix: 'a',
  },
  englishDevolvedArea: {
    label: 'English Devolved Area',
    plural: 'English Devolved Areas',
    prefix: 'an',
  },
  institution: {
    label: 'Institution',
    plural: 'Institutions',
    prefix: 'an',
  },
  localAuthority: {
    label: 'Local Authority',
    plural: 'Local Authorities',
    prefix: 'a',
  },
  localAuthorityDistrict: {
    label: 'Local Authority District',
    plural: 'Local Authority Districts',
    prefix: 'a',
  },
  localEnterprisePartnership: {
    label: 'Local Enterprise Partnership',
    plural: 'Local Enterprise Partnerships',
    prefix: 'a',
  },
  mayoralCombinedAuthority: {
    label: 'Mayoral Combined Authority',
    plural: 'Mayoral Combined Authorities',
    prefix: 'a',
  },
  multiAcademyTrust: {
    label: 'Multi Academy Trust',
    plural: 'Multi Academy Trusts',
    prefix: 'a',
  },
  opportunityArea: {
    label: 'Opportunity Area',
    plural: 'Opportunity Areas',
    prefix: 'an',
  },
  parliamentaryConstituency: {
    label: 'Parliamentary Constituency',
    plural: 'Parliamentary Constituencies',
    prefix: 'a',
  },
  provider: {
    label: 'Provider',
    plural: 'Providers',
    prefix: 'a',
  },
  region: {
    label: 'Region',
    plural: 'Regions',
    prefix: 'a',
  },
  rscRegion: {
    label: 'RSC Region',
    plural: 'RSC Regions',
    prefix: 'an',
  },
  school: {
    label: 'School',
    plural: 'Schools',
    prefix: 'a',
  },
  sponsor: {
    label: 'Sponsor',
    plural: 'Sponsors',
    prefix: 'a',
  },
  ward: {
    label: 'Ward',
    plural: 'Wards',
    prefix: 'a',
  },
  planningArea: {
    label: 'Planning Area',
    plural: 'Planning Areas',
    prefix: 'a',
  },
};

export default locationLevelsMap;
