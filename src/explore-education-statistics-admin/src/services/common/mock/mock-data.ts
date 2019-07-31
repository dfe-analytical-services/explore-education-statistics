import { IdTitlePair } from '@admin/services/common/types';
import { Dictionary } from '@common/types';

const publicationDetailsByReleaseId: Dictionary<IdTitlePair> = {
  'my-publication-1-release-1': {
    id: 'my-publication-1',
    title: 'Pupil absence statistics and data for schools in England',
  },
};

export default {
  getPublicationDetailsForRelease: (releaseId: string) =>
    publicationDetailsByReleaseId[releaseId],
};
