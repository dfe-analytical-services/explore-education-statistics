import Details from '@common/components/Details';
import {FormSelect} from '@common/components/form';
import {SelectOption} from '@common/components/form/FormSelect';
import {
  ChartDefinition,
  ChartProps,
  createDataForAxis,
  generateKeyFromDataSet,
  ChartDataB,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';

import {
  DataBlockData,
  DataBlockGeoJsonProperties,
  DataBlockMetadata,
} from '@common/services/dataBlockService';

import classNames from 'classnames';
import {Feature, FeatureCollection, Geometry} from 'geojson';

import {Layer, LeafletMouseEvent, Path, Polyline, PathOptions} from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React from 'react';
import {GeoJSON, LatLngBounds, Map} from 'react-leaflet';
import {DataSetConfiguration} from '@common/services/publicationService';
import {Dictionary} from '@common/types';
import styles from './MapBlock.module.scss';

type MapBlockProperties = DataBlockGeoJsonProperties & {
  scaledData: number;
  data: number;
  color: string;
  measures: string[];
  className: string;
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

//<editor-fold desc="Static functions">
function getLocationsForDataSet(
  data: DataBlockData,
  meta: DataBlockMetadata,
  chartData: ChartDataB[],
) {
  const allLocationIds = chartData.map(({name}) => name);

  return [
    {label: 'select...', value: ''},
    ...allLocationIds

    .reduce((locations: { label: string; value: string }[], next: string) => {
      const {label, value} = (meta.locations || {})[next];

      return [...locations, {label, value}];
    }, [])
    .sort((a, b) => {
      if (a.label < b.label) return -1;
      if (a.label > b.label) return 1;
      return 0;
    }),
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
    features: sourceData.map(({name, data, ...measures}) => ({
      ...meta.locations[name].geoJson[0],
      id: meta.locations[name].geoJson[0].properties.code,
      properties: {
        ...meta.locations[name].geoJson[0].properties,
        measures,
        color: selectedDataSet.colour,
        data: Number.parseFloat(data),
        scaledData: (Number.parseFloat(data) - min) * scale,
      },
    })),
  };
}

function calculateMinAndScaleForSourceData(sourceData: ChartDataB[]) {
  const {min, max} = sourceData.reduce(
    // eslint-disable-next-line no-shadow
    ({min, max}, {data}) => {
      const dataVal = Number.parseFloat(data);
      return {
        min: dataVal < min ? dataVal : min,
        max: dataVal > max ? dataVal : max,
      };
    },
    {min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY},
  );

  if (min === max) {
    return {min, scale: 0, range: 1};
  }

  const range = max - min;
  const scale = 1 / range; // / 5.0;

  return {min, max, range, scale};
}

function generateGeometryAndLegendForSelectedOptions(
  meta: DataBlockMetadata,
  labels: Dictionary<DataSetConfiguration>,
  chartData: ChartDataB[],
  selectedDataSet: string,
) {
  const sourceData = chartData
  .map(entry => ({...entry, data: entry[selectedDataSet]}))
  .filter(({data}) => data !== undefined);

  const {min, max, range, scale} = calculateMinAndScaleForSourceData(sourceData);

  console.log(min,max,range,scale);

  const legend: LegendEntry[] = [...Array(5)].map((_, idx) => {
    const i = idx / 4;

    const minValue = i;

    console.log(minValue);

    return {
      minValue,
      min: (min + i * range).toFixed(1),
      max: (min + (i + 0.25) * range).toFixed(1),
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

  return {geometry, legend};
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

function calculateColour({
  scaledData = 1.0,
  color = '#ff0000',
}) {
  const rescale = scaledData * 0.75 + 0.25;

  const scaled = [
    '#',
    ...(color.substr(1).match(/.{2}/g) || ['0', '0', 'ff']).map(subColour =>
      `0${Math.floor(Number.parseInt(subColour, 16) * rescale).toString(
        16,
      )}`.substr(-2),
    ),
  ].join('');

  return scaled;
}

//</editor-fold>

const MapBlock = ({
  data,
  meta,
  position = {lat: 53.00986, lng: -3.2524038},
  width,
  height,
  labels,
  axes,
}: MapProps) => {
  const mapRef = React.createRef<Map>();
  const geoJsonRef = React.createRef<GeoJSON>();
  const container = React.createRef<HTMLDivElement>();

  const [geometry, setGeometry] = React.useState<FeatureCollection<Geometry, DataBlockGeoJsonProperties>>();

  const [ukGeometry, setUkGeometry] = React.useState<FeatureCollection>();

  const intersectionObserver = React.useRef<IntersectionObserver>();

  const [dataSetOptions, setDataSetOptions] = React.useState<SelectOption[]>();

  const [majorOptions, setMajorOptions] = React.useState<SelectOption[]>([]);

  const [legend, setLegend] = React.useState<LegendEntry[]>([]);

  const [selectedDataSetIndex, setSelectedDataSetIndex] = React.useState<number>(0);
  const [selectedDataSetKey, setSelectedDataSetKey] = React.useState<string>(
    generateKeyFromDataSet(axes.major.dataSets[0], axes.major.groupBy),
  );

  const [selectedLocation, setSelectedLocation] = React.useState<string>('');

  const [results, setResults] = React.useState<IdValue[]>([]);

  const [chartData, setChartData] = React.useState<ChartDataB[]>([]);

  React.useEffect(() => {

    setDataSetOptions(axes.major.dataSets.map((dataSet, index) => {
      const dataKey = generateKeyFromDataSet(dataSet, axes.major.groupBy);

      return {...labels[dataKey], value: index};
    }));

  }, [axes.major.dataSets, axes.major.groupBy, labels]);

  React.useEffect(() => {

    if (geoJsonRef.current) {
      geoJsonRef.current.leafletElement.clearLayers();
      if (geometry) {
        geoJsonRef.current.leafletElement.addData(geometry);
      }
    }
  }, [geoJsonRef, geometry]);

  React.useEffect(() => {
    const {
      geometry: newGeometry,
      legend: newLegend,
    } = generateGeometryAndLegendForSelectedOptions(
      meta,
      labels,
      chartData,
      selectedDataSetKey,
    );

    setGeometry(newGeometry);
    setLegend(newLegend);
  }, [chartData, meta, labels, selectedDataSetKey]);

  React.useEffect(() => {
    const generatedChartData = createDataForAxis(axes.major, data.result, meta);
    setChartData(generatedChartData);

    setMajorOptions(getLocationsForDataSet(data, meta, generatedChartData));
  }, [data, axes, meta]);

  React.useEffect(() => {
    import('@common/services/UKGeoJson').then(imported => {
      setUkGeometry(imported.default);
    });

    if (container.current) {
      intersectionObserver.current = registerResizingCheck(
        container.current,
        () => {
          if (mapRef.current) {
            const {current} = mapRef;
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

  const onSelectIndicator = (selectedDatasetIndex: number) => {
    const selectedDataSet = axes.major.dataSets[selectedDatasetIndex];
    const newSelectedDataSetKey = generateKeyFromDataSet(selectedDataSet);

    const {
      geometry: newGeometry,
      legend: newLegend,
    } = generateGeometryAndLegendForSelectedOptions(
      meta,
      labels,
      chartData,
      newSelectedDataSetKey,
    );

    setGeometry(newGeometry);

    setLegend(newLegend);

    setMajorOptions(getLocationsForDataSet(data, meta, chartData));

    setSelectedDataSetIndex(selectedDatasetIndex);
    setSelectedDataSetKey(newSelectedDataSetKey);
  };

  const selectLocationOption = (locationValue: string) => {
    let calculatedResults: IdValue[] = [];

    const {element: currentSelectedLocationElement} = getFeatureElementById(
      selectedLocation,
      geometry,
    );

    if (currentSelectedLocationElement) {
      currentSelectedLocationElement.classList.remove(styles.selected);
    }

    const {
      layer: selectedLayer,
      element: selectedLocationElement,
      feature: selectedFeature,
    } = getFeatureElementById(locationValue, geometry);

    if (selectedLocationElement && selectedLayer && selectedFeature) {
      selectedLocationElement.classList.add(styles.selected);

      if (mapRef.current) {
        const polyLine: Polyline = selectedLayer as Polyline;

        mapRef.current.leafletElement.fitBounds(polyLine.getBounds(), {
          padding: [200, 200],
        });
        selectedLayer.bringToFront();
      }

      const {properties} = selectedFeature;

      if (properties) {
        // eslint-disable-next-line prefer-destructuring
        const measures: { [key: string]: string } = properties.measures;

        calculatedResults = Object.entries(measures).reduce(
          (r: IdValue[], [id, value]) => [...r, {id, value}],
          [],
        );
      }
    }

    setSelectedLocation(locationValue);

    setResults(calculatedResults);
  };

  const onEachFeature = (feature: MapFeature, featureLayer: Path) => {
    if (feature.properties) {
      // eslint-disable-next-line no-param-reassign
      feature.properties.layer = featureLayer;
    }

    featureLayer.setStyle({
      className: classNames(
        feature.properties && feature.properties.className,
        {[styles.selected]: feature.id === selectedLocation},
      ),
    });

    featureLayer.bindTooltip(() => {
      if (feature.properties) {
        const content = Object.entries(feature.properties.measures).map(
          ([id, value]) => `${labels[id].label} : ${value}${labels[id].unit}`,
        );

        if (feature.id) {
          content.unshift(
            `<strong>${(meta.locations || {})[feature.id].label}</strong>`,
          );
        }

        return content.join('<br />');
      }
      return '';
    });
  };

  const onClick = (e: MapClickEvent) => {
    const {feature} = e.sourceTarget;

    if (feature.properties) {
      selectLocationOption(feature.properties.code);
    }
  };

  if (
    data === undefined ||
    axes.major === undefined ||
    axes.major.dataSets === undefined ||
    axes.minor === undefined
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
              value={selectedDataSetIndex}
              onChange={e =>
                onSelectIndicator(Number.parseInt(e.currentTarget.value, 10))
              }
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
              onChange={e => selectLocationOption(e.currentTarget.value)}
              options={majorOptions}
              order={[]}
            />
          </div>
        </form>

        {results.length > 0 ? (
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
        ) : (
          ''
        )}

        {selectedDataSetKey && labels && labels[selectedDataSetKey] && (
          <div>
            <h3 className="govuk-heading-s">
              Key to {labels[selectedDataSetKey].label}
            </h3>
            <dl className="govuk-list">
              {legend &&
              legend.map(({min, max, idx, minValue}) => (
                <dd className={styles.legend} key={idx}>
                  <span
                    className={styles[`rate${idx}`]}
                    style={{
                      backgroundColor: calculateColour({
                        scaledData: minValue,
                        color: labels[selectedDataSetKey].colour
                      })
                    }}
                  >
                    &nbsp;
                  </span> {min}
                  {labels[selectedDataSetKey].unit}&nbsp; to {max}
                  {labels[selectedDataSetKey].unit}{' '}
                </dd>
              ))}
            </dl>
          </div>
        )}
      </div>

      <div className={classNames('govuk-grid-column-two-thirds')}>
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
          {ukGeometry && <GeoJSON data={ukGeometry} className={styles.uk} />}

          {geometry && (
            <GeoJSON
              ref={geoJsonRef}
              data={geometry}
              onEachFeature={onEachFeature}
              style={(feature?: MapFeature): PathOptions => ({
                fillColor:
                  feature &&
                  feature.properties &&
                  calculateColour(feature.properties),
                className: classNames({
                  [styles.selected]:
                  selectedDataSetIndex &&
                  feature &&
                  feature.id ===
                  axes.major.dataSets[selectedDataSetIndex].location,
                }),
              })}
              onclick={onClick}
            />
          )}
        </Map>
      </div>
    </div>
  );
};

const definition: ChartDefinition = {
  type: 'map',
  name: 'Geographic',

  capabilities: {
    dataSymbols: false,
    stackable: false,
    lineStyle: false,
    gridLines: false,
    canSize: true,
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
