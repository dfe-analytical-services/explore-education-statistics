import client from '@admin/services/utils/service';
import downloadFile from '@common/utils/file/downloadFile';
import parseContentDisposition from '@common/utils/http/parseContentDisposition';

const releaseFileService = {
  // TODO - EES-6480 - rewrite to use token-based secure download?
  // This would avoid the blob buffering in the browser's memory
  // prior to initiating the download.
  //
  // Not easy to do though as this ZIP file is not currently from
  // Blob Storage but is created on-the-fly.
  downloadFilesAsZip(releaseId: string, fileIds?: string[]): Promise<void> {
    return client
      .get<Blob>(
        `/release/${releaseId}/files${
          fileIds ? `/?fileIds=${fileIds.join(',')}` : ''
        }`,
        {
          responseType: 'blob',
          rawResponse: true,
        },
      )
      .then(({ data, headers }) => {
        const disposition = parseContentDisposition(
          headers['content-disposition'],
        );

        if (disposition.type === 'attachment') {
          downloadFile({
            file: data,
            fileName: disposition.filename,
          });
        }
      });
  },
};

export default releaseFileService;
