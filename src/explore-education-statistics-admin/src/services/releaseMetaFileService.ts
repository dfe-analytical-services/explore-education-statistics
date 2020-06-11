import client from '@admin/services/utils/service';
import downloadFile from './utils/file/downloadFile';

const releaseMetaFileService = {
  downloadDataMetadataFile(releaseId: string, fileName: string): Promise<void> {
    return client
      .get<Blob>(`/release/${releaseId}/meta/${fileName}`, {
        responseType: 'blob',
      })
      .then(response => downloadFile(response, fileName));
  },
};

export default releaseMetaFileService;
