import { FormGroup, FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import useCallbackRef from '@common/hooks/useCallbackRef';
import useIntersectionObserver from '@common/hooks/useIntersectionObserver';
import styles from '@common/modules/charts/components/MapBlock.module.scss';
import createMapDataSetCategories, {
  MapDataSetCategory,
} from '@common/modules/charts/components/utils/createMapDataSetCategories';
import generateLegendDataGroups, {
  LegendDataGroup,
} from '@common/modules/charts/components/utils/generateLegendDataGroups';
import {
  AxisConfiguration,
  ChartProps,
  CustomDataGroup,
  DataClassification,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import { LegendConfiguration } from '@common/modules/charts/types/legend';
import getDataSetCategoryConfigs, {
  DataSetCategoryConfig,
} from '@common/modules/charts/util/getDataSetCategoryConfigs';
import KeyStatTile from '@common/modules/find-statistics/components/KeyStatTile';
import { GeoJsonFeatureProperties } from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import generateHslColour from '@common/utils/colour/generateHslColour';
import locationLevelsMap from '@common/utils/locationLevelsMap';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Layer, Path, PathOptions, Polyline } from 'leaflet';
import keyBy from 'lodash/keyBy';
import orderBy from 'lodash/orderBy';
import uniq from 'lodash/uniq';
import React, {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';

export interface MapFeatureProperties extends GeoJsonFeatureProperties {
  colour: string;
  data: number;
  dataSets: DataSetCategory['dataSets'];
  layer?: Layer & Path & Polyline;
}

export type MapFeature = Feature<Geometry, MapFeatureProperties>;

export type MapFeatureCollection = FeatureCollection<
  Geometry,
  MapFeatureProperties
>;

export interface MapBlockProps extends ChartProps {
  axes: {
    major: AxisConfiguration;
  };
  // eslint-disable-next-line react/no-unused-prop-types
  boundaryLevel?: number;
  customDataGroups?: CustomDataGroup[];
  dataGroups?: number;
  dataClassification?: DataClassification;
  id: string;
  legend: LegendConfiguration;
  // eslint-disable-next-line react/no-unused-prop-types
  maxBounds?: LatLngBounds;
  position?: { lat: number; lng: number };
}

export default function MapBlockInternal({
  id,
  customDataGroups = [],
  data,
  dataGroups = 5,
  dataClassification = 'EqualIntervals',
  meta,
  legend,
  position = { lat: 53.00986, lng: -3.2524038 },
  width,
  height,
  axes,
}: MapBlockProps) {
  const mapRef = useRef<Map>(null);
  const container = useRef<HTMLDivElement>(null);
  const geometryRef = useRef<GeoJSON>(null);
  const ukRef = useRef<GeoJSON>(null);

  const axisMajor = useMemo<AxisConfiguration>(
    () => ({
      ...axes.major,
      // Enforce grouping by locations
      groupBy: 'locations',
    }),
    [axes.major],
  );

  const dataSetCategories = useMemo<MapDataSetCategory[]>(
    () => createMapDataSetCategories(axisMajor, data, meta),
    [axisMajor, data, meta],
  );

  const dataSetCategoryConfigs = useMemo<Dictionary<DataSetCategoryConfig>>(
    () =>
      keyBy(
        getDataSetCategoryConfigs(dataSetCategories, legend.items, meta),
        dataSetConfig => dataSetConfig.dataKey,
      ),
    [dataSetCategories, legend, meta],
  );

  const dataSetOptions = useMemo<SelectOption[]>(() => {
    return orderBy(
      Object.values(dataSetCategoryConfigs).map(dataSet => ({
        label: dataSet.config.label,
        value: dataSet.dataKey,
      })),
      ['label'],
    );
  }, [dataSetCategoryConfigs]);

  const shouldGroupLocationOptions = useMemo(() => {
    return dataSetCategories.some(
      element =>
        element.filter.level === 'localAuthority' ||
        element.filter.level === 'localAuthorityDistrict',
    );
  }, [dataSetCategories]);

  // If there are no LAs or LADs don't group the locations.
  const locationOptions = useMemo(() => {
    if (shouldGroupLocationOptions) {
      return undefined;
    }
    return orderBy(
      dataSetCategories.map(dataSetCategory => ({
        label: dataSetCategory.filter.label,
        value: dataSetCategory.filter.id,
      })),
      ['label'],
    );
  }, [dataSetCategories, shouldGroupLocationOptions]);

  // If there are LAs or LADs, group them by region and group any others by level
  const groupedLocationOptions = useMemo(() => {
    if (!shouldGroupLocationOptions) {
      return undefined;
    }
    return dataSetCategories.reduce<Dictionary<SelectOption[]>>(
      (acc, { filter }) => {
        const groupLabel =
          filter.level === 'localAuthority' ||
          filter.level === 'localAuthorityDistrict'
            ? (filter.group as string)
            : locationLevelsMap[filter.level].label;

        (acc[groupLabel] ??= []).push({
          label: filter.label,
          value: filter.id,
        });
        return acc;
      },
      {},
    );
  }, [dataSetCategories, shouldGroupLocationOptions]);

  const locationType = useMemo(() => {
    const levels = uniq(
      dataSetCategories.map(category => category.filter.level),
    );
    return !levels.every(level => level === levels[0]) ||
      !locationLevelsMap[levels[0]]
      ? { label: 'location', prefix: 'a' }
      : locationLevelsMap[levels[0]];
  }, [dataSetCategories]);

  const [selectedDataSetKey, setSelectedDataSetKey] = useState<string>(
    (dataSetOptions[0]?.value as string) ?? '',
  );
  const [selectedFeature, setSelectedFeature] = useState<MapFeature>();

  const [features, setFeatures] = useState<MapFeatureCollection>();
  const [ukGeometry, setUkGeometry] = useState<FeatureCollection>();

  const [legendDataGroups, setLegendDataGroups] = useState<LegendDataGroup[]>(
    [],
  );

  const selectedDataSetConfig = dataSetCategoryConfigs[selectedDataSetKey];

  const selectedDataSet =
    selectedFeature?.properties?.dataSets[selectedDataSetKey];

  // initialise
  useEffect(() => {
    import('@common/modules/charts/files/ukGeoJson.json').then(imported => {
      setUkGeometry(imported.default as FeatureCollection);
    });
  }, []);

  const [intersectionEntry] = useIntersectionObserver(container, {
    threshold: 0.00001,
  });

  useEffect(() => {
    const { current } = mapRef;
    if (current && intersectionEntry) {
      requestAnimationFrame(() => {
        current.leafletElement.invalidateSize();
      });
    }
  }, [intersectionEntry]);

  // For a refresh of the Leaflet map element
  // if width or height are changed
  useEffect(() => {
    if (mapRef.current) {
      mapRef.current.leafletElement.invalidateSize();
    }
  }, [width, height]);

  // Rebuild the geometry if the selection has changed
  useEffect(() => {
    if (dataSetCategories.length && selectedDataSetConfig) {
      const {
        features: newFeatures,
        dataGroups: newDataGroups,
      } = generateFeaturesAndDataGroups({
        selectedDataSetConfig,
        dataSetCategories,
        dataGroups,
        classification: dataClassification,
        customDataGroups,
      });

      setFeatures(newFeatures);
      setLegendDataGroups(newDataGroups);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [
    customDataGroups.length,
    dataGroups,
    dataClassification,
    dataSetCategories,
    dataSetCategoryConfigs,
    meta,
    selectedDataSetConfig,
    selectedDataSetKey,
  ]);

  // Reset the GeoJson layer if the geometry is changed,
  // updating the component doesn't do it once it's rendered
  useEffect(() => {
    if (geometryRef.current) {
      geometryRef.current.leafletElement.clearLayers();

      if (features) {
        geometryRef.current.leafletElement.addData(features);
      }
    }
  }, [features]);

  const updateSelectedFeature = useCallback(
    (feature: MapFeature) => {
      if (selectedFeature) {
        const element = selectedFeature?.properties.layer?.getElement();

        if (element) {
          element.classList.remove(styles.selected);
          element.removeAttribute('data-testid');
        }
      }

      setSelectedFeature(feature);

      const { layer } = feature.properties;

      if (!layer) {
        return;
      }

      layer.bringToFront();

      const element = layer.getElement();

      if (element) {
        element.classList.add(styles.selected);
        element.setAttribute('data-testid', 'mapBlock-selectedFeature');
      }

      if (mapRef.current) {
        // Centers the feature on the map
        mapRef.current.leafletElement.fitBounds(layer.getBounds());
      }
    },
    [selectedFeature],
  );

  const resetSelectedFeature = useCallback(() => {
    if (selectedFeature) {
      const element = selectedFeature?.properties.layer?.getElement();

      if (element) {
        element.classList.remove(styles.selected);
        element.removeAttribute('data-testid');
      }
      if (mapRef.current) {
        mapRef.current.leafletElement.setZoom(5);
      }

      setSelectedFeature(undefined);
    }
  }, [selectedFeature]);

  // We have to assign our `onEachFeature` callback to a ref
  // as `onEachFeature` forms an internal closure which
  // prevents us from updating the callback's dependencies.
  // This would otherwise lead to stale state and most likely
  // result in the callback throwing null pointer errors.
  const onEachFeature = useCallbackRef(
    (feature: MapFeature, featureLayer: Layer) => {
      if (feature.properties) {
        // eslint-disable-next-line no-param-reassign
        feature.properties.layer = featureLayer as MapFeatureProperties['layer'];
      }

      featureLayer.bindTooltip(() => {
        if (feature.properties.dataSets[selectedDataSetKey]) {
          const dataSetValue = formatPretty(
            feature.properties.dataSets[selectedDataSetKey].value,
            selectedDataSetConfig.dataSet.indicator.unit,
            selectedDataSetConfig.dataSet.indicator.decimalPlaces,
          );
          const content = `${selectedDataSetConfig.config.label}: ${dataSetValue}`;

          const mapWidth = mapRef.current?.container?.clientWidth;

          // Not ideal, we would want to use `max-width` instead.
          // Unfortunately it doesn't seem to work with the tooltip
          // for some reason (maybe due to the pane styling).
          const tooltipStyle = mapWidth ? `width: ${mapWidth / 2}px` : '';

          return (
            `<div class="${styles.tooltip}" style="${tooltipStyle}">` +
            `<p><strong data-testid="chartTooltip-label">${feature.properties.name}</strong></p>` +
            `<p class="${styles.tooltipContent}" data-testid="chartTooltip-contents">${content}</p>` +
            `</div>`
          );
        }

        return '';
      });
    },
    [dataSetCategoryConfigs, selectedDataSetKey],
  );

  if (
    data === undefined ||
    axisMajor === undefined ||
    axisMajor.dataSets === undefined
  ) {
    return <div>Unable to render map, map incorrectly configured</div>;
  }

  return (
    <>
      <form className="govuk-!-margin-bottom-2">
        <div className="govuk-grid-row">
          <FormGroup className="govuk-grid-column-two-thirds">
            <FormSelect
              name="selectedDataSet"
              id={`${id}-selectedDataSet`}
              className="govuk-!-width-full"
              label="1. Select data to view"
              value={selectedDataSetKey}
              onChange={e => setSelectedDataSetKey(e.currentTarget.value)}
              options={dataSetOptions}
              order={FormSelect.unordered}
            />
          </FormGroup>

          <FormGroup className="govuk-grid-column-one-third">
            <FormSelect
              name="selectedLocation"
              id={`${id}-selectedLocation`}
              label={`2. Select ${locationType.prefix} ${locationType.label}`}
              value={selectedFeature?.id?.toString()}
              options={locationOptions}
              optGroups={groupedLocationOptions}
              order={FormSelect.unordered}
              placeholder="None selected"
              onChange={e => {
                const feature = features?.features.find(
                  feat => feat.id === e.currentTarget.value,
                );
                if (feature) {
                  return updateSelectedFeature(feature);
                }
                return resetSelectedFeature();
              }}
            />
          </FormGroup>
        </div>
      </form>

      <div className="govuk-grid-row govuk-!-margin-bottom-4" ref={container}>
        <div className="govuk-grid-column-two-thirds">
          {ukGeometry && (
            <Map
              ref={mapRef}
              style={{
                width: (width && `${width}px`) || '100%',
                height: `${height || 600}px`,
              }}
              className={classNames(styles.map, 'dfe-print-break-avoid')}
              center={position}
              minZoom={5}
              zoom={5}
              whenReady={() => {
                mapRef.current?.leafletElement.setMaxBounds(
                  mapRef.current.leafletElement.getBounds(),
                );
              }}
            >
              <GeoJSON data={ukGeometry} className={styles.uk} ref={ukRef} />

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
                      fillColor: feature.properties.colour,
                    };
                  }}
                  onclick={e => {
                    const { feature } = e.sourceTarget;

                    if (feature.properties && feature.id) {
                      updateSelectedFeature(feature);
                    }
                  }}
                />
              )}
            </Map>
          )}
        </div>
        {selectedDataSetConfig && (
          <div className="govuk-grid-column-one-third">
            <h3 className="govuk-heading-s">
              Key to {selectedDataSetConfig?.config?.label}
            </h3>
            <ul className="govuk-list">
              {legendDataGroups.map(({ min, max, colour }) => (
                <li
                  key={`${min}-${max}-${colour}`}
                  className={styles.legend}
                  data-testid="mapBlock-legend-item"
                >
                  <span
                    className={styles.legendIcon}
                    data-testid="mapBlock-legend-colour"
                    style={{
                      backgroundColor: colour,
                    }}
                  />
                  {`${min} to ${max}`}
                </li>
              ))}
            </ul>

            <div
              aria-live="polite"
              className="govuk-!-margin-top-5"
              data-testid="mapBlock-indicator"
            >
              {selectedFeature && (
                <>
                  <h3 className="govuk-heading-s">
                    {selectedFeature?.properties.name}
                  </h3>

                  {selectedDataSet && (
                    <KeyStatTile
                      testId="mapBlock-indicatorTile"
                      title={selectedDataSetConfig.config.label}
                      value={formatPretty(
                        selectedDataSet.value,
                        selectedDataSetConfig.dataSet.indicator.unit,
                        selectedDataSetConfig.dataSet.indicator.decimalPlaces,
                      )}
                    />
                  )}
                </>
              )}
            </div>
          </div>
        )}
      </div>
    </>
  );
}

