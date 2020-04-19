import { FormFieldset, FormGroup, FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import useIntersectionObserver from '@common/hooks/useIntersectionObserver';
import styles from '@common/modules/charts/components/MapBlock.module.scss';
import {
  AxisConfiguration,
  ChartProps,
} from '@common/modules/charts/types/chart';
import { DataSetCategory } from '@common/modules/charts/types/dataSet';
import createDataSetCategories from '@common/modules/charts/util/createDataSetCategories';
import getCategoryDataSetConfigurations, {
  CategoryDataSetConfiguration,
} from '@common/modules/charts/util/getCategoryDataSetConfigurations';
import stylesIndicators from '@common/modules/find-statistics/components/KeyStatTile.module.scss';
import { FullTableMeta } from '@common/modules/table-tool/types/fullTable';
import {
  GeoJsonFeature,
  GeoJsonFeatureProperties,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import formatPretty from '@common/utils/number/formatPretty';
import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Layer, LeafletMouseEvent, Path, PathOptions, Polyline } from 'leaflet';
import keyBy from 'lodash/keyBy';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';

interface MapFeatureProperties extends GeoJsonFeatureProperties {
  data: number;
  scaledData: number;
  dataSets: DataSetCategory['dataSets'];
  color?: string;
  layer?: Layer & Path & Polyline;
}

export type MapFeature = Feature<Geometry, MapFeatureProperties>;

type MapFeatureCollection = FeatureCollection<Geometry, MapFeatureProperties>;

interface LegendEntry {
  minValue: number;
  min: string;
  max: string;
  idx: number;
}

interface MapClickEvent extends LeafletMouseEvent {
  layer: Layer;
  sourceTarget: {
    feature: MapFeature;
  };
}

interface MapDataSetCategory extends DataSetCategory {
  geoJson: GeoJsonFeature;
}

function calculateMinAndScaleForSourceData(
  mapData: MapDataSetCategory[],
  selectedDataSetKey: string,
) {
  const minMax = mapData.reduce(
    ({ min, max }, category) => {
      const { value } = category.dataSets[selectedDataSetKey];
      return {
        min: value < min ? value : min,
        max: value > max ? value : max,
      };
    },
    { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
  );

  const { min, max } = minMax;

  if (min === max) {
    return { min, max: min, scale: 0, range: 1 };
  }

  const range = Math.abs(max - min);
  const scale = 1 / range;

  return { min, max, range, scale };
}

function generateGeometryAndLegendForSelectedOptions(
  selectedDataSetKey: string,
  dataSetConfigurationsByKey: Dictionary<CategoryDataSetConfiguration>,
  meta: FullTableMeta,
  dataSetCategories: MapDataSetCategory[],
): {
  geometry: MapFeatureCollection;
  legend: LegendEntry[];
} {
  const { min, max, range, scale } = calculateMinAndScaleForSourceData(
    dataSetCategories,
    selectedDataSetKey,
  );

  let fixedScale = Math.log10((max - min) / 5);

  let numberFormatter: (n: number) => string;
  if (fixedScale < 0 && Number.isFinite(fixedScale)) {
    fixedScale = -Math.floor(fixedScale);
    numberFormatter = n => n.toFixed(fixedScale);
  } else {
    numberFormatter = n =>
      n.toLocaleString(undefined, { maximumFractionDigits: 0 });
  }

  const legend: LegendEntry[] = [...Array(5)].map((_, idx) => {
    const i = idx / 4;

    return {
      minValue: i,
      min: numberFormatter(min + i * range),
      max: numberFormatter(min + (i + 0.25) * range),
      idx,
    };
  });

  const geometry: FeatureCollection<Geometry, MapFeatureProperties> = {
    type: 'FeatureCollection',
    features: dataSetCategories.map(({ dataSets, filter, geoJson }) => {
      const data = dataSets?.[selectedDataSetKey]?.value ?? 0;

      return {
        ...geoJson,
        id: filter.id,
        properties: {
          ...geoJson.properties,
          dataSets,
          color: dataSetConfigurationsByKey[selectedDataSetKey]?.config?.colour,
          data,
          scaledData: (data - min) * scale,
        },
      };
    }),
  };

  return { geometry, legend };
}

function calculateColour({ scaledData = 1.0, color = '#ff0000' }) {
  const rescale = 1 - (scaledData * 0.75 + 0.25);

  return [
    '#',
    ...(color.substr(1).match(/.{2}/g) || ['0', '0', 'ff']).map(subColour =>
      `0${Math.floor(Number.parseInt(subColour, 16) * rescale).toString(
        16,
      )}`.substr(-2),
    ),
  ].join('');
}

export interface MapBlockInternalProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
  geographicId?: string;
  axes: {
    major: AxisConfiguration;
  };
}

export const MapBlockInternal = ({
  data,
  meta,
  position = { lat: 53.00986, lng: -3.2524038 },
  width,
  height,
  axes,
}: MapBlockInternalProps) => {
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

  const dataSetCategories = useMemo<MapDataSetCategory[]>(() => {
    return createDataSetCategories(axisMajor, data, meta)
      .map(category => {
        return {
          ...category,
          geoJson: meta.locations.find(
            location => location.id === category.filter.id,
          )?.geoJson?.[0],
        };
      })
      .filter(category => !!category?.geoJson) as MapDataSetCategory[];
  }, [axisMajor, data, meta]);

  const dataSetConfigurations = useMemo<
    Dictionary<CategoryDataSetConfiguration>
  >(
    () =>
      keyBy(
        getCategoryDataSetConfigurations(dataSetCategories, axisMajor, meta),
        dataSetConfig => dataSetConfig.dataKey,
      ),
    [axisMajor, dataSetCategories, meta],
  );

  const dataSetOptions = useMemo<SelectOption[]>(() => {
    return Object.values(dataSetConfigurations).map(dataSet => ({
      label: dataSet.config.label,
      value: dataSet.dataKey,
    }));
  }, [dataSetConfigurations]);

  const locationOptions = useMemo(() => {
    return dataSetCategories.map(dataSetCategory => ({
      label: dataSetCategory.filter.label,
      value: dataSetCategory.filter.id,
    }));
  }, [dataSetCategories]);

  const [selectedDataSetKey, setSelectedDataSetKey] = useState<string>(
    (dataSetOptions[0]?.value as string) ?? '',
  );
  const [selectedFeature, setSelectedFeature] = useState<MapFeature>();

  const [geometry, setGeometry] = useState<MapFeatureCollection>();
  const [ukGeometry, setUkGeometry] = useState<FeatureCollection>();

  const [legend, setLegend] = useState<LegendEntry[]>([]);

  const selectedDataSetConfiguration =
    dataSetConfigurations[selectedDataSetKey];

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
    if (dataSetCategories.length && selectedDataSetConfiguration) {
      const {
        geometry: newGeometry,
        legend: newLegend,
      } = generateGeometryAndLegendForSelectedOptions(
        selectedDataSetKey,
        dataSetConfigurations,
        meta,
        dataSetCategories,
      );

      if (newGeometry && newLegend) {
        setGeometry(newGeometry);
        setLegend(newLegend);
      }
    }
  }, [dataSetCategories, dataSetConfigurations, meta, selectedDataSetKey]);

  // Reset the GeoJson layer if the geometry is changed,
  // updating the component doesn't do it once it's rendered
  useEffect(() => {
    if (geometryRef.current) {
      geometryRef.current.leafletElement.clearLayers();

      if (geometry) {
        geometryRef.current.leafletElement.addData(geometry);
      }
    }
  }, [geometry]);

  const updateSelectedFeature = (feature: MapFeature) => {
    if (selectedFeature) {
      const element = selectedFeature?.properties.layer?.getElement();

      if (element) {
        element.classList.remove(styles.selected);
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
    }

    if (mapRef.current) {
      // Centers the feature on the map
      mapRef.current.leafletElement.fitBounds(layer.getBounds());
    }
  };

  const handleFeatureClick = (e: MapClickEvent) => {
    const { feature } = e.sourceTarget;

    if (feature.properties && feature.id) {
      updateSelectedFeature(feature);
    }
  };

  if (
    data === undefined ||
    axisMajor === undefined ||
    axisMajor.dataSets === undefined
  ) {
    return <div>Unable to render map, map incorrectly configured</div>;
  }

  return (
    <>
      <form>
        <FormFieldset
          id="data-and-location"
          legend="Select data and location to view indicators"
          legendHidden
        >
          <div className={styles.mapContainer}>
            <FormGroup className={styles.mapIndicator}>
              <FormSelect
                name="selectedIndicator"
                id="selectedIndicator"
                className="govuk-!-width-full"
                label="1. Select data to view"
                value={selectedDataSetKey}
                onChange={e => setSelectedDataSetKey(e.currentTarget.value)}
                options={dataSetOptions}
              />
            </FormGroup>

            <FormGroup className={styles.mapLocation}>
              <FormSelect
                name="selectedLocation"
                id="selectedLocation"
                label="2. Select a location"
                value={selectedFeature?.id}
                placeholder="Select location"
                options={locationOptions}
                onChange={e => {
                  const feature = geometry?.features.find(
                    feat => feat.id === e.currentTarget.value,
                  );

                  if (feature) {
                    updateSelectedFeature(feature);
                  }
                }}
              />
            </FormGroup>
          </div>
        </FormFieldset>
      </form>
      <div className={styles.mapContainer} ref={container}>
        <div className={styles.mapView}>
          {ukGeometry && (
            <Map
              ref={mapRef}
              style={{
                width: (width && `${width}px`) || '100%',
                height: `${height || 600}px`,
              }}
              className={classNames(styles.map, 'dfe-print-break-avoid')}
              center={position}
              zoom={6.5}
            >
              <GeoJSON data={ukGeometry} className={styles.uk} ref={ukRef} />

              {geometry && (
                <GeoJSON
                  ref={geometryRef}
                  data={geometry}
                  onEachFeature={(feature: MapFeature, featureLayer: Layer) => {
                    if (feature.properties) {
                      // eslint-disable-next-line no-param-reassign
                      feature.properties.layer = featureLayer as MapFeatureProperties['layer'];
                    }

                    featureLayer.bindTooltip(() => {
                      if (feature.properties) {
                        const content = [
                          `<strong>${feature.properties.name}</strong>`,
                          ...Object.entries(feature.properties.dataSets).map(
                            ([dataSetKey, dataSet]) => {
                              const dataSetConfig =
                                dataSetConfigurations[dataSetKey];

                              return `${
                                dataSetConfig.config.label
                              } : ${formatPretty(
                                dataSet.value,
                                dataSetConfig.dataSet.indicator.unit,
                              )}`;
                            },
                          ),
                        ];

                        return content.join('<br />');
                      }

                      return '';
                    });
                  }}
                  style={(feature?: MapFeature): PathOptions => {
                    if (!feature) {
                      return {};
                    }

                    return {
                      fillColor: calculateColour(feature.properties),
                    };
                  }}
                  onclick={handleFeatureClick}
                />
              )}
            </Map>
          )}
        </div>
        <div className={styles.mapControls} aria-live="assertive">
          {selectedDataSetKey && selectedDataSetConfiguration && (
            <div className={styles.mapKey}>
              <h3 className="govuk-heading-s">
                Key to {selectedDataSetConfiguration?.config?.label}
              </h3>
              <dl className="govuk-list">
                {legend?.map(({ min, max, idx, minValue }) => (
                  <dd className={styles.legend} key={idx}>
                    <span
                      className={styles[`rate${idx}`]}
                      style={{
                        backgroundColor: calculateColour({
                          scaledData: minValue,
                          color: selectedDataSetConfiguration?.config?.colour,
                        }),
                      }}
                    >
                      &nbsp;
                    </span>
                    {` ${formatPretty(
                      min,
                      selectedDataSetConfiguration?.config?.unit,
                      selectedDataSetConfiguration?.config.decimalPlaces,
                    )} to ${formatPretty(
                      max,
                      selectedDataSetConfiguration?.config?.unit,
                      selectedDataSetConfiguration?.config.decimalPlaces,
                    )}`}
                  </dd>
                ))}
              </dl>
            </div>
          )}
        </div>
      </div>

      {selectedDataSetConfiguration && selectedFeature && (
        <>
          <h3 className="govuk-heading-m govuk-!-margin-left-1 govuk-!-margin-bottom-0">
            {selectedFeature?.properties.name}
          </h3>

          {selectedFeature?.properties?.dataSets && (
            <div className={stylesIndicators.keyStatsContainer}>
              {Object.keys(selectedFeature?.properties.dataSets).map(
                dataSetKey => {
                  const {
                    config,
                    dataSet,
                    value,
                  } = selectedDataSetConfiguration;

                  return (
                    <div
                      key={dataSetKey}
                      className={stylesIndicators.keyStatTile}
                    >
                      <div className={stylesIndicators.keyStat}>
                        <h4 className="govuk-heading-s">{config.label}</h4>
                        <p
                          className="govuk-heading-xl govuk-!-margin-bottom-2"
                          aria-label={config.label}
                        >
                          <span>
                            {formatPretty(value, dataSet.indicator.unit)}
                          </span>
                        </p>
                      </div>
                    </div>
                  );
                },
              )}
            </div>
          )}
        </>
      )}
    </>
  );
};

export default MapBlockInternal;
