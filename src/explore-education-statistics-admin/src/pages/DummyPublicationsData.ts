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

const theme1: Theme = {
  id: 'theme-1',
  title: 'Pupils and schools',
};

const theme1Topic1: Topic = {
  id: 'topic-1',
  title: 'pupil absence',
  theme: theme1,
};

const theme1Topic2: Topic = {
  id: 'topic-2',
  title: 'exclusions',
  theme: theme1,
};

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
  id: 'my-publication-1-release-1',
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
  scheduledReleaseDate: new Date('2020-09-20'),
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
  dataType: dataTypeRevised,
  lead: {
    name: 'John Smith',
    telNo: '07654 653763',
    email: 'js@example.com',
  },
  comments: [
    {
      id: '1',
      author: PrototypeLoginService.getUser('user2'),
      datetime: new Date('2018-06-17 17:35'),
      content: `Lorem ipsum, dolor sit amet consectetur adipisicing elit.
                Fugit rem, optio sunt dolorum corrupti harum labore quia
                repellat! Quae voluptatem illo soluta optio ducimus at
                possimus quisquam doloremque veritatis provident!`,
    },
    {
      id: '2',
      author: PrototypeLoginService.getUser('user3'),
      datetime: new Date('2018-06-17 13:35'),
      content: `Corrupti harum labore quia repellat! Quae voluptatem illo
                soluta optio ducimus at possimus quisquam doloremque veritatis
                provident!`,
    },
  ],
};

const publicationTemplate: Publication = {
  id: 'my-publication-1',
  slug: 'pupil-absence-statistics-and-data-for-schools-in-england',
  title: 'Pupil absence statistics and data for schools in England',
  description: '',
  dataSource: '',
  summary: '',
  nextUpdate: '',
  releases: [],
  legacyReleases: [],
  topic: theme1Topic1,
  contact: {
    email: '',
    name: '',
    telNo: '',
  },
  methodology: methodologies[0],
  owner: PrototypeLoginService.getUser('user1'),
};

//
// MY PUBLICATION 1
//

const myPublication1ReleaseTemplate = {
  ...releaseTemplate,
};

const myPublication1Releases: Release[] = [
  {
    ...myPublication1ReleaseTemplate,
    status: {
      ...myPublication1ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...myPublication1ReleaseTemplate,
    id: 'my-publication-1-release-2',
    releaseName: '2016 to 2017',
  },
  {
    ...myPublication1ReleaseTemplate,
    id: 'my-publication-1-release-3',
    releaseName: '2015 to 2016',
  },
];

const myPublication1 = {
  ...publicationTemplate,
  releases: myPublication1Releases,
};

//
// MY PUBLICATION 2
//

const myPublication2ReleaseTemplate = {
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

const myPublication2Releases: Release[] = [
  {
    ...myPublication2ReleaseTemplate,
    id: 'my-publication-2-release-1',
    status: {
      ...myPublication2ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...myPublication2ReleaseTemplate,
    id: 'my-publication-2-release-2',
    releaseName: '2016 to 2017',
  },
];

const myPublication2 = {
  ...publicationTemplate,
  id: 'my-publication-2',
  slug: 'pupil-absence-statistics-and-data-for-schools-in-england-autumn-term',
  title:
    'Pupil absence statistics and data for schools in England: autumn term',
  releases: myPublication2Releases,
};

//
// MY PUBLICATION 3
//

const myPublication3ReleaseTemplate = {
  ...releaseTemplate,
  timePeriodCoverage: {
    label: 'Autumn and spring terms, academic year',
    academicYear: {
      yearStarting: 2017,
      timePeriod: timePeriodTermAutumn,
      termsPerYear: 6,
    },
  },
  topic: theme1Topic2,
};

const myPublication3Releases: Release[] = [
  {
    ...myPublication3ReleaseTemplate,
    id: 'my-publication-3-release-1',
    status: {
      ...myPublication3ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...myPublication3ReleaseTemplate,
    id: 'my-publication-1-release-2',
    releaseName: '2016 to 2017',
  },
];

const myPublication3 = {
  ...publicationTemplate,
  id: 'my-publication-3',
  slug:
    'pupil-exclusions-statistics-and-data-for-schools-in-england-autumn-and-spring-terms',
  title:
    'Pupil exclusions statistics and data for schools in England: autumn and spring terms',
  releases: myPublication3Releases,
  topic: theme1Topic2,
};

//
// IN PROGRESS PUBLICATION 1
//

const inProgressPublication1ReleaseTemplate = {
  ...releaseTemplate,
};

const inProgressPublication1Releases: Release[] = [
  {
    ...inProgressPublication1ReleaseTemplate,
    id: 'in-progress-publication-1-release-1',
    status: {
      ...inProgressPublication1ReleaseTemplate.status,
      isLatest: true,
    },
  },
  {
    ...inProgressPublication1ReleaseTemplate,
    id: 'in-progress-publication-1-release-2',
    releaseName: '2016 to 2017',
  },
  {
    ...inProgressPublication1ReleaseTemplate,
    id: 'in-progress-publication-1-release-3',
    releaseName: '2015 to 2016',
  },
];

const inProgressPublication1 = {
  ...publicationTemplate,
  title:
    'Pupil absence statistics and data for schools in England: autumn and spring terms',
  owner: PrototypeLoginService.getUser('user2'),
  releases: inProgressPublication1Releases,
};

const myPublications: Publication[] = [
  myPublication1,
  myPublication2,
  myPublication3,
];
const inProgressPublications: Publication[] = [inProgressPublication1];
const allPublications = [...myPublications, ...inProgressPublications];

export default {
  allPublications,
  getReleaseById(id: string): Release {
    const allReleases = allPublications.flatMap(
      publication => publication.releases,
    );
    return allReleases.filter(release => release.id === id)[0];
  },
  getOwningPublicationForRelease(release: Release): Publication | undefined {
    return allPublications.find(publication =>
      publication.releases.includes(release),
    );
  },
};
