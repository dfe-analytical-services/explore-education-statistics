import { Dictionary } from '@common/types';

const locationLevelsMap: Dictionary<{
  label: string;
  plural: string;
  prefix: string;
  code: string;
}> = {
  country: {
    label: 'Country',
    plural: 'Countries',
    prefix: 'a',
    code: 'NAT',
  },
  englishDevolvedArea: {
    label: 'English Devolved Area',
    plural: 'English Devolved Areas',
    prefix: 'an',
    code: 'EDA',
  },
  institution: {
    label: 'Institution',
    plural: 'Institutions',
    prefix: 'an',
    code: 'INST',
  },
  localAuthority: {
    label: 'Local Authority',
    plural: 'Local Authorities',
    prefix: 'a',
    code: 'LA',
  },
  localAuthorityDistrict: {
    label: 'Local Authority District',
    plural: 'Local Authority Districts',
    prefix: 'a',
    code: 'LAD',
  },
  localEnterprisePartnership: {
    label: 'Local Enterprise Partnership',
    plural: 'Local Enterprise Partnerships',
    prefix: 'a',
    code: 'LEP',
  },
  localSkillsImprovementPlanArea: {
    label: 'Local Skills Improvement Plan Area',
    plural: 'Local Skills Improvement Plan Areas',
    prefix: 'a',
    code: 'LSIP',
  },
  mayoralCombinedAuthority: {
    label: 'Mayoral Combined Authority',
    plural: 'Mayoral Combined Authorities',
    prefix: 'a',
    code: 'MCA',
  },
  multiAcademyTrust: {
    label: 'Multi Academy Trust',
    plural: 'Multi Academy Trusts',
    prefix: 'a',
    code: 'MAT',
  },
  opportunityArea: {
    label: 'Opportunity Area',
    plural: 'Opportunity Areas',
    prefix: 'an',
    code: 'OA',
  },
  parliamentaryConstituency: {
    label: 'Parliamentary Constituency',
    plural: 'Parliamentary Constituencies',
    prefix: 'a',
    code: 'PCON',
  },
  provider: {
    label: 'Provider',
    plural: 'Providers',
    prefix: 'a',
    code: 'PROV',
  },
  region: {
    label: 'Region',
    plural: 'Regions',
    prefix: 'a',
    code: 'REG',
  },
  rscRegion: {
    label: 'RSC Region',
    plural: 'RSC Regions',
    prefix: 'an',
    code: 'RSC',
  },
  school: {
    label: 'School',
    plural: 'Schools',
    prefix: 'a',
    code: 'SCH',
  },
  sponsor: {
    label: 'Sponsor',
    plural: 'Sponsors',
    prefix: 'a',
    code: 'SPON',
  },
  ward: {
    label: 'Ward',
    plural: 'Wards',
    prefix: 'a',
    code: 'WARD',
  },
  planningArea: {
    label: 'Planning Area',
    plural: 'Planning Areas',
    prefix: 'a',
    code: 'PA',
  },
};

export default locationLevelsMap;
