import Tag, { TagProps } from '@common/components/Tag';
import React from 'react';
import styles from './ApiUploadStatus.module.scss';

export type ApiUploadStatusValue = 'API DRAFT' | 'API READY' | 'API LIVE';

interface Props {
  status?: ApiUploadStatusValue;
}

const statusColours: Record<ApiUploadStatusValue, TagProps['colour']> = {
  'API DRAFT': 'yellow',
  'API READY': 'green',
  'API LIVE': 'blue',
};

const statusLabels: Record<ApiUploadStatusValue, string> = {
  'API DRAFT': 'API Draft',
  'API READY': 'API Ready',
  'API LIVE': 'API Live',
};

export default function ApiUploadStatus({ status }: Props) {
  if (!status) {
    return null;
  }

  return (
    <div className="govuk-!-padding-top-2">
      <Tag colour={statusColours[status]} className={styles.tag}>
        {statusLabels[status]}
      </Tag>
    </div>
  );
}
