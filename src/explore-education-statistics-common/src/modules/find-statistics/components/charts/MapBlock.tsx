import Details from '@common/components/Details';
import {FormSelect} from '@common/components/form';
import {SelectOption} from '@common/components/form/FormSelect';
import {
  ChartDefinition,
  ChartProps,
} from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  DataBlockData,
  DataBlockGeoJsonProperties,
  DataBlockLocation,
  DataBlockLocationMetadata,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import {Dictionary} from '@common/types/util';
import classNames from 'classnames';
import {
  Feature,
  FeatureCollection,
  GeoJsonProperties,
  Geometry,
} from 'geojson';

import {Layer, LeafletMouseEvent, Path, Polyline} from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React from 'react';
import {GeoJSON, LatLngBounds, Map} from 'react-leaflet';
import styles from './MapBlock.module.scss';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
}

interface IdValue {
  id: string;
  value: string;
}

interface MapState {
  options: {
    location: SelectOption[];
  };

  selected: {
    indicator: string;
    timePeriod: string;
    location: string;
    results: IdValue[];
  };

  legend: LegendEntry[];
}


interface LegendEntry {
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

function getLowestLocationCode(location: DataBlockLocation) {
  return (
    (location.localAuthorityDistrict && location.localAuthorityDistrict.code) ||
    (location.localAuthority && location.localAuthority.code) ||
    (location.region && location.region.code) ||
    (location.country && location.country.code) ||
    ''
  );
}

function getLocationsForIndicator(
  data: DataBlockData,
  meta: DataBlockMetadata,
  indicator: string,
) {
  const allLocationIds = Array.from(
    new Set(
      data.result
      .filter(r => r.measures[indicator] !== undefined)
      .map(r => getLowestLocationCode(r.location)),
    ),
  );

  return [
    {label: 'select...', value: ''},
    ...allLocationIds
    .reduce(
      (locations: { label: string; value: string }[], next: string) => {
        const {label, value} = (meta.locations || {})[next];

        return [...locations, {label, value}];
      },
      [],
    )
    .sort((a, b) => {
      if (a.label < b.label) return -1;
      if (a.label > b.label) return 1;
      return 0;
    }),
  ];
}

function getGeometryForOptions(
  sourceData: {
    location: DataBlockLocationMetadata;
    selectedMeasure: number;
    measures: Dictionary<string>;
  }[],
  min: number,
  scale: number,
): FeatureCollection<Geometry, DataBlockGeoJsonProperties> {
  const calculateColorStyle = (value: number) =>
    styles[
      `rate${Math.min(Math.floor((value - min) / scale), 4).toFixed(0)}`
      ];

  return {
    type: 'FeatureCollection',
    features: sourceData.map(({location, selectedMeasure, measures}) => ({
      ...location.geoJson[0],
      id: location.geoJson[0].properties.code,
      properties: {
        ...location.geoJson[0].properties,
        measures,
        className: calculateColorStyle(selectedMeasure),
      },
    })),
  };
}

function calculateMinAndScaleForSourceData(
  sourceData: { selectedMeasure: number }[],
) {
  const {min, max} = sourceData.reduce(
    // eslint-disable-next-line no-shadow
    ({min, max}, {selectedMeasure}) => ({
      min: selectedMeasure < min ? selectedMeasure : min,
      max: selectedMeasure > max ? selectedMeasure : max,
    }),
    {min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY},
  );

  if (min === max) {
    return {min, scale: 0};
  }

  const range = max - min;
  const scale = range / 5.0;
  return {min, scale};
}

function calculateSourceData(
  data: DataBlockData,
  meta: DataBlockMetadata,
  displayedFilter: number,
  selectedYear: string,
) {
  const resultsFilteredByYear = data.result.filter(
    result => result.timePeriod === selectedYear,
  );

  return resultsFilteredByYear
  .map(result => ({
    location: (meta.locations || {})[
      getLowestLocationCode(result.location)
      ],
    selectedMeasure: +result.measures[displayedFilter],
    measures: result.measures,
  }))
  .filter(
    r => r.location !== undefined && r.location.geoJson !== undefined,
  );
}


function generateGeometryAndLegendForSelectedOptions(
  data: DataBlockData,
  meta: DataBlockMetadata,
  selectedIndicator: string,
  selectedYear: string,
) {
  const displayedFilter = +selectedIndicator;

  const sourceData = calculateSourceData(
    data,
    meta,
    displayedFilter,
    selectedYear,
  );

  const {min, scale} = calculateMinAndScaleForSourceData(sourceData);

  const legend: LegendEntry[] = [...Array(5)].map((_, idx) => {
    return {
      min: (min + idx * scale).toFixed(1),
      max: (min + (idx + 1) * scale).toFixed(1),
      idx,
    };
  });

  const geometry = getGeometryForOptions(sourceData, min, scale);

  return {geometry, legend};
}

function registerResizingCheck(container: HTMLDivElement, callback: () => void): IntersectionObserver {
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
  geometry?: FeatureCollection<Geometry, DataBlockGeoJsonProperties>
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

const MapBlock = ({
  data,
  meta,
  position = {lat: 53.00986, lng: -3.2524038},
  width,
  height,
  axes,
}: MapProps) => {

  const mapRef = React.createRef<Map>();
  const geoJsonRef = React.createRef<GeoJSON>();
  const container = React.createRef<HTMLDivElement>();


  const [state, setState] = React.useState<MapState>({
    selected: {
      indicator: '',
      timePeriod: '',
      location: '',
      results: [],
    },
    options: {
      location: [],
    },
    legend: [],
  });

  const [geometry, setGeometry] = React.useState<FeatureCollection<Geometry, DataBlockGeoJsonProperties>>();

  const [ukGeometry, setUkGeometry] = React.useState<FeatureCollection>();

  const intersectionObserver = React.useRef<IntersectionObserver>();


  React.useEffect(() => {
    let {selected} = state;

    if (data.result && data.result.length > 0) {
      const sortedMeasures = Object.values(meta.indicators).sort((a, b) =>
        a.label.localeCompare(b.label),
      );

      // TODO, if required, allow range of years to be selected
      const firstTimePeriod =
        meta.timePeriods &&
        meta.timePeriods[data.result[0].timePeriod] &&
        meta.timePeriods[data.result[0].timePeriod].value;

      selected = {
        ...selected,
        indicator: sortedMeasures[0].value,
        timePeriod: firstTimePeriod,
      };

      const {
        legend,
      } = generateGeometryAndLegendForSelectedOptions(
        data,
        meta,
        selected.indicator,
        selected.timePeriod,
      );

      import('@common/services/UKGeoJson')
      .then(imported => {

        setUkGeometry(imported.default);
      });


      const location = getLocationsForIndicator(
        data,
        meta,
        selected.indicator,
      );

      setState({
        selected,
        options: {
          location,
        },
        legend,
      });
    }

    if (container.current) {
      intersectionObserver.current = registerResizingCheck(container.current, () => {
        if (mapRef.current) {
          const {current} = mapRef;
          requestAnimationFrame(() => {
            current.leafletElement.invalidateSize();
          });
        }
      });
    }

    return () => {
      if (intersectionObserver.current !== undefined) {
        intersectionObserver.current.disconnect();
      }
    };

  }, []);


  const updateGeojsonGeometry = (
    geoJson: FeatureCollection<Geometry, DataBlockGeoJsonProperties>,
  ) => {
    if (geoJsonRef.current) {
      geoJsonRef.current.leafletElement.clearLayers();
      geoJsonRef.current.leafletElement.addData(geoJson);
    }
  };

  const onSelectIndicator = (newSelectedIndicator: string) => {
    const {selected, options} = state;

    const {
      geometry: newGeometry,
      legend,
    } = generateGeometryAndLegendForSelectedOptions(
      data,
      meta,
      newSelectedIndicator,
      selected.timePeriod,
    );

    updateGeojsonGeometry(newGeometry);

    setGeometry(newGeometry);

    setState({
      selected: {...selected, indicator: newSelectedIndicator},

      legend,

      options: {
        ...options,
        location: getLocationsForIndicator(
          data,
          meta,
          newSelectedIndicator,
        ),
      },
    });
  };


  function selectLocationOption(locationValue: string) {
    const {selected} = state;
    let results: IdValue[] = [];

    const {
      element: currentSelectedLocationElement,
    } = getFeatureElementById(selected.location, geometry);

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

        results = Object.entries(measures).reduce(
          (r: IdValue[], [id, value]) => [...r, {id, value}],
          [],
        );
      }
    }

    setState({
      ...state,
      selected: {
        ...selected,
        location: locationValue,
        results,
      },
    });
  }

