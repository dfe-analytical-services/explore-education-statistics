import client from '@admin/services/utils/service';
import downloadFile from '@common/utils/file/downloadFile';
import parseContentDisposition from '@common/utils/http/parseContentDisposition';

const releaseFileService = {
  downloadAllFilesZip(releaseId: string): Promise<void> {
    return client.axios
      .get<Blob>(`/release/${releaseId}/files`, {
        responseType: 'blob',
      })
      .then(({ data, headers }) => {
        const disposition = parseContentDisposition(
          headers['content-disposition'],
        );

        if (disposition.type === 'attachment') {
          downloadFile(data, disposition.filename);
        }
      });
  },
};

export default releaseFileService;
