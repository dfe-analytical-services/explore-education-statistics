import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import { DataSetUploadResult } from 'src/services/releaseDataFileService';
import React from 'react';

interface Props {
  uploadResults: DataSetUploadResult[];
  onConfirm: (uploadResult: string[]) => void;
  onCancel: () => void;
}

export default function DataSetUploadModalConfirm({
  uploadResults,
  onConfirm,
  onCancel,
}: Props) {
  return (
    <ModalConfirm
      title="Upload summary"
      open
      onConfirm={() => onConfirm(uploadResults.map(upload => upload.id))}
      onExit={onCancel}
      onCancel={onCancel}
    >
      <table>
        <thead>
          <tr>
            <th scope="col">Dataset name</th>
            <th scope="col">Data file</th>
            <th scope="col">Additional information</th>
          </tr>
        </thead>
        <tbody>
          {uploadResults.map(uploadResult => (
            <tr key={uploadResult.id}>
              <td>{uploadResult.dataSetTitle}</td>
              <td>{uploadResult.dataFileName}</td>
              <td>
                {uploadResult.replacingFileId && (
                  <WarningMessage className="govuk-!-margin-0 govuk-!-padding-0">
                    Upload will initiate a file replacement
                  </WarningMessage>
                )}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </ModalConfirm>
  );
}
