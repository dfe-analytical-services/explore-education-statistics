import { Dictionary } from '@common/types';
import { mapKeys } from 'lodash';

export type GeographicLevelCode =
  | 'EDA'
  | 'INST'
  | 'LA'
  | 'LAD'
  | 'LEP'
  | 'LSIP'
  | 'MAT'
  | 'MCA'
  | 'NAT'
  | 'OA'
  | 'PCON'
  | 'PROV'
  | 'REG'
  | 'RSC'
  | 'SCH'
  | 'SPON'
  | 'PA'
  | 'WARD'
  | 'PFA';

export interface LocationLevelDetails {
  filterLabel: string; // used in the dropdown that filters data sets by geographic level
  label: string;
  plural: string;
  prefix: string;
  code: GeographicLevelCode;
}

const locationLevelsMap = {
  country: {
    filterLabel: 'National',
    label: 'Country',
    plural: 'Countries',
    prefix: 'a',
    code: 'NAT',
  },
  englishDevolvedArea: {
    filterLabel: 'English Devolved Area',
    label: 'English Devolved Area',
    plural: 'English Devolved Areas',
    prefix: 'an',
    code: 'EDA',
  },
  institution: {
    filterLabel: 'Institution',
    label: 'Institution',
    plural: 'Institutions',
    prefix: 'an',
    code: 'INST',
  },
  localAuthority: {
    filterLabel: 'Local Authority',
    label: 'Local Authority',
    plural: 'Local Authorities',
    prefix: 'a',
    code: 'LA',
  },
  localAuthorityDistrict: {
    filterLabel: 'Local Authority District',
    label: 'Local Authority District',
    plural: 'Local Authority Districts',
    prefix: 'a',
    code: 'LAD',
  },
  localEnterprisePartnership: {
    filterLabel: 'Local Enterprise Partnership',
    label: 'Local Enterprise Partnership',
    plural: 'Local Enterprise Partnerships',
    prefix: 'a',
    code: 'LEP',
  },
  localSkillsImprovementPlanArea: {
    filterLabel: 'Local Skills Improvement Plan Area',
    label: 'Local Skills Improvement Plan Area',
    plural: 'Local Skills Improvement Plan Areas',
    prefix: 'a',
    code: 'LSIP',
  },
  policeForceArea: {
    filterLabel: 'Police Force Area',
    label: 'Police Force Area',
    plural: 'Police Force Areas',
    prefix: 'a',
    code: 'PFA',
  },
  mayoralCombinedAuthority: {
    filterLabel: 'Mayoral Combined Authority',
    label: 'Mayoral Combined Authority',
    plural: 'Mayoral Combined Authorities',
    prefix: 'a',
    code: 'MCA',
  },
  multiAcademyTrust: {
    filterLabel: 'Multi Academy Trust',
    label: 'Multi Academy Trust',
    plural: 'Multi Academy Trusts',
    prefix: 'a',
    code: 'MAT',
  },
  opportunityArea: {
    filterLabel: 'Opportunity Area',
    label: 'Opportunity Area',
    plural: 'Opportunity Areas',
    prefix: 'an',
    code: 'OA',
  },
  parliamentaryConstituency: {
    filterLabel: 'Parliamentary Constituency',
    label: 'Parliamentary Constituency',
    plural: 'Parliamentary Constituencies',
    prefix: 'a',
    code: 'PCON',
  },
  planningArea: {
    filterLabel: 'Planning Area',
    label: 'Planning Area',
    plural: 'Planning Areas',
    prefix: 'a',
    code: 'PA',
  },
  provider: {
    filterLabel: 'Provider',
    label: 'Provider',
    plural: 'Providers',
    prefix: 'a',
    code: 'PROV',
  },
  region: {
    filterLabel: 'Region',
    label: 'Region',
    plural: 'Regions',
    prefix: 'a',
    code: 'REG',
  },
  rscRegion: {
    filterLabel: 'RSC Region',
    label: 'RSC Region',
    plural: 'RSC Regions',
    prefix: 'an',
    code: 'RSC',
  },
  school: {
    filterLabel: 'School',
    label: 'School',
    plural: 'Schools',
    prefix: 'a',
    code: 'SCH',
  },
  sponsor: {
    filterLabel: 'Sponsor',
    label: 'Sponsor',
    plural: 'Sponsors',
    prefix: 'a',
    code: 'SPON',
  },
  ward: {
    filterLabel: 'Ward',
    label: 'Ward',
    plural: 'Wards',
    prefix: 'a',
    code: 'WARD',
  },
} as const satisfies Dictionary<LocationLevelDetails>;

export default locationLevelsMap;

export type LocationLevelKey = keyof typeof locationLevelsMap;

export const geographicLevelCodesMap = mapKeys(
  locationLevelsMap,
  level => level.code,
) as Record<GeographicLevelCode, LocationLevelDetails>;
