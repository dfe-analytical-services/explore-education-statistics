import ModalConfirm from '@common/components/ModalConfirm';
import WarningMessage from '@common/components/WarningMessage';
import { ArchiveDataSetFile } from 'src/services/releaseDataFileService';
import React from 'react';

interface Props {
  bulkUploadPlan: ArchiveDataSetFile[];
  onConfirm: (bulkUploadPlan: ArchiveDataSetFile[]) => void;
  onCancel: () => void;
}

export default function BulkZipUploadModalConfirm({
  bulkUploadPlan,
  onConfirm,
  onCancel,
}: Props) {
  return (
    <ModalConfirm
      title="Upload summary"
      open
      onConfirm={() => onConfirm(bulkUploadPlan)}
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
          {bulkUploadPlan.map(archiveDataSet => (
            <tr key={archiveDataSet.title}>
              <td>{archiveDataSet.title}</td>
              <td>{archiveDataSet.dataFileName}</td>
              <td>
                {archiveDataSet.replacingFileId && (
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
