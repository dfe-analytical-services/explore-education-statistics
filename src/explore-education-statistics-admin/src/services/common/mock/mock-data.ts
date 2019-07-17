import {IdLabelPair} from "@admin/services/common/types";
import {Dictionary} from '@common/types';

const publicationDetailsByReleaseId: Dictionary<IdLabelPair> = {
  'my-publication-1-release-1': {
    id: 'my-publication-1',
    label: 'Pupil absence statistics and data for schools in England',
  },
};

export default {
  getPublicationDetailsForRelease: (releaseId: string) =>
    publicationDetailsByReleaseId[releaseId],
};
