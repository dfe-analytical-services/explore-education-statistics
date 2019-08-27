import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
import {
  ChartDataB,
  ChartDefinition,
  ChartProps,
  createSortedAndMappedDataForAxis,
  generateKeyFromDataSet,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import {
  DataBlockData,
  DataBlockGeoJsonProperties,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import {
  AxisGroupBy,
  ChartDataSet,
  DataSetConfiguration,
} from '@common/services/publicationService';
import { Dictionary } from '@common/types';

import classNames from 'classnames';
import { Feature, FeatureCollection, Geometry } from 'geojson';

import { Layer, LeafletMouseEvent, Path, PathOptions, Polyline } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';
import styles from './MapBlock.module.scss';

type MapBlockProperties = DataBlockGeoJsonProperties & {
  scaledData: number;
  data: number;
  color: string;
  measures: string[];
  className: string;
  layer: Layer;
};

export type MapFeature = Feature<Geometry, MapBlockProperties>;

interface MapProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
}

interface IdValue {
  id: string;
  value: string;
}

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

// <editor-fold desc=" Static functions">
function getLocationsForDataSet(
  data: DataBlockData,
  meta: DataBlockMetadata,
  chartData: ChartDataB[],
) {
  const allLocationIds = chartData.map(({ __name }) => __name);

  return [
    { label: 'select...', value: '' },
    ...allLocationIds.reduce(
      (locations: { label: string; value: string }[], next: string) => {
        const { label, value } = (meta.locations || {})[next];

        return [...locations, { label, value }];
      },
      [],
    ),
    /*
      .sort((a, b) => {
        if (a.label < b.label) return -1;
        if (a.label > b.label) return 1;
        return 0;
      }),
       */
  ];
}

function getGeometryForOptions(
  meta: DataBlockMetadata,
  selectedDataSet: DataSetConfiguration,
  sourceData: ChartDataB[],
  min: number,
  scale: number,
): FeatureCollection<Geometry, DataBlockGeoJsonProperties> {
  return {
    type: 'FeatureCollection',
    features: sourceData.map(({ __name: id, name: _, data, ...measures }) => {
      return {
        ...meta.locations[id].geoJson[0],
        id: meta.locations[id].geoJson[0].properties.code,
        properties: {
          ...meta.locations[id].geoJson[0].properties,
          measures,
          color: selectedDataSet.colour,
          data: Number.parseFloat(data),
          scaledData: (Number.parseFloat(data) - min) * scale,
        },
      };
    }),
  };
}

function calculateMinAndScaleForSourceData(sourceData: ChartDataB[]) {
  const { min, max } = sourceData.reduce(
    // eslint-disable-next-line no-shadow
    ({ min, max }, { data }) => {
      const dataVal = Number.parseFloat(data);
      return {
        min: dataVal < min ? dataVal : min,
        max: dataVal > max ? dataVal : max,
      };
    },
    { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
  );

  if (min === max) {
    return { min, max: min, scale: 0, range: 1 };
  }

  const range = max - min;
  const scale = 1 / range; // / 5.0;

  return { min, max, range, scale };
}

function generateGeometryAndLegendForSelectedOptions(
  meta: DataBlockMetadata,
  labels: Dictionary<DataSetConfiguration>,
  chartData: ChartDataB[],
  selectedDataSet: string,
) {
  const sourceData = chartData
    .map<ChartDataB>(entry => ({ ...entry, data: entry[selectedDataSet] }))
    .filter(
      ({ data, __name: id }) =>
        data !== undefined && meta.locations[id] && meta.locations[id].geoJson,
    );

  const { min, max, range, scale } = calculateMinAndScaleForSourceData(
    sourceData,
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

  const geometry = getGeometryForOptions(
    meta,
    labels[selectedDataSet],
    sourceData,
    min,
    scale,
  );

  return { geometry, legend };
}

function registerResizingCheck(
  container: HTMLDivElement,
  callback: () => void,
): IntersectionObserver {
  const intersectionObserver = new IntersectionObserver(
    entries => {
      if (entries.length > 0) {
        if (entries[0].intersectionRatio > 0) {
          callback();
        }
      }
    },
    {
      threshold: 0.00001,
    },
  );

  intersectionObserver.observe(container);
  return intersectionObserver;
}

function getFeatureElementById(
  id: string,
  geometry?: FeatureCollection<Geometry, DataBlockGeoJsonProperties>,
): { element?: Element; layer?: Path; feature?: Feature } {
  if (geometry) {
    const selectedFeature = geometry.features.find(
      feature => feature.id === id,
    );

    if (selectedFeature) {
      const selectedLayer: Path = selectedFeature.properties.layer as Path;

      return {
        element: selectedLayer.getElement(),
        layer: selectedLayer,
        feature: selectedFeature,
      };
    }
  }

  return {};
}

function calculateColour({ scaledData = 1.0, color = '#ff0000' }) {
  const rescale = scaledData * 0.75 + 0.25;

  return [
    '#',
    ...(color.substr(1).match(/.{2}/g) || ['0', '0', 'ff']).map(subColour =>
      `0${Math.floor(Number.parseInt(subColour, 16) * rescale).toString(
        16,
      )}`.substr(-2),
    ),
  ].join('');
}

// this is the most annoying painful way of having to make this work!!!
// using a reference to the callback to make the onEachFeature callback work
// otherwise it ends up creating a closure that has no access to updated properties?
// calling the referenced callback and rebuilding it when the data it uses changes
function useCallbackRef<T extends (...args: never[]) => unknown>(
  callback: () => T,
  deps: unknown[] | undefined,
) {
  const ref = React.useRef<T>();

  React.useEffect(() => {
    ref.current = callback();
  }, deps); // eslint-disable-line

  return ref;
}

// </editor-fold>

// <editor-fold desc="Data building functions">

function generateDataOptions(
  dataSets: ChartDataSet[],
  labels: Dictionary<DataSetConfiguration>,
  groupBy?: AxisGroupBy,
) {
  return dataSets.map(dataSet => {
    const dataKey = generateKeyFromDataSet(dataSet, groupBy);
    return { ...labels[dataKey], value: dataKey };
  });
}

// </editor-fold>

const MapBlock = ({
  data,
  meta,
  position = { lat: 53.00986, lng: -3.2524038 },
  width,
  height,
  labels,
  axes,
}: MapProps) => {
  const mapRef = React.createRef<Map>();
  const geoJsonRef = React.createRef<GeoJSON>();
  const container = React.createRef<HTMLDivElement>();
  const ukRef = React.createRef<GeoJSON>();

  const [geometry, setGeometry] = React.useState<
    FeatureCollection<Geometry, DataBlockGeoJsonProperties>
  >();

  const [ukGeometry, setUkGeometry] = React.useState<FeatureCollection>();

  const intersectionObserver = React.useRef<IntersectionObserver>();

  const [dataSetOptions, setDataSetOptions] = React.useState<SelectOption[]>();

  const [majorOptions, setMajorOptions] = React.useState<SelectOption[]>([]);

  const [legend, setLegend] = React.useState<LegendEntry[]>([]);

  const [selectedDataSetKey, setSelectedDataSetKey] = React.useState<string>();

  const [selectedLocation, setSelectedLocation] = React.useState<string>('');

  const [results, setResults] = React.useState<IdValue[]>([]);

  const [chartData, setChartData] = React.useState<ChartDataB[]>();

  // initialise
  React.useEffect(() => {
    import('@common/services/UKGeoJson').then(imported => {
      setUkGeometry(imported.default);
    });
  }, []);

  // resize handler
  React.useLayoutEffect(() => {
    if (container.current) {
      intersectionObserver.current = registerResizingCheck(
        container.current,
        () => {
          if (mapRef.current) {
            const { current } = mapRef;
            requestAnimationFrame(() => {
              current.leafletElement.invalidateSize();
            });
          }
        },
      );
    }

    return () => {
      if (intersectionObserver.current !== undefined) {
        intersectionObserver.current.disconnect();
      }
    };
  }, [container, mapRef]);

  // initialise on prop changes
  React.useEffect(() => {
    const generatedChartData = createSortedAndMappedDataForAxis(
      axes.major,
      data.result,
      meta,
      labels,
      true,
    ).filter(
      ({ __name: id }) => meta.locations[id] && meta.locations[id].geoJson,
    );

    setChartData(generatedChartData);

    setMajorOptions(getLocationsForDataSet(data, meta, generatedChartData));
  }, [data, axes.major, meta, labels]);

  React.useEffect(() => {
    if (
      selectedDataSetKey === undefined ||
      labels[selectedDataSetKey] === undefined
    ) {
      setSelectedDataSetKey(
        generateKeyFromDataSet(axes.major.dataSets[0], axes.major.groupBy),
      );
    }
  }, [axes.major.dataSets, axes.major.groupBy, labels, selectedDataSetKey]);

  // update settings for the data sets
  React.useEffect(() => {
    setDataSetOptions(
      generateDataOptions(axes.major.dataSets, labels, axes.major.groupBy),
    );
  }, [axes.major.dataSets, axes.major.groupBy, labels]);

  // force a refresh of the leaflet element if width or height are changed
  React.useEffect(() => {
    if (mapRef.current) {
      mapRef.current.leafletElement.invalidateSize();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [width, height]);

  // Rebuild the geometry if the selection has changed
  React.useEffect(() => {
    if (chartData && selectedDataSetKey && labels[selectedDataSetKey]) {
      const {
        geometry: newGeometry,
        legend: newLegend,
      } = generateGeometryAndLegendForSelectedOptions(
        meta,
        labels,
        chartData,
        selectedDataSetKey,
      );

      if (newGeometry && newLegend) {
        setGeometry(newGeometry);
        setLegend(newLegend);
      }
    }
  }, [chartData, meta, labels, selectedDataSetKey]);

  // callbacks for the Leaflet element
  const onEachFeatureCallback = useCallbackRef(
    () => (feature: MapFeature, featureLayer: Layer) => {
      if (feature.properties) {
        // eslint-disable-next-line no-param-reassign
        feature.properties.layer = featureLayer;
      }

      const featurePath: Path = featureLayer as Path;
      featurePath.setStyle({
        className: classNames(
          feature.properties && feature.properties.className,
          { [styles.selected]: feature.id === selectedLocation },
        ),
      });

      featureLayer.bindTooltip(() => {
        if (feature.properties) {
          const content = Object.entries(feature.properties.measures)
            .map(([id, value]) => ({
              ...(labels[id] || { label: '', unit: '' }),
              value,
            }))
            .map(({ label, value, unit }) => `${label} : ${value}${unit}`);

          if (feature.id) {
            content.unshift(
              `<strong>${(meta.locations || {})[feature.id].label}</strong>`,
            );
          }

          return content.join('<br />');
        }
        return '';
      });
    },
    [labels, meta.locations, selectedLocation],
  );

  const updateSelectedLocation = (
    newSelectedLocation: string,
    panTo: boolean = true,
  ) => {
    const oldSelectedLocation = selectedLocation;

    if (oldSelectedLocation) {
      const { element: oldSelectedLocationElement } = getFeatureElementById(
        oldSelectedLocation,
        geometry,
      );

      if (oldSelectedLocationElement) {
        oldSelectedLocationElement.classList.remove(styles.selected);
      }
    }

    if (oldSelectedLocation !== newSelectedLocation) {
      let calculatedResults: IdValue[] = [];

      const {
        layer: selectedLayer,
        element: selectedLocationElement,
        feature: selectedFeature,
      } = getFeatureElementById(newSelectedLocation, geometry);

      if (selectedLocationElement && selectedLayer && selectedFeature) {
        selectedLayer.bringToFront();
        selectedLocationElement.classList.add(styles.selected);

        if (mapRef.current && panTo) {
          const polyLine: Polyline = selectedLayer as Polyline;
          mapRef.current.leafletElement.fitBounds(polyLine.getBounds());
        }

        const { properties } = selectedFeature;

        if (properties) {
          // eslint-disable-next-line prefer-destructuring
          const measures: { [key: string]: string } = properties.measures;

          calculatedResults = Object.entries(measures).reduce(
            (r: IdValue[], [id, value]) => [...r, { id, value }],
            [],
          );
        }
      }

      setResults(calculatedResults);
    }

    setSelectedLocation(newSelectedLocation);
  };

  const onClick = (e: MapClickEvent) => {
    const { feature } = e.sourceTarget;

    if (feature.properties && feature.properties.code) {
      updateSelectedLocation(feature.properties.code);
    }
  };

  // reset the GeoJson layer if the geometry is changed, updating the component doesn't do it once it's rendered
  React.useEffect(() => {
    if (geoJsonRef.current) {
      geoJsonRef.current.leafletElement.clearLayers();

      if (geometry) {
        geoJsonRef.current.leafletElement.addData(geometry);
      }
    }
    // DO NOT ADD THE geoJsonRef as a dependency, it breaks the click event handler
    // eslint-disable-next-line
  }, [geometry]);

  if (
    data === undefined ||
    axes.major === undefined ||
    axes.major.dataSets === undefined
  )
    return <div>An error occurred</div>;

  return (
    <div className="govuk-grid-row" ref={container}>
      <div
        className={classNames('govuk-grid-column-one-third')}
        aria-live="assertive"
      >
        <form>
          <div className="govuk-form-group govuk-!-margin-bottom-6">
            <FormSelect
              name="selectedIndicator"
              id="selectedIndicator"
              label="Select data to view"
              value={selectedDataSetKey}
              onChange={e => setSelectedDataSetKey(e.currentTarget.value)}
              options={dataSetOptions}
              order={[]}
            />
          </div>

          <div className="govuk-form-group govuk-!-margin-bottom-6">
            <FormSelect
              name="selectedLocation"
              id="selectedLocation"
              label="Select a location"
              value={selectedLocation}
              onChange={e => updateSelectedLocation(e.currentTarget.value)}
              options={majorOptions}
              order={[]}
            />
          </div>
        </form>

        {results.length > 0 && (
          <div>
            {results.map(result => (
              <div
                key={result.id}
                className="dfe-dash-tiles__tile govuk-!-margin-bottom-6"
              >
                <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                  {labels[result.id].label}
                </h3>
                <p
                  className="govuk-heading-xl govuk-!-margin-bottom-2"
                  aria-label={labels[result.id].label}
                >
                  <span>{` ${result.value}${labels[result.id].unit} `}</span>
                </p>
                <Details summary={`What is ${labels[result.id].label}?`}>
                  Description for {labels[result.id].label}
                </Details>
              </div>
            ))}
          </div>
        )}

        {selectedDataSetKey && labels && labels[selectedDataSetKey] && (
          <div>
            <h3 className="govuk-heading-s">
              Key to {labels[selectedDataSetKey].label}
            </h3>
            <dl className="govuk-list">
              {legend &&
                legend.map(({ min, max, idx, minValue }) => (
                  <dd className={styles.legend} key={idx}>
                    <span
                      className={styles[`rate${idx}`]}
                      style={{
                        backgroundColor: calculateColour({
                          scaledData: minValue,
                          color: labels[selectedDataSetKey].colour,
                        }),
                      }}
                    >
                      &nbsp;
                    </span>{' '}
                    {min}
                    {labels[selectedDataSetKey].unit}&nbsp; to {max}
                    {labels[selectedDataSetKey].unit}{' '}
                  </dd>
                ))}
            </dl>
          </div>
        )}
      </div>

      <div className={classNames('govuk-grid-column-two-thirds')}>
        {geometry && ukGeometry && (
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

            <GeoJSON
              ref={geoJsonRef}
              data={geometry}
              onEachFeature={(feature, layer) =>
                onEachFeatureCallback.current &&
                onEachFeatureCallback.current(feature, layer)
              }
              style={(feature?: MapFeature): PathOptions => ({
                fillColor:
                  feature &&
                  feature.properties &&
                  calculateColour(feature.properties),
                className: classNames({
                  [styles.selected]: feature && feature.id === selectedLocation,
                }),
              })}
              onclick={onClick}
            />
          </Map>
        )}
      </div>
    </div>
  );
};

const definition: ChartDefinition = {
  type: 'map',
  name: 'Geographic',

  height: 600,

  capabilities: {
    dataSymbols: false,
    stackable: false,
    lineStyle: false,
    gridLines: false,
    canSize: true,
    fixedAxisGroupBy: true,
    hasAxes: false,
    hasReferenceLines: false,
  },

  data: [
    {
      type: 'geojson',
      title: 'Geographic',
      entryCount: 'multiple',
      targetAxis: 'geojson',
    },
  ],

  axes: [
    {
      id: 'geojson',
      title: 'geojson',
      type: 'major',
      forcedDataType: 'locations',
    },
  ],
};

MapBlock.definition = definition;

export default MapBlock;
