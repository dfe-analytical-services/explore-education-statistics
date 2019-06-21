import {
  ApprovalStatus,
  Methodology,
  Publication,
  Release,
  ReleaseDataType,
  Theme,
  TimePeriod,
  Topic,
} from '@admin/services/publicationService';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';

const methodologies: Methodology[] = [
  {
    id: 'methodology-1',
    title: 'A guide to absence statistics',
  },
];

const themes: Theme[] = [
  {
    id: 'theme-1',
    title: 'Pupils and schools',
  },
];

const topics: Topic[] = [
  {
    id: 'topic-1',
    title: 'pupil absence',
    theme: themes[0],
  },
];

const timePeriodTermAutumn: TimePeriod = {
  id: 'term-autumn',
  title: 'Autumn term',
};

const timePeriodTermFullAcademicYear: TimePeriod = {
  id: 'term-full-academic-year',
  title: 'Full academic year',
};

const dataTypeRevised: ReleaseDataType = {
  id: 'data-type-revised',
  title: 'Revised',
};

const releaseTemplate: Release = {
  id: 'release-1',
  releaseName: '2017 to 2018',
  slug: '2017-2018',
  timePeriodCoverage: {
    label: 'Academic year',
    academicYear: {
      yearStarting: 2017,
      timePeriod: timePeriodTermFullAcademicYear,
      termsPerYear: 6,
    },
  },
  status: {
    approvalStatus: ApprovalStatus.Approved,
    isLive: true,
    isLatest: false,
    isNew: false,
    lastEdited: new Date('2019-03-20 17:37'),
    lastEditor: PrototypeLoginService.getUser('user1'),
    published: new Date('2019-09-20 09:30'),
    nextRelease: new Date('2020-09-20 09:30'),
  },
  meta: {
    editing: false,
    dataType: dataTypeRevised,
    review: true,
    lead: {
      contactName: 'John Smith',
      contactTelNo: '07654 653763',
      teamName: '',
      teamEmail: 'js@example.com',
    },
    showComments: true,
  },
};

const publicationTemplate: Publication = {
  id: 'publication-1',
  slug: 'pupil-absence-statistics-and-data-for-schools-in-england',
  title: 'Pupil absence statistics and data for schools in England',
  description: '',
  dataSource: '',
  summary: '',
  nextUpdate: '',
  releases: [],
  legacyReleases: [],
  topic: topics[0],
  contact: {
    teamName: '',
    teamEmail: '',
    contactName: '',
    contactTelNo: '',
  },
  methodology: methodologies[0],
};

//
// PUBLICATION 1
//

const publication1ReleaseTemplate = {
  ...releaseTemplate,
};

const publication1Releases: Release[] = [
  {
    ...publication1ReleaseTemplate,
    status: {
      ...publication1ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...publication1ReleaseTemplate,
    releaseName: '2016 to 2017',
  },
  {
    ...publication1ReleaseTemplate,
    releaseName: '2015 to 2016',
  },
];

const publication1 = {
  ...publicationTemplate,
  releases: publication1Releases,
};

//
// PUBLICATION 2
//

const publication2ReleaseTemplate = {
  ...releaseTemplate,
  timePeriodCoverage: {
    label: 'Autumn term, academic year',
    academicYear: {
      yearStarting: 2017,
      timePeriod: timePeriodTermAutumn,
      termsPerYear: 6,
    },
  },
};

const publication2Releases: Release[] = [
  {
    ...publication2ReleaseTemplate,
    status: {
      ...publication2ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...publication2ReleaseTemplate,
    releaseName: '2016 to 2017',
  },
];

const publication2 = {
  ...publicationTemplate,
  id: 'publication-2',
  slug: 'pupil-absence-statistics-and-data-for-schools-in-england-autumn-term',
  title:
    'Pupil absence statistics and data for schools in England: autumn term',
  releases: publication2Releases,
};

//
// PUBLICATION 3
//

const publication3ReleaseTemplate = {
  ...releaseTemplate,
  timePeriodCoverage: {
    label: 'Autumn and spring terms, academic year',
    academicYear: {
      yearStarting: 2017,
      timePeriod: timePeriodTermAutumn,
      termsPerYear: 6,
    },
  },
};

const publication3Releases: Release[] = [
  {
    ...publication3ReleaseTemplate,
    status: {
      ...publication3ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...publication3ReleaseTemplate,
    releaseName: '2016 to 2017',
  },
];

const publication3 = {
  ...publicationTemplate,
  id: 'publication-3',
  slug:
    'pupil-absence-statistics-and-data-for-schools-in-england-autumn-and-spring-terms',
  title:
    'Pupil absence statistics and data for schools in England: autumn and spring terms',
  releases: publication3Releases,
};

const publications = [publication1, publication2, publication3];

export default {
  publications,
};
