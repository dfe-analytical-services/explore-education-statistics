import { Publication, Release } from '@common/services/publicationService';

export const testPublication: Publication = {
  id: 'publication-1',
  title: 'Pupil absence in schools in England',
  slug: 'pupil-absence-in-schools-in-england',
  releases: [
    { id: 'release-3', slug: '2018-19', title: 'Academic year 2018/19' },
    { id: 'release-2', slug: '2017-18', title: 'Academic year 2017/18' },
    { id: 'release-1', slug: '2016-17', title: 'Academic year 2016/17' },
  ],
  legacyReleases: [
    {
      id: 'legacy-release-3',
      description: 'Academic year 2014/15',
      url: 'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2014-to-2015',
    },
    {
      id: 'legacy-release-2',
      description: 'Academic year 2013/14',
      url: 'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2013-to-2014',
    },
    {
      id: 'legacy-release-1',
      description: 'Academic year 2012/13',
      url: 'https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2012-to-2013',
    },
  ],
  topic: {
    theme: {
      title: 'Pupils and schools',
    },
  },
  contact: {
    teamName: 'School absence and exclusions team',
    teamEmail: 'schools.statistics@education.gov.uk',
    contactName: 'Sean Gibson',
    contactTelNo: '0370 000 2288',
  },
  methodologies: [
    {
      id: 'caa8e56f-41d2-4129-a5c3-53b051134bd7',
      slug: 'pupil-absence-in-schools-in-england',
      title: 'Pupil absence statistics: methodology',
    },
  ],
  supersededById: 'superseding-publication-id',
  isSuperseded: false,
};

