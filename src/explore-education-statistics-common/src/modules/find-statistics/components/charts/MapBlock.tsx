import Details from '@common/components/Details';
import { FormSelect } from '@common/components/form';
import { SelectOption } from '@common/components/form/FormSelect';
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
import { Dictionary } from '@common/types/util';
import classNames from 'classnames';
import {
  Feature,
  FeatureCollection,
  GeoJsonProperties,
  Geometry,
} from 'geojson';

import { Layer, LeafletMouseEvent, Path, Polyline } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React, { Component, createRef } from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';
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

  ukGeometry?: FeatureCollection;

  geometry: FeatureCollection<Geometry, DataBlockGeoJsonProperties> | undefined;
  legend: LegendEntry[];
}

interface LegendEntry {
  min: string;
  max: string;
  idx: number;
}

interface GroupOption {
  id: string;
  title: string;
  type: 'timeperiod' | 'filter' | 'location' | null;
  selected: string;
  options: SelectOption[];
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

class MapBlock extends Component<MapProps, MapState> {
  private readonly mapRef = createRef<Map>();

  private readonly geoJsonRef = createRef<GeoJSON>();

  private readonly container = createRef<HTMLDivElement>();

  public state: MapState = {
    selected: {
      indicator: '',
      timePeriod: '',
      location: '',
      results: [],
    },
    options: {
      location: [],
    },
    geometry: undefined,
    legend: [],
  };

  private intersectionObserver!: IntersectionObserver;

  public async componentDidMount() {
    const { data, meta } = this.props;
    let { selected } = this.state;

    const sortedMeasures = Object.values(meta.indicators).sort((a, b) =>
      a.label.localeCompare(b.label),
    );

    // TODO, if required, allow range of years to be selected
    const firstTimePeriod = meta.timePeriods[data.result[0].timePeriod].value;

    selected = {
      ...selected,
      indicator: sortedMeasures[0].value,
      timePeriod: firstTimePeriod,
    };

    const {
      geometry,
      legend,
    } = MapBlock.generateGeometryAndLegendForSelectedOptions(
      data,
      meta,
      selected.indicator,
      selected.timePeriod,
    );

    const imported = await import('@common/services/UKGeoJson');

    const location = MapBlock.getLocationsForIndicator(
      data,
      meta,
      selected.indicator,
    );

    this.setState({
      selected,
      options: {
        location,
      },
      geometry,
      legend,
      ukGeometry: imported.default,
    });

    this.registerResizingCheck();
  }

  public componentWillUnmount(): void {
    if (this.intersectionObserver) {
      this.intersectionObserver.disconnect();
    }
  }

