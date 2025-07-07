import releaseDataFileService, {
  DataSetUpload,
} from '@admin/services/releaseDataFileService';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import Tag from '@common/components/Tag';
import React from 'react';
import ButtonText from '@common/components/ButtonText';
import {
  getDataSetUploadStatusColour,
  getDataSetUploadStatusLabel,
} from './ImporterStatus';

interface Props {
  releaseVersionId: string;
  dataSetUpload: DataSetUpload;
}

export default function DataSetUploadSummaryList({
  releaseVersionId,
  dataSetUpload,
}: Props) {
  const uploadedByUrl = `mailto:${dataSetUpload.uploadedBy}`;

  return (
    <SummaryList testId="Data file details">
      <SummaryListItem term="Title">
        {dataSetUpload.dataSetTitle}
      </SummaryListItem>
      <SummaryListItem term="Data file">
        <ButtonText
          onClick={() =>
            releaseDataFileService.downloadTemporaryFile(
              releaseVersionId,
              'data',
              dataSetUpload.id,
              dataSetUpload.dataFileName,
            )
          }
        >
          {dataSetUpload.dataFileName}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Meta file">
        <ButtonText
          onClick={() =>
            releaseDataFileService.downloadTemporaryFile(
              releaseVersionId,
              'metadata',
              dataSetUpload.id,
              dataSetUpload.metaFileName,
            )
          }
        >
          {dataSetUpload.metaFileName}
        </ButtonText>
      </SummaryListItem>
      <SummaryListItem term="Size">
        {dataSetUpload.dataFileSize}
      </SummaryListItem>
      <SummaryListItem term="Status">
        <Tag colour={getDataSetUploadStatusColour(dataSetUpload.status)}>
          {getDataSetUploadStatusLabel(dataSetUpload.status)}
        </Tag>
      </SummaryListItem>{' '}
      <SummaryListItem term="Uploaded by">
        <a href={uploadedByUrl}>{dataSetUpload.uploadedBy}</a>
      </SummaryListItem>
      <SummaryListItem term="Date uploaded">
        <FormattedDate format="d MMMM yyyy, HH:mm">
          {dataSetUpload.created}
        </FormattedDate>
      </SummaryListItem>
    </SummaryList>
  );
}
