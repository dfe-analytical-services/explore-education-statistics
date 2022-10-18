import { MapBlockProps as MapBlockInternalProps } from '@common/modules/charts/components/MapBlockInternal';
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
    canPositionLegendInline: false,
    canSize: true,
    canSort: false,
    hasGridLines: false,
    hasLegend: true,
    hasLegendPosition: false,
    hasLineStyle: false,
    hasReferenceLines: false,
    hasSymbols: false,
    requiresGeoJson: true,
    stackable: false,
  },
  options: {
    defaults: {
      height: 600,
      dataClassification: 'EqualIntervals',
      dataGroups: 5,
      customDataGroups: [],
    },
  },
  legend: {
    defaults: {},
  },
  axes: {
    major: {
      id: 'geojson',
      title: 'GeoJSON (major axis)',
      type: 'major',
      hide: true,
      capabilities: {
        canRotateLabel: false,
      },
      defaults: {
        groupBy: 'locations',
      },
      constants: {
        groupBy: 'locations',
      },
    },
  },
};

export default memo(MapBlock);
