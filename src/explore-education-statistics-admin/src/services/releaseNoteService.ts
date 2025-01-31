import client from '@admin/services/utils/service';
import { ReleaseNote } from '@common/services/publicationService';

const releaseNoteService = {
  create: (
    releaseId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'on' | 'releaseId'>,
  ): Promise<ReleaseNote[]> => {
    return client.post(
      `/release/${releaseId}/content/release-note`,
      releaseNote,
    );
  },
  delete: (id: string, releaseId: string): Promise<ReleaseNote[]> => {
    return client.delete(`/release/${releaseId}/content/release-note/${id}`);
  },
  edit: (
    id: string,
    releaseId: string,
    releaseNote: Omit<ReleaseNote, 'id' | 'releaseId'>,
  ): Promise<ReleaseNote[]> => {
    return client.put(
      `/release/${releaseId}/content/release-note/${id}`,
      releaseNote,
    );
  },
};

export default releaseNoteService;
