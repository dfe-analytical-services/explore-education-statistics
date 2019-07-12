import {
  ApprovalStatus,
  IdLabelPair,
  Publication,
  Release,
  ReleaseDataType,
  ReleaseSetupDetails,
  Topic,
} from '@admin/services/types/types';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';

const methodologies: IdLabelPair[] = [
  {
    id: 'methodology-1',
    label: 'A guide to absence statistics',
  },
];

const theme1: IdLabelPair = {
  id: 'theme-1',
  label: 'Pupils and schools',
};

const theme1Topic1: Topic = {
  id: '67c249de-1cca-446e-8ccb-dcdac542f460',
  title: 'Pupil absence',
  theme: theme1,
};

const theme1Topic2: Topic = {
  id: '77941b7d-bbd6-4069-9107-565af89e2dec',
  title: 'Exclusions',
  theme: theme1,
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
    code: 'AYQ1Q4',
    startDate: new Date('2017-01-01'),
  },
  scheduledReleaseDate: {
    day: 20,
    month: 9,
    year: 2020,
  },
  nextReleaseExpectedDate: new Date('2021-09-20'),
  status: {
    approvalStatus: ApprovalStatus.Approved,
    isLive: true,
    isLatest: false,
    isNew: false,
    lastEdited: new Date('2019-03-20 17:37'),
    lastEditor: PrototypeLoginService.getUser(
      '4add7621-4aef-4abc-b2e6-0938b37fe5b9',
    ),
    published: new Date('2019-09-20 09:30'),
    nextRelease: new Date('2020-09-20 09:30'),
  },
  releaseType: {
    id: 'national-stats',
    label: 'National statistics',
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
      author: PrototypeLoginService.getUser(
        '8e3a250b-6153-4c5e-aba5-363a554bc288',
      ),
      datetime: new Date('2018-06-17 17:35'),
      content: `Lorem ipsum, dolor sit amet consectetur adipisicing elit.
                Fugit rem, optio sunt dolorum corrupti harum labore quia
                repellat! Quae voluptatem illo soluta optio ducimus at
                possimus quisquam doloremque veritatis provident!`,
    },
    {
      id: '2',
      author: PrototypeLoginService.getUser(
        'b7630cce-7f5f-4233-90fe-a8c751b1c38c',
      ),
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
  owner: PrototypeLoginService.getUser('4add7621-4aef-4abc-b2e6-0938b37fe5b9'),
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
    code: 'AYQ1',
    startDate: new Date('2017-01-01'),
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
    code: 'AYQ1Q3',
    startDate: new Date('2016-01-01'),
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
  owner: PrototypeLoginService.getUser('8e3a250b-6153-4c5e-aba5-363a554bc288'),
  releases: inProgressPublication1Releases,
};

const myPublications: Publication[] = [
  myPublication1,
  myPublication2,
  myPublication3,
];
const inProgressPublications: Publication[] = [inProgressPublication1];
const allPublications = [...myPublications, ...inProgressPublications];

const getReleaseById = (id: string): Release => {
  const allReleases = allPublications.flatMap(
    publication => publication.releases,
  );
  return allReleases.filter(release => release.id === id)[0];
};

const getOwningPublicationForRelease = (release: Release) => {
  return allPublications.filter(publication =>
    publication.releases.includes(release),
  )[0];
};

const getReleaseSetupDetails = (releaseId: string): ReleaseSetupDetails => {
  const release = getReleaseById(releaseId);
  const owningPublication = getOwningPublicationForRelease(release);

  return {
    id: release.id,
    publicationTitle: owningPublication.title,
    timePeriodCoverageCode: release.timePeriodCoverage.code,
    timePeriodCoverageStartDate: release.timePeriodCoverage.startDate,
    releaseType: release.releaseType,
    leadStatisticianName: release.lead.name,
    scheduledReleaseDate: release.scheduledReleaseDate,
    nextReleaseExpectedDate: release.nextReleaseExpectedDate,
  };
};

export interface ThemeAndTopics {
  title: string;
  id: string;
  topics: {
    title: string;
    id: string;
  }[];
}

