import client from '@admin/services/utils/service';
import { ReleaseNote } from '@common/services/publicationService';

const releaseNoteService = {
  create: (
    releaseVersionId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'on' | 'releaseVersionId'>,
  ): Promise<ReleaseNote[]> => {
    return client.post(
      `/release/${releaseVersionId}/content/release-note`,
      releaseNote,
    );
  },
  delete: (id: string, releaseVersionId: string): Promise<ReleaseNote[]> => {
    return client.delete(
      `/release/${releaseVersionId}/content/release-note/${id}`,
    );
  },
  edit: (
    id: string,
    releaseVersionId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'releaseVersionId'>,
  ): Promise<ReleaseNote[]> => {
    return client.put(
      `/release/${releaseVersionId}/content/release-note/${id}`,
      releaseNote,
    );
  },
};

export default releaseNoteService;