  const onEachFeature = (feature: MapFeature, featureLayer: Path) => {
    const {selected} = state;

    if (feature.properties) {
      // eslint-disable-next-line no-param-reassign
      feature.properties.layer = featureLayer;
    }

    featureLayer.setStyle({
      className: classNames(
        feature.properties && feature.properties.className,
        {[styles.selected]: feature.id === selected.location},
      ),
    });

    featureLayer.bindTooltip(() => {
      if (feature.properties) {
        const content = Object.entries(feature.properties.measures).map(
          ([id, value]) =>
            `${meta.indicators[id].label} : ${value}${meta.indicators[id].unit}`,
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


  const {selected, options, legend} = state;


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
              value={selected.indicator}
              onChange={e => onSelectIndicator(e.currentTarget.value)}
              options={axes.major.dataSets.map(
                indicator => meta.indicators[indicator.indicator],
              )}
              order={[]}
            />
          </div>

          <div className="govuk-form-group govuk-!-margin-bottom-6">
            <FormSelect
              name="selectedLocation"
              id="selectedLocation"
              label="Select a location"
              value={selected.location}
              onChange={e => selectLocationOption(e.currentTarget.value)}
              options={options.location}
              order={[]}
            />
          </div>

        </form>

        {selected.results.length > 0 ? (
          <div>
            {selected.results.map(result => (
              <div
                key={result.id}
                className="dfe-dash-tiles__tile govuk-!-margin-bottom-6"
              >
                <h3 className="govuk-heading-m dfe-dash-tiles__heading">
                  {meta.indicators[result.id].label}
                </h3>
                <p
                  className="govuk-heading-xl govuk-!-margin-bottom-2"
                  aria-label={meta.indicators[result.id].label}
                >
                  <span>
                    {` ${result.value}${meta.indicators[result.id].unit} `}
                  </span>
                </p>
                <Details
                  summary={`What is ${meta.indicators[result.id].label}?`}
                >
                  Description for {meta.indicators[result.id].label}
                </Details>
              </div>
            ))}
          </div>
        ) : (
          ''
        )}

        {selected.indicator !== '' && (
          <div>
            <h3 className="govuk-heading-s">
              Key to {meta.indicators[selected.indicator].label}
            </h3>
            <dl className="govuk-list">
              {legend &&
              legend.map(({min, max, idx}) => (
                <dd className={styles.legend} key={idx}>
                  <span className={styles[`rate${idx}`]}>&nbsp;</span> {min}
                  {meta.indicators[selected.indicator].unit}&nbsp; to {max}
                  {meta.indicators[selected.indicator].unit}{' '}
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
              style={(feature?: Feature) => ({
                className: classNames(
                  feature &&
                  feature.properties &&
                  feature.properties.className,
                  {
                    [styles.selected]:
                    feature && feature.id === selected.location,
                  },
                ),
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
    },
  ],
};

MapBlock.definition = definition;

export default MapBlock;
