import useCallbackRef from '@common/hooks/useCallbackRef';
import useIntersectionObserver from '@common/hooks/useIntersectionObserver';
import styles from '@common/modules/charts/components/MapBlock.module.scss';
import {
  MapFeature,
  MapFeatureCollection,
  MapFeatureProperties,
} from '@common/modules/charts/components/MapBlock';
import { MapDataSetCategoryConfig } from '@common/modules/charts/util/getMapDataSetCategoryConfigs';
import { Dictionary } from '@common/types';
import formatPretty from '@common/utils/number/formatPretty';
import { FeatureCollection } from 'geojson';
import Leaflet, { Layer, PathOptions } from 'leaflet';
import React, { useEffect, useRef, useState } from 'react';
import { GeoJSON, useMap } from 'react-leaflet';

interface Props {
  dataSetCategoryConfigs: Dictionary<MapDataSetCategoryConfig>;
  features?: MapFeatureCollection;
  selectedFeature?: MapFeature;
  selectedDataSetKey: string;
  height: number;
  width?: number;
  onSelectFeature: (feature: MapFeature) => void;
}

export default function MapGeoJSON({
  features,
  selectedFeature,
  width,
  height,
  selectedDataSetKey,
  dataSetCategoryConfigs,
  onSelectFeature,
}: Props) {
  const map = useMap();
  const container = useRef<HTMLDivElement>(null);
  const geometryRef = useRef<Leaflet.GeoJSON>(null);
  const ukRef = useRef<Leaflet.GeoJSON>(null);

  const [ukGeometry, setUkGeometry] = useState<FeatureCollection>();

  const selectedDataSetConfig = dataSetCategoryConfigs[selectedDataSetKey];

  // initialise
  useEffect(() => {
    import('@common/modules/charts/files/ukGeoJson.json').then(imported => {
      setUkGeometry(imported.default as FeatureCollection);
    });
  }, []);

  // set max bounds to prevent dragging far away from UK in the map
  useEffect(() => {
    map.setMaxBounds(map.getBounds());
  }, [map]);

  const [intersectionEntry] = useIntersectionObserver(container, {
    threshold: 0.00001,
  });

  useEffect(() => {
    if (map && intersectionEntry) {
      requestAnimationFrame(() => {
        map.invalidateSize();
      });
    }
  }, [map, intersectionEntry]);

  // For a refresh of the Leaflet map element
  // if width or height are changed
  useEffect(() => {
    if (map) {
      map.invalidateSize();
    }
  }, [map, width, height]);

  // Reset the GeoJson layer if the geometry is changed,
  // updating the component doesn't do it once it's rendered
  useEffect(() => {
    if (geometryRef.current) {
      geometryRef.current.clearLayers();

      if (features) {
        geometryRef.current.addData(features);
      }
    }
  }, [features]);

  useEffect(() => {
    if (!selectedFeature) {
      resetZoom();
      return;
    }

    const { layer } = selectedFeature.properties;

    if (!layer) {
      return;
    }

    layer.bringToFront();

    if (map) {
      // Centers the feature on the map
      map.fitBounds(layer.getBounds());
    }
  }, [map, selectedFeature]);

  const resetZoom = () => map.setZoom(5);

  // We have to assign our `onEachFeature` callback to a ref
  // as `onEachFeature` forms an internal closure which
  // prevents us from updating the callback's dependencies.
  // This would otherwise lead to stale state and most likely
  // result in the callback throwing null pointer errors.
  const onEachFeature = useCallbackRef(
    (feature: MapFeature, featureLayer: Layer) => {
      if (feature.properties) {
        // eslint-disable-next-line no-param-reassign
        feature.properties.layer =
          featureLayer as MapFeatureProperties['layer'];
      }

      featureLayer.bindTooltip(
        () => {
          // Not ideal, we would want to use `max-width` instead.
          // Unfortunately it doesn't seem to work with the tooltip
          // for some reason (maybe due to the pane styling).
          const mapWidth = map?.getContainer().clientWidth;
          const tooltipStyle = mapWidth ? `width: ${mapWidth / 2 - 20}px` : '';

          if (feature.properties.dataSets[selectedDataSetKey]) {
            const dataSetValue = formatPretty(
              feature.properties.dataSets[selectedDataSetKey].value,
              selectedDataSetConfig.dataSet.indicator.unit,
              selectedDataSetConfig.dataSet.indicator.decimalPlaces,
            );
            const content = `${selectedDataSetConfig.config.label}: ${dataSetValue}`;

            return (
              `<div class="${styles.tooltip}" style="${tooltipStyle}">` +
              `<p><strong data-testid="chartTooltip-label">${feature.properties.Name}</strong></p>` +
              `<p class="${styles.tooltipContent}" data-testid="chartTooltip-contents">${content}</p>` +
              `</div>`
            );
          }

          return (
            `<div class="${styles.tooltip}" style="${tooltipStyle}">` +
            `<p><strong data-testid="chartTooltip-label">${feature.properties.Name}</strong></p>` +
            `<p class="${styles.tooltipContent}" data-testid="chartTooltip-contents">No data available.</p>` +
            `</div>`
          );
        },
        { sticky: true },
      );
    },
    [dataSetCategoryConfigs, selectedDataSetKey],
  );

  return (
    <>
      {ukGeometry && Leaflet.Browser.svg && (
        // Don't render the UK's GeoJSON if the browser doesn't support SVGs.
        // This is needed in Jest tests or an error is thrown.
        <GeoJSON
          data={ukGeometry}
          interactive={false}
          style={() => {
            return {
              color: '#cfdce3',
              fillColor: '#003078',
              fillOpacity: 0.1,
              stroke: false,
              weight: 1,
            };
          }}
          ref={ukRef}
        />
      )}

      {features && (
        <GeoJSON
          ref={geometryRef}
          data={features}
          onEachFeature={(...params) => {
            if (onEachFeature.current) {
              onEachFeature.current(...params);
            }
          }}
          style={(feature?: MapFeature): PathOptions => {
            if (!feature) {
              return {};
            }

            return {
              color: '#081b26',
              fillColor: feature.properties.colour,
              fillOpacity: 1,
              weight: selectedFeature?.id === feature.id ? 3 : 1,
            };
          }}
          eventHandlers={{
            click: e => {
              const { feature } = e.sourceTarget;
              if (feature.properties && feature.id) {
                onSelectFeature(feature);
              }
            },
            keydown: e => {
              if (e.originalEvent.code === 'Tab') {
                // https://dfedigital.atlassian.net/browse/EES-5910
                // Reset the map zoom when user tabs through regions
                // to ensure tooltips that appear are in bounds and visible
                resetZoom();
              } else if (e.originalEvent.code === 'Enter') {
                // Also allow a feature to be 'selected' by pressing Enter
                const { feature } = e.sourceTarget;
                if (feature.properties && feature.id) {
                  onSelectFeature(feature);
                }
              }
            },
          }}
        />
      )}
    </>
  );
}
