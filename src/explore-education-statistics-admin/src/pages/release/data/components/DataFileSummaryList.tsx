import ImporterStatus, {
  ImportStatusChangeHandler,
} from '@admin/components/ImporterStatus';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import releaseMetaFileService from '@admin/services/releaseMetaFileService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React, { ReactNode } from 'react';

interface Props {
  children?: ReactNode;
  dataFile: DataFile;
  releaseId: string;
  onStatusChange?: ImportStatusChangeHandler;
}

const DataFileSummaryList = ({
  children,
  dataFile,
  releaseId,
  onStatusChange,
}: Props) => {
  return (
    <SummaryList>
      <SummaryListItem term="Subject title">{dataFile.title}</SummaryListItem>
      <SummaryListItem term="Data file">
        <ButtonText
          onClick={() =>
            releaseDataFileService.downloadDataFile(
              releaseId,
              dataFile.filename,
            )
          }
        >
          {dataFile.filename}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Metadata file">
        <ButtonText
          onClick={() =>
            releaseMetaFileService.downloadDataMetadataFile(
              releaseId,
              dataFile.metadataFilename,
            )
          }
        >
          {dataFile.metadataFilename}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Data file size">
        {dataFile.fileSize.size.toLocaleString()} {dataFile.fileSize.unit}
      </SummaryListItem>
      <SummaryListItem term="Number of rows">
        {dataFile.rows.toLocaleString()}
      </SummaryListItem>

      <ImporterStatus
        releaseId={releaseId}
        dataFile={dataFile}
        onStatusChange={onStatusChange}
      />

      {dataFile.userName && (
        <SummaryListItem term="Uploaded by">
          <a href={`mailto:${dataFile.userName}`}>{dataFile.userName}</a>
        </SummaryListItem>
      )}

      {dataFile.created && (
        <SummaryListItem term="Date uploaded">
          <FormattedDate format="d MMMM yyyy HH:mm">
            {dataFile.created}
          </FormattedDate>
        </SummaryListItem>
      )}

      {children}
    </SummaryList>
  );
};

export default DataFileSummaryList;
