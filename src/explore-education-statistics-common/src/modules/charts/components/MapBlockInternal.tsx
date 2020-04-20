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
import {
  GeoJsonFeature,
  GeoJsonFeatureProperties,
} from '@common/services/tableBuilderService';
import { Dictionary } from '@common/types';
import generateHslColour from '@common/utils/colour/generateHslColour';
import lighten from '@common/utils/colour/lighten';
import formatPretty from '@common/utils/number/formatPretty';
import getMinMax from '@common/utils/number/getMinMax';
import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Layer, LeafletMouseEvent, Path, PathOptions, Polyline } from 'leaflet';
import keyBy from 'lodash/keyBy';
import times from 'lodash/times';
import React, { useEffect, useMemo, useRef, useState } from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';

interface MapFeatureProperties extends GeoJsonFeatureProperties {
  colour: string;
  data: number;
  dataSets: DataSetCategory['dataSets'];
  scaledData: number;
  layer?: Layer & Path & Polyline;
}

export type MapFeature = Feature<Geometry, MapFeatureProperties>;

type MapFeatureCollection = FeatureCollection<Geometry, MapFeatureProperties>;

interface LegendEntry {
  colour: string;
  min: string;
  max: string;
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

function calculateColour({
  scaledData,
  colour,
}: {
  scaledData: number;
  colour: string;
}) {
  return lighten(colour, 90 - (scaledData / 0.2) * 30);
}

function generateGeometryAndLegend(
  selectedDataSetConfiguration: CategoryDataSetConfiguration,
  dataSetCategories: MapDataSetCategory[],
): {
  geometry: MapFeatureCollection;
  legend: LegendEntry[];
} {
  const selectedDataSetKey = selectedDataSetConfiguration.dataKey;

  const { min = 0, max = 0 } = getMinMax(
    dataSetCategories.map(
      category => category.dataSets[selectedDataSetKey].value,
    ),
  );

  const range = max - min;

  const colour =
    selectedDataSetConfiguration.config.colour ??
    generateHslColour(selectedDataSetConfiguration.dataKey);

  const {
    unit,
    decimalPlaces,
  } = selectedDataSetConfiguration.dataSet.indicator;

  const legend: LegendEntry[] =
    range > 0
      ? times(5, idx => {
          const i = idx / 4;

          return {
            colour: calculateColour({ scaledData: i, colour }),
            min: formatPretty(min + i * range, unit, decimalPlaces),
            max: formatPretty(min + (i + 0.25) * range, unit, decimalPlaces),
          };
        })
      : [
          {
            colour,
            min: formatPretty(min, unit, decimalPlaces),
            max: formatPretty(max, unit, decimalPlaces),
          },
        ];

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
          colour,
          data,
          scaledData: (data - min) / range,
        },
      };
    }),
  };

  return { geometry, legend };
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
      } = generateGeometryAndLegend(
        selectedDataSetConfiguration,
        dataSetCategories,
      );

      if (newGeometry && newLegend) {
        setGeometry(newGeometry);
        setLegend(newLegend);
      }
    }
  }, [
    dataSetCategories,
    dataSetConfigurations,
    meta,
    selectedDataSetConfiguration,
    selectedDataSetKey,
  ]);

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
      <form className="govuk-!-margin-bottom-2">
        <FormFieldset
          id="data-and-location"
          legend="Select data and location to view indicators"
          legendHidden
        >
          <div className="govuk-grid-row">
            <FormGroup className="govuk-grid-column-two-thirds">
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

            <FormGroup className="govuk-grid-column-one-third">
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
        {selectedDataSetConfiguration && (
          <div className="govuk-grid-column-one-third" aria-live="assertive">
            <h3 className="govuk-heading-s">
              Key to {selectedDataSetConfiguration?.config?.label}
            </h3>
            <ul className="govuk-list">
              {legend.map(({ min, max, colour }, index) => (
                // eslint-disable-next-line react/no-array-index-key
                <li className={styles.legend} key={index}>
                  <span
                    className={styles.legendIcon}
                    style={{
                      backgroundColor: colour,
                    }}
                  />
                  {`${min} to ${max}`}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {selectedDataSetConfiguration && selectedFeature && (
        <>
          <h3 className="govuk-heading-m govuk-!-margin-left-1 govuk-!-margin-bottom-0">
            {selectedFeature?.properties.name}
          </h3>

          {selectedFeature?.properties?.dataSets && selectedDataSetKey && (
            <div className={stylesIndicators.keyStatsContainer}>
              {Object.entries(selectedFeature?.properties.dataSets).map(
                ([dataSetKey, dataSet]) => {
                  if (!dataSetConfigurations[dataSetKey]) {
                    return null;
                  }

                  const {
                    config,
                    dataSet: expandedDataSet,
                  } = dataSetConfigurations[dataSetKey];

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
                            {formatPretty(
                              dataSet.value,
                              expandedDataSet.indicator.unit,
                            )}
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
