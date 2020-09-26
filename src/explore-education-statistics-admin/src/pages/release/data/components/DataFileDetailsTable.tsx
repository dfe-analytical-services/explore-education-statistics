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
        <tr data-testid="Subject title">
          <th scope="row" className="govuk-!-width-one-third">
            Subject title
          </th>
          <td>{dataFile.title}</td>
          {replacementDataFile && <td>{replacementDataFile.title}</td>}
        </tr>
        <tr data-testid="Data file">
          <th scope="row">Data file</th>
          <td>
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
            <td>
              <ButtonText
                onClick={() =>
                  releaseDataFileService.downloadFile(
                    releaseId,
                    dataFile.id,
                    dataFile.fileName,
                  )
                }
              >
                {replacementDataFile.fileName}
              </ButtonText>
            </td>
          )}
        </tr>
        <tr data-testid="Metadata file">
          <th scope="row">Metadata file</th>
          <td>
            <ButtonText
              onClick={() =>
                releaseDataFileService.downloadFile(
                  releaseId,
                  dataFile.id,
                  dataFile.metaFileName,
                )
              }
            >
              {dataFile.metaFileName}
            </ButtonText>
          </td>
          {replacementDataFile && (
            <td>
              <ButtonText
                onClick={() =>
                  releaseDataFileService.downloadFile(
                    releaseId,
                    replacementDataFile.id,
                    replacementDataFile.metaFileName,
                  )
                }
              >
                {replacementDataFile.metaFileName}
              </ButtonText>
            </td>
          )}
        </tr>
        <tr data-testid="Data file size">
          <th scope="row">Data file size</th>
          <td>
            {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
          </td>
          {replacementDataFile && (
            <td>
              {replacementDataFile.fileSize.size.toLocaleString()}{' '}
              {replacementDataFile.fileSize.unit}
            </td>
          )}
        </tr>
        <tr data-testid="Number of rows">
          <th scope="row">Number of rows</th>
          <td>{dataFile.rows.toLocaleString()}</td>
          {replacementDataFile && (
            <td>{replacementDataFile.rows.toLocaleString()}</td>
          )}
        </tr>
        <tr data-testid="Status">
          <th scope="row">Status</th>
          <td>
            <ImporterStatus
              releaseId={releaseId}
              dataFile={dataFile}
              onStatusChange={onStatusChange}
            />
          </td>
          {replacementDataFile && (
            <td>
              <ImporterStatus
                releaseId={releaseId}
                dataFile={replacementDataFile}
                onStatusChange={onReplacementStatusChange}
              />
            </td>
          )}
        </tr>
        <tr data-testid="Uploaded by">
          <th scope="row">Uploaded by</th>
          <td>
            <a href={`mailto:${dataFile.userName}`}>{dataFile.userName}</a>
          </td>
          {replacementDataFile && (
            <td>
              <a href={`mailto:${replacementDataFile.userName}`}>
                {replacementDataFile.userName}
              </a>
            </td>
          )}
        </tr>
        {dataFile.created && (
          <tr data-testid="Date uploaded">
            <th scope="row">Date uploaded</th>
            <td>
              <FormattedDate format="d MMMM yyyy HH:mm">
                {dataFile.created}
              </FormattedDate>
            </td>
            {replacementDataFile?.created && (
              <td>
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
