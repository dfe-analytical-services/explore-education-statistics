import { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import dynamic from 'next/dynamic';
import React, { memo } from 'react';

const MapBlockInternal = dynamic(
  () => import('@common/modules/charts/components/MapBlock'),
  {
    ssr: false,
  },
);

/**
 * Wrapper component so that we can lazily render {@see MapBlockInternal}.
 * We need this as it otherwise causes issues with SSR due to Leaflet
 * trying to initialise server-side (causing undefined `window` errors).
 *
 * We also need to keep the {@see mapBlockDefinition} here as importing
 * it from {@see MapBlockInternal} also triggers SSR errors.
 */
const DynamicMapBlock = (props: MapBlockProps) => {
  return <MapBlockInternal {...props} />;
};

export default memo(DynamicMapBlock);
