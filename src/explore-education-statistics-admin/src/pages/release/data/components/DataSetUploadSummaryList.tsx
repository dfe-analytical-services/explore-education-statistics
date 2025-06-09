import { DataSetUpload } from '@admin/services/releaseDataFileService';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import React from 'react';

interface Props {
  dataSetUpload: DataSetUpload;
}

export default function DataSetUploadSummaryList({ dataSetUpload }: Props) {
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
      <SummaryListItem term="Size">999999 bytes</SummaryListItem>
      <SummaryListItem term="Status">{dataSetUpload.status}</SummaryListItem>
      <SummaryListItem term="Uploaded by">
        <a href="mailto:'joe.bloggs@acme.com">joe.bloggs@acme.com</a>
      </SummaryListItem>
      <SummaryListItem term="Date uploaded">
        <FormattedDate format="d MMMM yyyy, HH:mm">1970-01-01</FormattedDate>
      </SummaryListItem>
    </SummaryList>
  );
}
