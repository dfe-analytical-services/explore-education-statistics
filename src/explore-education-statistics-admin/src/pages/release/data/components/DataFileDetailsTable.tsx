import ImporterStatus, {
  ImporterStatusChangeHandler,
} from '@admin/pages/release/data/components/ImporterStatus';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import React, { ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  dataFile: DataFile;
  replacementDataFile?: DataFile;
  releaseId: string;
  onStatusChange?: ImporterStatusChangeHandler;
  onReplacementStatusChange?: ImporterStatusChangeHandler;
}

const DataFileDetailsTable = ({
  children,
  dataFile,
  replacementDataFile,
  releaseId,
  onStatusChange,
  onReplacementStatusChange,
}: Props) => {
  return (
    <table>
      {replacementDataFile && (
        <thead>
          <tr>
            <td />
            <th scope="col">Original file</th>
            <th scope="col">Replacement file</th>
          </tr>
        </thead>
      )}
      <tbody>
        <tr>
          <th scope="row" className="govuk-!-width-one-third">
            Subject title
          </th>
          <td data-testid="Subject title">{dataFile.title}</td>
          {replacementDataFile && (
            <td data-testid="Replacement Subject title">
              {replacementDataFile.title}
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Data file</th>
          <td data-testid="Data file">
            <ButtonText
              onClick={() =>
                releaseDataFileService.downloadFile(
                  releaseId,
                  dataFile.id,
                  dataFile.fileName,
                )
              }
            >
              {dataFile.fileName}
            </ButtonText>
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Data file">
              <ButtonText
                onClick={() =>
                  releaseDataFileService.downloadFile(
                    releaseId,
                    replacementDataFile.id,
                    replacementDataFile.fileName,
                  )
                }
              >
                {replacementDataFile.fileName}
              </ButtonText>
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Metadata file</th>
          <td data-testid="Metadata file">
            <ButtonText
              onClick={() =>
                releaseDataFileService.downloadFile(
                  releaseId,
                  dataFile.metaFileId,
                  dataFile.metaFileName,
                )
              }
            >
              {dataFile.metaFileName}
            </ButtonText>
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Metadata file">
              <ButtonText
                onClick={() =>
                  releaseDataFileService.downloadFile(
                    releaseId,
                    replacementDataFile.metaFileId,
                    replacementDataFile.metaFileName,
                  )
                }
              >
                {replacementDataFile.metaFileName}
              </ButtonText>
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Data file size</th>
          <td data-testid="Data file size">
            {`${dataFile.fileSize.size.toLocaleString()} ${
              dataFile.fileSize.unit
            }`}
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Data file size">
              {`${replacementDataFile.fileSize.size.toLocaleString()} ${
                replacementDataFile.fileSize.unit
              }`}
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Number of rows</th>
          <td data-testid="Number of rows">
            {dataFile.rows?.toLocaleString() ?? 'Unknown'}
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Number of rows">
              {replacementDataFile.rows?.toLocaleString() ?? 'Unknown'}
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Status</th>
          <td data-testid="Status">
            <ImporterStatus
              releaseId={releaseId}
              dataFile={dataFile}
              onStatusChange={onStatusChange}
            />
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Status">
              <ImporterStatus
                releaseId={releaseId}
                dataFile={replacementDataFile}
                onStatusChange={onReplacementStatusChange}
              />
            </td>
          )}
        </tr>
        <tr>
          <th scope="row">Uploaded by</th>
          <td data-testid="Uploaded by">
            <a href={`mailto:${dataFile.userName}`}>{dataFile.userName}</a>
          </td>
          {replacementDataFile && (
            <td data-testid="Replacement Uploaded by">
              <a href={`mailto:${replacementDataFile.userName}`}>
                {replacementDataFile.userName}
              </a>
            </td>
          )}
        </tr>
        {dataFile.created && (
          <tr>
            <th scope="row">Date uploaded</th>
            <td data-testid="Date uploaded">
              <FormattedDate format="d MMMM yyyy HH:mm">
                {dataFile.created}
              </FormattedDate>
            </td>
            {replacementDataFile?.created && (
              <td data-testid="Replacement Date uploaded">
                <FormattedDate format="d MMMM yyyy HH:mm">
                  {replacementDataFile.created}
                </FormattedDate>
              </td>
            )}
          </tr>
        )}

        {children && (
          <tr>
            <th scope="row">Actions</th>
            <td colSpan={2}>
              <div className="dfe-float--right">{children}</div>
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
};

export default DataFileDetailsTable;