const themesAndTopics: ThemeAndTopics[] = [
  {
    title: 'Children, early years and social care',
    id: 'cc8e02fd-5599-41aa-940d-26bca68eab53',
    topics: [
      {
        title: 'Childcare and early years',
        id: '1003fa5c-b60a-4036-a178-e3a69a81b852',
      },
      {
        title: 'Children in need and child protection',
        id: '22c52d89-88c0-44b5-96c4-042f1bde6ddd',
      },
      {
        title: "Children's social work workforce",
        id: '734820b7-f80e-45c3-bb92-960edcc6faa5',
      },
      {
        title: 'Early years foundation stage profile',
        id: '17b2e32c-ed2f-4896-852b-513cdf466769',
      },
      {
        title: 'Looked-after children',
        id: '66ff5e67-36cf-4210-9ad2-632baeb4eca7',
      },
      {
        title: "Secure children's homes",
        id: 'd5288137-e703-43a1-b634-d50fc9785cb9',
      },
    ],
  },
  {
    title: 'Destination of pupils and students',
    id: '6412a76c-cf15-424f-8ebc-3a530132b1b3',
    topics: [
      {
        title: 'Destinations of key stage 4 and key stage 5 pupils',
        id: '0b920c62-ff67-4cf1-89ec-0c74a364e6b4',
      },
      {
        title: 'Graduate labour market',
        id: '3bef5b2b-76a1-4be1-83b1-a3269245c610',
      },
      {
        title: 'NEET and participation',
        id: '6a0f4dce-ae62-4429-834e-dd67cee32860',
      },
    ],
  },
  {
    title: 'Finance and funding',
    id: 'bc08839f-2970-4f34-af2d-29608a48082f',
    topics: [
      {
        title: 'Local authority and school finance',
        id: '4c658598-450b-4493-b972-8812acd154a7',
      },
      {
        title: 'Student loan forecasts',
        id: '5c5bc908-f813-46e2-aae8-494804a57aa1',
      },
    ],
  },
  {
    title: 'Further education',
    id: '92c5df93-c4da-4629-ab25-51bd2920cdca',
    topics: [
      {
        title: 'Advanced learner loans',
        id: 'ba0e4a29-92ef-450c-97c5-80a0a6144fb5',
      },
      {
        title: 'FE choices',
        id: 'dd4a5d02-fcc9-4b7f-8c20-c153754ba1e4',
      },
      {
        title: 'Further education and skills',
        id: '88d08425-fcfd-4c87-89da-70b2062a7367',
      },
      {
        title: 'Further education for benefits claimants',
        id: 'cf1f1dc5-27c2-4d15-a55a-9363b7757ff3',
      },
      {
        title: 'National achievement rates tables',
        id: 'dc7b7a89-e968-4a7e-af5f-bd7d19c346a5',
      },
    ],
  },
  {
    title: 'Higher education',
    id: '2ca22e34-b87a-4281-a0eb-b80f4f8dd374',
    topics: [
      {
        title: 'Higher education graduate employment and earnings',
        id: '53a1fbb7-5234-435f-892b-9baad4c82535',
      },
      {
        title: 'Higher education statistics',
        id: '2458a916-df6e-4845-9658-a81eace42ffd',
      },
      {
        title: 'Participation rates in higher education',
        id: '04d95654-9fe0-4f78-9dfd-cf396661ebe9',
      },
      {
        title: 'Widening participation in higher education',
        id: '7871f559-0cfe-47c0-b48d-25b2bc8a0418',
      },
    ],
  },
  {
    title: 'Pupils and schools',
    id: 'ee1855ca-d1e1-4f04-a795-cbd61d326a1f',
    topics: [
      {
        title: 'Admission appeals',
        id: 'c9f0b897-d58a-42b0-9d12-ca874cc7c810',
      },
      {
        title: 'Exclusions',
        id: '77941b7d-bbd6-4069-9107-565af89e2dec',
      },
      {
        title: 'Parental responsibility measures',
        id: '6b8c0242-68e2-420c-910c-e19524e09cd2',
      },
      {
        title: 'Pupil absence',
        id: '67c249de-1cca-446e-8ccb-dcdac542f460',
      },
      {
        title: 'Pupil projections',
        id: '5e196d11-8ac4-4c82-8c46-a10a67c1118e',
      },
      {
        title: 'School and pupils numbers',
        id: 'e50ba9fd-9f19-458c-aceb-4422f0c7d1ba',
      },
      {
        title: 'School applications',
        id: '1a9636e4-29d5-4c90-8c07-f41db8dd019c',
      },
      {
        title: 'School capacity',
        id: '87c27c5e-ae49-4932-aedd-4405177d9367',
      },
      {
        title: 'Special educational needs (SEN)',
        id: '85349b0a-19c7-4089-a56b-ad8dbe85449a',
      },
    ],
  },
  {
    title: 'School and college outcomes and performance',
    id: '74648781-85a9-4233-8be3-fe6f137165f4',
    topics: [
      {
        title: '16 to 19 attainment',
        id: '85b5454b-3761-43b1-8e84-bd056a8efcd3',
      },
      {
        title: 'GCSEs (key stage 4)',
        id: '1e763f55-bf09-4497-b838-7c5b054ba87b',
      },
      {
        title: 'Key stage 1',
        id: '504446c2-ddb1-4d52-bdbc-4148c2c4c460',
      },
      {
        title: 'Key stage 2',
        id: 'eac38700-b968-4029-b8ac-0eb8e1356480',
      },
      {
        title: 'Outcome based success measures',
        id: 'a7ce9542-20e6-401d-91f4-f832c9e58b12',
      },
      {
        title: 'Performance tables',
        id: '1318eb73-02a8-4e50-82a9-7e271176c4d1',
      },
    ],
  },
  {
    title: 'Teachers and school workforce',
    id: 'b601b9ea-b1c7-4970-b354-d1f695c446f1',
    topics: [
      {
        title: 'Initial teacher training (ITT)',
        id: '0f8792d2-28b1-4537-a1b4-3e139fcf0ca7',
      },
      {
        title: 'School workforce',
        id: '28cfa002-83cb-4011-9ddd-859ec99e0aa0',
      },
      {
        title: 'Teacher workforce statistics and analysis',
        id: '6d434e17-7b76-425d-897d-c7b369b42e35',
      },
    ],
  },
  {
    title: 'UK education and training statistics',
    id: 'a95d2ca2-a969-4320-b1e9-e4781112574a',
    topics: [
      {
        title: 'UK education and training statistics',
        id: '692050da-9ac9-435a-80d5-a6be4915f0f7',
      },
    ],
  },
];

export default {
  myPublications,
  allPublications,
  getReleaseById,
  getOwningPublicationForRelease,
  getReleaseSetupDetails,
  themesAndTopics,
};
