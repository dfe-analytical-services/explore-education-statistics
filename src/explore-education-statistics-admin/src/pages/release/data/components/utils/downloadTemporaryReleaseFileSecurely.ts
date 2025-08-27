import releaseDataFileService, {
  FileType,
} from '@admin/services/releaseDataFileService';

/**
 * Download a temporary ReleaseVersion file securely by first obtaining
 * permission to view the file and receiving a short-lived download token,
 * and then using the token to subsequently download the file.
 *
 * @param releaseVersionId - the ID of the ReleaseVersion to which the file
 * belongs.
 * @param dataSetUploadId - the ID of the DataSetUpload record that owns the
 * data and metadata files.
 * @param fileType - the type of file to retrieve from the DataSetUpload.
 * Can be either 'data' or 'metadata'.
 */
const downloadTemporaryReleaseFileSecurely = async ({
  releaseVersionId,
  dataSetUploadId,
  fileType,
}: {
  releaseVersionId: string;
  dataSetUploadId: string;
  fileType: FileType;
}) => {
  const token = await releaseDataFileService.getDownloadTemporaryBlobToken(
    releaseVersionId,
    dataSetUploadId,
    fileType,
  );

  window.open(`/api/download-blob?token=${token}`);
};

export default downloadTemporaryReleaseFileSecurely;
