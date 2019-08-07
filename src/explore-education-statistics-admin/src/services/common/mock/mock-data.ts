import {BasicPublicationDetails, IdTitlePair} from '@admin/services/common/types';
import { Dictionary } from '@common/types';

const publicationDetailsByPublicationId: Dictionary<BasicPublicationDetails> = {
  'my-publication-1': {
    id: 'my-publication-1',
    title: 'Pupil absence statistics and data for schools in England',
    contact: {
      id: 'contact-1',
      contactName: 'Amy Evans',
      contactTelNo: '01234 567890',
      teamName: 'Team 1',
      teamEmail: 'team1@example.com',
    }
  },
};

const releaseTypes: IdTitlePair[] = [
  {
    id: 'national-statistics',
    title: 'National Statistics',
  },
  {
    id: 'adhoc-statistics',
    title: 'Official / ad hoc statistics',
  },
];

export default {
  publicationDetailsByPublicationId,
  getPublicationDetails: (publicationId: string) =>
    publicationDetailsByPublicationId[publicationId],
  getReleaseTypes: () => releaseTypes,
};
