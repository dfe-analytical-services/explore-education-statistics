import { DataSetUpload } from '@admin/services/releaseDataFileService';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

interface Props {
  dataSetUpload: DataSetUpload;
}

export default function DataSetUploadSummaryList({ dataSetUpload }: Props) {
  const uploadedByUrl = `mailto:${dataSetUpload.uploadedBy}`;

  return (
    <SummaryList testId="Data file details">
      <SummaryListItem term="Title">
        {dataSetUpload.dataSetTitle}
      </SummaryListItem>
      <SummaryListItem term="Data file">
        {dataSetUpload.dataFileName}
      </SummaryListItem>
      <SummaryListItem term="Meta file">
        {dataSetUpload.metaFileName}
      </SummaryListItem>
      <SummaryListItem term="Size">
        {dataSetUpload.dataFileSizeInBytes} bytes
      </SummaryListItem>
      <SummaryListItem term="Status">{dataSetUpload.status}</SummaryListItem>
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