export const testRelease: Release = {
  latestRelease: true,
  publication: testPublication,
  id: 'release-1',
  title: 'Academic year 2016/17',
  yearTitle: '2016/17',
  coverageTitle: 'Academic year',
  releaseName: '2016',
  nextReleaseDate: {
    year: '2019',
    month: '3',
    day: '22',
  },
  published: '2018-04-25T09:30:00',
  slug: '2016-17',
  hasDataGuidance: true,
  hasPreReleaseAccessList: true,
  type: 'NationalStatistics',
  updates: [
    {
      id: '18e0d40e-bdf7-4c84-99dd-732e72e9c9a5',
      reason: 'First update',
      on: new Date('2018-03-22T00:00:00'),
    },
    {
      id: '9c0f0139-7f88-4750-afe0-1c85cdf1d047',
      reason: 'Second update',
      on: new Date('2018-04-19T00:00:00'),
    },
  ],
  content: [
    {
      id: '24c6e9a3-1415-4ca5-9f21-b6b51cb7ba94',
      order: 1,
      heading: 'About these statistics',
      caption: '',
      content: [
        {
          id: '7eeb1478-ab26-4b70-9128-b976429efa2f',
          order: 0,
          body: "The statistics and data cover the absence of pupils of compulsory school age during the 2016/17 academic year in the following state-funded school types:\n\n- primary schools\n- secondary schools\n- special schools\n\nThey also include information for [pupil referral units](/glossary#pupil-referral-unit) and pupils aged 4 years.\n\nWe use the key measures of [overall absence](/glossary#overall-absence) and [persistent absence](/glossary#persistent-absence) to monitor pupil absence and also include [absence by reason](#contents-sections-heading-4) and [pupil characteristics](#contents-sections-heading-6).\n\nThe statistics and data are available at national, regional, local authority (LA) and school level and are used by LAs and schools to compare their local absence rates to regional and national averages for different pupil groups.\n\nThey're also used for policy development as key indicators in behaviour and school attendance policy.\n\nWithin this release, absence by reason is broken down in three different ways:\n\n* distribution of absence by reason - the proportion of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of absences\n\n* rate of absence by reason - the rate of absence for each reason, calculated by taking the number of absences for a specific reason as a percentage of the total number of possible sessions\n\n* one or more sessions missed due to each reason - the number of pupils missing at least 1 session due to each reason",
          type: 'MarkDownBlock',
        },
      ],
    },
    {
      id: '8965ef44-5ad7-4ab0-a142-78453d6f40af',
      order: 2,
      heading: 'Pupil absence rates',
      caption: '',
      content: [
        {
          id: '2c369594-3bbc-40b4-ad19-196c923f5c7f',
          order: 0,
          body: '**Overall absence**\n\nThe [overall absence](/glossary#overall-absence) rate has increased across state-funded primary, secondary and special schools between 2015/16 and 2016/17 driven by an increase in the unauthorised absence rate.\n\nIt increased from 4.6% to 4.7% over this period while the [unauthorised absence](/glossary#unauthorised-absence) rate increased from 1.1% to 1.3%.\n\nThe rate stayed the same at 4% in primary schools but increased from 5.2% to 5.4% for secondary schools. However, in special schools it was much higher and rose to 9.7%.\n\nThe overall and [authorised absence](/glossary#authorised-absence) rates have been fairly stable over recent years after gradually decreasing between 2006/07 and 2013/14.',
          type: 'MarkDownBlock',
        },
        {
          id: '3913a0af-9455-4802-a037-c4cfd4719b18',
          order: 0,
          body: '**Unauthorised absence**\n\nThe [unauthorised absence](/glossary#unauthorised-absence) rate has not varied much since 2006/07 but is at its highest since records began - 1.3%.\n\nThis is due to an increase in absence due to family holidays not agreed by schools.\n\n**Authorised absence**\n\n',
          type: 'MarkDownBlock',
        },
      ],
    },
    {
      id: '6f493eee-443a-4403-9069-fef82e2f5788',
      order: 3,
      heading: 'Persistent absence',
      caption: '',
      content: [
        {
          id: '8a8add13-368c-4067-9210-166bb19a00c1',
          order: 0,
          body: "The [persistent absence](/glossary#persistent-absence) rate increased to and accounted for 37.6% of all absence - up from 36.6% in 2015/16 but still down from 43.3% in 2011/12.\n\nIt also accounted for almost a third (31.6%) of all [authorised absence](/glossary#authorised-absence) and more than half (53.8%) of all [unauthorised absence](/glossary#unauthorised-absence).\n\nOverall, it's increased across primary and secondary schools to 10.8% - up from 10.5% in 2015/16.",
          type: 'MarkDownBlock',
        },
        {
          id: '4aa06200-406b-4f5a-bee4-19e3b83eb1d2',
          order: 0,
          body: '**Persistent absentees**\n\nThe [overall absence](/glossary#overall-absence) rate for persistent absentees across all schools increased to 18.1% - nearly 4 times higher than the rate for all pupils. This is slightly up from 17.6% in 2015/16.\n\n**Illness absence rate**\n\nThe illness absence rate is almost 4 times higher for persistent absentees at 7.6% compared to 2% for other pupils.',
          type: 'MarkDownBlock',
        },
      ],
    },
    {
      id: 'fbf99442-3b72-46bc-836d-8866c552c53d',
      order: 4,
      heading: 'Reasons for absence',
      caption: '',
      content: [
        {
          id: '2ef5f84f-e151-425d-8906-2921712f9157',
          order: 0,
          body: '**Illness**\n\nThis is the main driver behind [overall absence](/glossary#overall-absence) and accounted for 55.3% of all absence - down from 57.3% in 2015/16 and 60.1% in 2014/15.\n\nWhile the overall absence rate has slightly increased since 2015/16 the illness rate has stayed the same at 2.6%.\n\nThe absence rate due to other unauthorised circumstances has also stayed the same since 2015/16 at 0.7%.',
          type: 'MarkDownBlock',
        },
      ],
    },
  ],
  summarySection: {
    id: '4f30b382-ce28-4a3e-801a-ce76004f5eb4',
    order: 1,
    heading: '',
    caption: '',
    content: [
      {
        id: 'a0b85d7d-a9bd-48b5-82c6-a119adc74ca2',
        order: 1,
        body: 'Read national statistical summaries, view charts and tables and download data files.\n\nFind out how and why these statistics are collected and published - [Pupil absence statistics: methodology](/methodology/pupil-absence-in-schools-in-england).\n\nThis release was created as example content during the platform’s Private Beta phase, whilst it provides access to real data, the below release should be used with some caution. To access the original, release please see [Pupil absence in schools in England: 2016 to 2017](https://www.gov.uk/government/statistics/pupil-absence-in-schools-in-england-2016-to-2017)',
        type: 'MarkDownBlock',
      },
    ],
  },
  headlinesSection: {
    id: 'headlines-id',
    order: 1,
    heading: '',
    caption: '',
    content: [
      {
        id: 'b9732ba9-8dc3-4fbc-9c9b-e504e4b58fb9',
        order: 1,
        body: '* pupils missed on average 8.2 school days\n* overall and unauthorised absence rates up on 2015/16\n* unauthorised absence rise due to higher rates of unauthorised holidays\n* 10% of pupils persistently absent during 2016/17\n',
        type: 'MarkDownBlock',
      },
    ],
  },
  keyStatistics: [],
  keyStatisticsSecondarySection: {
    id: 'key-stats-secondary-id',
    order: 1,
    heading: '',
    caption: '',
    content: [],
  },
  relatedDashboardsSection: {
    id: 'related-dashboards-id',
    order: 0,
    heading: '',
    caption: '',
    content: [
      {
        id: 'related-dashboards-content-block-id',
        order: 0,
        body: 'Related dashboards test text',
        type: 'HtmlBlock',
      },
    ],
  },
  downloadFiles: [],
  relatedInformation: [],
};
