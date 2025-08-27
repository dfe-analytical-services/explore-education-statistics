import releaseDataFileService from '@admin/services/releaseDataFileService';

/**
 * Download a ReleaseVersion file securely by first obtaining permission to
 * view the file and receiving a short-lived download token, and then using
 * the token to subsequently download the file.
 *
 * @param releaseVersionId - the ID of the ReleaseVersion to which the file
 * belongs.
 * @param fileId - the ID of the file to be downloaded.
 */
const downloadReleaseFileSecurely = async ({
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

  window.open(`/api/download-blob?token=${token}`);
};

export default downloadReleaseFileSecurely;
