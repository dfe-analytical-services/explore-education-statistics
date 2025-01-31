import { DataBlockTabsProps } from '@common/modules/find-statistics/components/DataBlockTabs';
import dynamic from 'next/dynamic';
import React, { memo } from 'react';

const DataBlockTabsInternal = dynamic(
  () => import('@common/modules/find-statistics/components/DataBlockTabs'),
  {
    ssr: false,
  },
);

/**
 * Wrapper component so that we can lazily render {@see DataBlockTabs}.
 * We need this as it otherwise causes issues with SSR due to Leaflet
 * trying to initialise server-side (causing undefined `window` errors).
 */
const DataBlockTabs = (props: DataBlockTabsProps) => {
  return <DataBlockTabsInternal {...props} />;
};

export default memo(DataBlockTabs);
