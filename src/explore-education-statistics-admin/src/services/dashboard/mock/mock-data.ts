import {
  AdminDashboardPublication,
  ReleaseApprovalStatus,
  ThemeAndTopics,
} from '@admin/services/dashboard/types';
import { PrototypeLoginService } from '@admin/services/PrototypeLoginService';

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

const dashboardPublications: AdminDashboardPublication[] = [
  {
    id: 'publication-1',
    title: 'Pupil absence statistics and data for schools in England',
    methodology: {
      id: 'methodology-1',
      label: 'A guide to absence statistics',
    },
    contact: {
      contactName: 'John Smith',
      contactTelNo: '07654 653763',
      teamEmail: 'js@example.com',
    },
    releases: [
      {
        id: 'my-publication-1-release-1',
        releaseName: '2017-2018',
        timePeriodCoverage: {
          id: 'AYQ1Q4',
          label: 'Academic year',
        },
        status: ReleaseApprovalStatus.Approved,
        latestRelease: true,
        live: true,
        publishScheduled: {
          day: 20,
          month: 9,
          year: 2020,
        },
        nextReleaseExpectedDate: {
          day: 20,
          month: 9,
          year: 2021,
        },
        contact: {
          contactName: 'John Smith',
          contactTelNo: '07654 653763',
          teamEmail: 'js@example.com',
        },
        lastEditedDateTime: '2019-09-20 09:30',
        lastEditedUser: PrototypeLoginService.getUser(
          '4add7621-4aef-4abc-b2e6-0938b37fe5b9',
        ),
      },
    ],
  },
];

export default {
  themesAndTopics,
  dashboardPublications,
};
