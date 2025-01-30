import ImporterStatus, {
  ImporterStatusChangeHandler,
} from '@admin/pages/release/data/components/ImporterStatus';
import releaseDataFileService, {
  DataFile,
} from '@admin/services/releaseDataFileService';
import ButtonText from '@common/components/ButtonText';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

interface Props {
  dataFile: DataFile;
  releaseVersionId: string;
  onStatusChange: ImporterStatusChangeHandler;
}

export default function DataFileSummaryList({
  dataFile,
  releaseVersionId,
  onStatusChange,
}: Props) {
  return (
    <SummaryList testId="Data file details">
      <SummaryListItem term="Title">{dataFile.title}</SummaryListItem>
      <SummaryListItem term="Data file">
        <ButtonText
          onClick={() =>
            releaseDataFileService.downloadFile(
              releaseVersionId,
              dataFile.id,
              dataFile.fileName,
            )
          }
        >
          {dataFile.fileName}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Meta file">
        <ButtonText
          onClick={() =>
            releaseDataFileService.downloadFile(
              releaseVersionId,
              dataFile.metaFileId,
              dataFile.metaFileName,
            )
          }
        >
          {dataFile.metaFileName}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Size">
        {`${dataFile.fileSize.size.toLocaleString()} ${dataFile.fileSize.unit}`}
      </SummaryListItem>
      <SummaryListItem term="Number of rows">
        {dataFile.rows?.toLocaleString() ?? 'Unknown'}
      </SummaryListItem>
      <SummaryListItem term="Status">
        <ImporterStatus
          releaseVersionId={releaseVersionId}
          dataFile={dataFile}
          onStatusChange={onStatusChange}
        />
      </SummaryListItem>
      <SummaryListItem term="Uploaded by">
        <a href={`mailto:${dataFile.userName}`}>{dataFile.userName}</a>
      </SummaryListItem>
      {dataFile.created && (
        <SummaryListItem term="Date uploaded">
          <FormattedDate format="d MMMM yyyy, HH:mm">
            {dataFile.created}
          </FormattedDate>
        </SummaryListItem>
      )}
    </SummaryList>
  );
}
