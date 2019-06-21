import React from 'react';
import {
  Methodology,
  ReleaseDataType,
  Release,
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

const timePeriodTermSpring: TimePeriod = {
  id: 'term-spring',
  title: 'Spring term',
};

const timePeriodTermSummer: TimePeriod = {
  id: 'term-summer',
  title: 'Summer term',
};

const dataTypeFinal: ReleaseDataType = {
  id: 'data-type-final',
  title: 'Final',
};

const dataTypeProvisional: ReleaseDataType = {
  id: 'data-type-provisional',
  title: 'Provisional',
};

const dataTypeRevised: ReleaseDataType = {
  id: 'data-type-revised',
  title: 'Revised',
};

const releases: Release[] = [
  {
    id: 'release-1',
    releaseName: '2018 to 2019',
    slug: '2018-2019',
    timePeriodCoverage: {
      label: 'Academic year',
      academicYear: {
        yearStarting: 2018,
        timePeriod: timePeriodTermFullAcademicYear,
        termsPerYear: 6,
      },
    },
    status: {
      title: 'Ready to review',
      isLive: true,
      isLatest: true,
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
  },
];

const publications = [
  {
    id: 'publication-1',
    slug: 'pupil-absence-statistics-and-data-for-schools-in-england',
    title: 'Pupil absence statistics and data for schools in England',
    description: '',
    dataSource: '',
    summary: '',
    nextUpdate: '',
    releases,
    legacyReleases: [],
    topic: topics[0],
    contact: {
      teamName: '',
      teamEmail: '',
      contactName: '',
      contactTelNo: '',
    },
    methodology: methodologies[0],
  },
];

export default {
  publications,
};
