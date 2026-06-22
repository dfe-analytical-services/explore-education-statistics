import Tag from '@common/components/Tag';
import React from 'react';
import { TypeMapping } from '@admin/pages/release/data/components/DataFileReplacementDifferencesTable';

export default function DataFileReplacementMappingCountsTag({
  mappingType,
  countType,
  count,
}: {
  mappingType: keyof TypeMapping;
  countType: 'mapped' | 'unmapped';
  count: number;
}) {
  return count === 0 ? null : (
    <Tag colour={countType === 'mapped' ? 'blue' : 'red'}>
      {`${count} ${countType} ${count === 1 ? mappingType : `${mappingType}s`}`}
    </Tag>
  );
}
