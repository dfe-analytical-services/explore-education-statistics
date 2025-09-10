import client from '@admin/services/utils/service';
import releaseDataFileService from '@admin/services/releaseDataFileService';

/**
 * Stream a ReleaseVersion file securely by first obtaining permission to
 * view the file and receiving a short-lived download token, and then using
 * the token to subsequently stream the file. Differs from
 * "downloadReleaseFileSecurely" in that it does not attempt to initiate a
 * file download, but instead streams the file to the browser to be opened
 * in place.
 *
 * @param releaseVersionId - the ID of the ReleaseVersion to which the file
 * belongs.
 * @param fileId - the ID of the file to be downloaded.
 */
const streamReleaseFileSecurely = async ({
  releaseVersionId,
  fileId,
}: {
  releaseVersionId: string;
  fileId: string;
}) => {
  const token = await releaseDataFileService.getDownloadBlobToken(
    releaseVersionId,
    fileId,
  );

  return client.get<Blob>(`/download-blob?token=${token}`, {
    responseType: 'blob',
  });
};

export default streamReleaseFileSecurely;