function generateFeaturesAndDataGroups({
  classification,
  customDataGroups,
  dataSetCategories,
  dataGroups: groups,
  selectedDataSetConfig,
}: {
  classification: DataClassification;
  customDataGroups: CustomDataGroup[];
  dataSetCategories: MapDataSetCategory[];
  dataGroups: number;
  selectedDataSetConfig: DataSetCategoryConfig;
}): {
  features: MapFeatureCollection;
  dataGroups: LegendDataGroup[];
} {
  const selectedDataSetKey = selectedDataSetConfig.dataKey;
  const { unit, decimalPlaces } = selectedDataSetConfig.dataSet.indicator;

  const colour =
    selectedDataSetConfig.config.colour ??
    generateHslColour(selectedDataSetConfig.dataKey);

  // Extract only the numeric values out of relevant data sets
  const values = dataSetCategories.reduce<number[]>((acc, category) => {
    const value = category.dataSets[selectedDataSetKey]?.value;

    if (Number.isFinite(value)) {
      acc.push(value);
    }

    return acc;
  }, []);

  const dataGroups = generateLegendDataGroups({
    colour,
    classification,
    customDataGroups,
    decimalPlaces,
    groups,
    values,
    unit,
  });

  // Default to white for areas not covered by custom data sets
  // to make it clearer which aren't covered by the groups.
  const defaultColour =
    classification === 'Custom' ? 'rgba(255, 255, 255, 1)' : 'rgba(0,0,0,0)';

  const features: MapFeatureCollection = {
    type: 'FeatureCollection',
    features: dataSetCategories.reduce<MapFeature[]>(
      (acc, { dataSets, filter, geoJson }) => {
        const value = dataSets?.[selectedDataSetKey]?.value;

        if (Number.isFinite(value)) {
          const matchingDataGroup = dataGroups.find(dataClass => {
            const roundedValue = Number(value.toFixed(dataClass.decimalPlaces));
            return (
              dataClass.minRaw <= roundedValue &&
              roundedValue <= dataClass.maxRaw
            );
          });

          acc.push({
            ...geoJson,
            id: filter.id,
            properties: {
              ...geoJson.properties,
              dataSets,
              // Default to transparent if no match
              colour: matchingDataGroup?.colour ?? defaultColour,
              data: value,
            },
          });
        }

        return acc;
      },
      [],
    ),
  };

  return { features, dataGroups };
}
