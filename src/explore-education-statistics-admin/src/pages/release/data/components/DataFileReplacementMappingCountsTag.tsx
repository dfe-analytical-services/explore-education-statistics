import { SourceMappingType } from '@admin/pages/release/data/components/DataFileReplacementDifferences';
import Tag from '@common/components/Tag';
import React from 'react';

export default function DataFileReplacementMappingCountsTag({
  mappingType,
  countType,
  count,
}: {
  mappingType: SourceMappingType;
  countType: 'mapped' | 'unmapped';
  count: number;
}) {
  return count === 0 ? null : (
    <Tag colour={countType === 'mapped' ? 'blue' : 'red'}>
      {`${count} ${countType} ${count === 1 ? mappingType : `${mappingType}s`}`}
    </Tag>
  );
}
