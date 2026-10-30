import { DataFile } from '@admin/services/releaseDataFileService';
import Tag, { TagProps } from '@common/components/Tag';
import React from 'react';

type ApiStatus = NonNullable<DataFile['apiStatus']>;

interface Props {
  status?: ApiStatus;
}

const statusColours: Record<ApiStatus, TagProps['colour']> = {
  'API DRAFT': 'yellow',
  'API READY': 'green',
  'API LIVE': 'blue',
};

export default function ApiUploadStatus({ status }: Props) {
  if (!status) {
    return null;
  }

  return (
    <div className="govuk-!-padding-top-2">
      <Tag colour={statusColours[status]}>{status}</Tag>
    </div>
  );
}
