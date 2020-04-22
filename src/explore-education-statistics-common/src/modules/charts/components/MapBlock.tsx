import { MapBlockInternalProps } from '@common/modules/charts/components/MapBlockInternal';
import { ChartDefinition } from '@common/modules/charts/types/chart';
import dynamic from 'next/dynamic';
import React, { memo } from 'react';

const MapBlockInternal = dynamic(
  () => import('@common/modules/charts/components/MapBlockInternal'),
  {
    ssr: false,
  },
);

export type MapBlockProps = MapBlockInternalProps;

/**
 * Wrapper component so that we can lazily render {@see MapBlockInternal}.
 * We need this as it otherwise causes issues with SSR due to Leaflet
 * trying to initialise server-side (causing undefined `window` errors).
 *
 * We also need to keep the {@see mapBlockDefinition} here as importing
 * it from {@see MapBlockInternal} also triggers SSR errors.
 */
const MapBlock = (props: MapBlockProps) => {
  return <MapBlockInternal {...props} />;
};

export const mapBlockDefinition: ChartDefinition = {
  type: 'map',
  name: 'Geographic',
  capabilities: {
    dataSymbols: false,
    stackable: false,
    lineStyle: false,
    gridLines: false,
    canSize: true,
    canSort: false,
    fixedAxisGroupBy: true,
    hasAxes: false,
    hasReferenceLines: false,
    hasLegend: false,
    requiresGeoJson: true,
  },
  options: {
    defaults: {
      height: 600,
      legend: 'none',
    },
  },
  data: [
    {
      type: 'geojson',
      title: 'Geographic',
      entryCount: 'multiple',
      targetAxis: 'geojson',
    },
  ],
  axes: {
    major: {
      id: 'geojson',
      title: 'GeoJSON (major axis)',
      type: 'major',
      constants: {
        groupBy: 'locations',
      },
    },
  },
};

export default memo(MapBlock);