  public static definition: ChartDefinition = {
    type: 'map',
    name: 'Geographic',

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

  private static getLocationsForIndicator(
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
      { label: 'select...', value: '' },
      ...allLocationIds
        .reduce(
          (locations: { label: string; value: string }[], next: string) => {
            const { label, value } = (meta.locations || {})[next];

            return [...locations, { label, value }];
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

  private static getGeometryForOptions(
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
      features: sourceData.map(({ location, selectedMeasure, measures }) => ({
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

  private static generateGeometryAndLegendForSelectedOptions(
    data: DataBlockData,
    meta: DataBlockMetadata,
    selectedIndicator: string,
    selectedYear: string,
  ) {
    const displayedFilter = +selectedIndicator;

    const sourceData = this.calculateSourceData(
      data,
      meta,
      displayedFilter,
      selectedYear,
    );

    const { min, scale } = this.calculateMinAndScaleForSourceData(sourceData);

    const legend: LegendEntry[] = [...Array(5)].map((_, idx) => {
      return {
        min: (min + idx * scale).toFixed(1),
        max: (min + (idx + 1) * scale).toFixed(1),
        idx,
      };
    });

    const geometry = this.getGeometryForOptions(sourceData, min, scale);

    return { geometry, legend };
  }

  private static calculateMinAndScaleForSourceData(
    sourceData: { selectedMeasure: number }[],
  ) {
    const { min, max } = sourceData.reduce(
      // eslint-disable-next-line no-shadow
      ({ min, max }, { selectedMeasure }) => ({
        min: selectedMeasure < min ? selectedMeasure : min,
        max: selectedMeasure > max ? selectedMeasure : max,
      }),
      { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
    );

    if (min === max) {
      return { min, scale: 0 };
    }

    const range = max - min;
    const scale = range / 5.0;
    return { min, scale };
  }

  private static calculateSourceData(
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

  private getFeatureElementById(
    id: string,
  ): { element?: Element; layer?: Path; feature?: Feature } {
    const { geometry } = this.state;

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

  private updateGeometryForOptions = () => {
    const { data, meta } = this.props;
    const { selected } = this.state;

    this.setState({
      ...MapBlock.generateGeometryAndLegendForSelectedOptions(
        data,
        meta,
        selected.indicator,
        selected.timePeriod,
      ),
    });
  };

  private registerResizingCheck() {
    if (this.container.current && this.container.current.parentElement) {
      this.intersectionObserver = new IntersectionObserver(
        entries => {
          if (entries.length > 0) {
            if (entries[0].intersectionRatio > 0) {
              if (this.mapRef.current) {
                const { current } = this.mapRef;
                requestAnimationFrame(() => {
                  current.leafletElement.invalidateSize();
                });
              }
            }
          }
        },
        {
          threshold: 0.00001,
        },
      );

      this.intersectionObserver.observe(this.container.current);
    }
  }

  private onSelectIndicator = (newSelectedIndicator: string) => {
    const { data, meta } = this.props;
    const { selected, options } = this.state;

    const {
      geometry,
      legend,
    } = MapBlock.generateGeometryAndLegendForSelectedOptions(
      data,
      meta,
      newSelectedIndicator,
      selected.timePeriod,
    );

    this.updateGeojsonGeometry(geometry);

    this.setState({
      selected: { ...selected, indicator: newSelectedIndicator },

      geometry,
      legend,

      options: {
        ...options,
        location: MapBlock.getLocationsForIndicator(
          data,
          meta,
          newSelectedIndicator,
        ),
      },
    });
  };

  private updateGeojsonGeometry(
    geometry: FeatureCollection<Geometry, DataBlockGeoJsonProperties>,
  ) {
    if (this.geoJsonRef.current) {
      this.geoJsonRef.current.leafletElement.clearLayers();
      this.geoJsonRef.current.leafletElement.addData(geometry);
    }
  }

  private selectLocationOption(locationValue: string) {
    const { selected } = this.state;
    let results: IdValue[] = [];

    const {
      element: currentSelectedLocationElement,
    } = this.getFeatureElementById(selected.location);

    if (currentSelectedLocationElement) {
      currentSelectedLocationElement.classList.remove(styles.selected);
    }

    const {
      layer: selectedLayer,
      element: selectedLocationElement,
      feature: selectedFeature,
    } = this.getFeatureElementById(locationValue);

    if (selectedLocationElement && selectedLayer && selectedFeature) {
      selectedLocationElement.classList.add(styles.selected);

      if (this.mapRef.current) {
        const polyLine: Polyline = selectedLayer as Polyline;

        this.mapRef.current.leafletElement.fitBounds(polyLine.getBounds(), {
          padding: [200, 200],
        });
        selectedLayer.bringToFront();
      }

      const { properties } = selectedFeature;

      if (properties) {
        // eslint-disable-next-line prefer-destructuring
        const measures: { [key: string]: string } = properties.measures;

        results = Object.entries(measures).reduce(
          (r: IdValue[], [id, value]) => [...r, { id, value }],
          [],
        );
      }
    }

    this.setState({
      selected: {
        ...selected,
        location: locationValue,
        results,
      },
    });
  }

  private updateSelectedGroupOption(group: GroupOption, value: string) {
    // eslint-disable-next-line no-param-reassign
    group.selected = value;
    this.updateGeometryForOptions();
  }

  private onEachFeature = (feature: MapFeature, featureLayer: Path) => {
    const { meta } = this.props;
    const { selected } = this.state;

    if (feature.properties) {
      // eslint-disable-next-line no-param-reassign
      feature.properties.layer = featureLayer;
    }

    featureLayer.setStyle({
      className: classNames(
        feature.properties && feature.properties.className,
        { [styles.selected]: feature.id === selected.location },
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

  private onClick = (e: MapClickEvent) => {
    const { feature } = e.sourceTarget;

    if (feature.properties) {
      this.selectLocationOption(feature.properties.code);
    }
  };

  public render() {
    const {
      position = { lat: 53.00986, lng: -3.2524038 },
      width,
      height,
      meta,
      axes,
    } = this.props;

    const { selected, options, geometry, legend, ukGeometry } = this.state;

    // TODO if filters are ever wanted to be included in the Maps
    const groupOptions: GroupOption[] = [];

    /*
    dataGroupings.map(
      ({timePeriod}, index) => {
        if (timePeriod) {
          return {
            id: `groupOption_year_${index}`,
            title: 'Select a time period',
            type: 'timeperiod',
            selected: `${data.result[0].year}`,
            options: Object.values<SelectOption>(
              data.result.reduce(
                (result: Dictionary<TimePeriod>, {year, timeIdentifier}) => {
                  return {
                    ...result,
                    [`${year}${timeIdentifier}`]: new TimePeriod(
                      year,
                      timeIdentifier,
                    ),
                  };
                },
                {},
              ),
            )
              .sort()
              .reverse(),
          };
        }

        return {
          id: `groupOption_${index}`,
          title: '',
          selected: '',
          type: null,
          options: Array<SelectOption>(),
        };
      },
    );
    */

    return (
      <div className="govuk-grid-row" ref={this.container}>
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
                onChange={e => this.onSelectIndicator(e.currentTarget.value)}
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
                onChange={e => this.selectLocationOption(e.currentTarget.value)}
                options={options.location}
                order={[]}
              />
            </div>

            {groupOptions.map(grouping => (
              <div
                key={grouping.id}
                className="govuk-form-group govuk-!-margin-bottom-6"
              >
                <FormSelect
                  name="grouping"
                  id={grouping.id}
                  label={grouping.title}
                  value={grouping.selected}
                  order={[]}
                  onChange={e =>
                    this.updateSelectedGroupOption(
                      grouping,
                      e.currentTarget.value,
                    )
                  }
                  options={grouping.options}
                />
              </div>
            ))}
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

          {selected.indicator !== '' ? (
            <div>
              <h3 className="govuk-heading-s">
                Key to {meta.indicators[selected.indicator].label}
              </h3>
              <dl className="govuk-list">
                {legend &&
                  legend.map(({ min, max, idx }) => (
                    <dd className={styles.legend} key={idx}>
                      <span className={styles[`rate${idx}`]}>&nbsp;</span> {min}
                      {meta.indicators[selected.indicator].unit}&nbsp; to {max}
                      {meta.indicators[selected.indicator].unit}{' '}
                    </dd>
                  ))}
              </dl>
            </div>
          ) : (
            ''
          )}
        </div>

        <div className={classNames('govuk-grid-column-two-thirds')}>
          <Map
            ref={this.mapRef}
            style={{
              width: (width && `${width}px`) || '100%',
              height: `${height || 600}px`,
            }}
            className={classNames(styles.map, 'dfe-print-break-avoid')}
            center={position}
            zoom={6.5}
            // minZoom={6.5}
            // zoomSnap={0.5}
            // maxBounds={this.state.maxBounds}
          >
            {ukGeometry && <GeoJSON data={ukGeometry} className={styles.uk} />}

            {geometry && (
              <GeoJSON
                ref={this.geoJsonRef}
                data={geometry}
                onEachFeature={this.onEachFeature}
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
                onclick={this.onClick}
                // style={this.styleFeature}
                // onClick={this.handleClick}
              />
            )}
          </Map>
        </div>
      </div>
    );
  }
}

export default MapBlock;
