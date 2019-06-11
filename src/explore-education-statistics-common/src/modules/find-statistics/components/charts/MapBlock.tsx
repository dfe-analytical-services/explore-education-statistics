import {
  Feature,
  FeatureCollection,
  GeoJsonProperties,
  Geometry,
} from 'geojson';
import 'leaflet/dist/leaflet.css';
import React, { Component, createRef } from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';
import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import {
  DataBlockData,
  DataBlockGeoJsonProperties,
  DataBlockLocation,
  DataBlockLocationMetadata,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import { FormSelect } from '@common/components/form';
import classNames from 'classnames';
import { SelectOption } from '@common/components/form/FormSelect';
import { Dictionary } from '@common/types/util';
import UKGeoJson from '@common/services/UKGeoJson';

import { Layer, LeafletMouseEvent, Path } from 'leaflet';
import Details from '@common/components/Details';
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
    location: string;
    results: IdValue[];
  };

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
    (location.localAuthorityDistrict &&
      location.localAuthorityDistrict.sch_lad_code) ||
    (location.localAuthority && location.localAuthority.new_la_code) ||
    (location.region && location.region.region_code) ||
    (location.country && location.country.country_code)
  );
}

class MapBlock extends Component<MapProps, MapState> {
  private readonly mapRef = createRef<Map>();

  private readonly geoJsonRef = createRef<GeoJSON>();

  public constructor(props: MapProps) {
    super(props);

    const { indicators } = props;

    this.state = {
      selected: {
        indicator: indicators[0],
        location: '',
        results: [],
      },
      options: {
        location: [],
      },
      geometry: undefined,
      legend: [],
    };
  }

  public componentDidMount(): void {
    const { data, meta } = this.props;
    const { selected } = this.state;

    const {
      geometry,
      legend,
    } = MapBlock.generateGeometryAndLegendForSelectedOptions(
      data,
      meta,
      selected.indicator,
    );

    this.setState({
      options: {
        location: MapBlock.getLocationsForIndicator(
          data,
          meta,
          selected.indicator,
        ),
      },
      geometry,
      legend,
    });

    const forceRefresh = () => {
      if (this.mapRef.current) {
        this.mapRef.current.leafletElement.invalidateSize();
      } else {
        window.requestAnimationFrame(forceRefresh);
      }
    };

    window.requestAnimationFrame(forceRefresh);
  }

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
            const { label, value } = meta.locations[next];

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
  ) {
    const calculateColorStyle = (value: number) =>
      styles[
        `rate${Math.min(Math.floor((value - min) / scale), 4).toFixed(0)}`
      ];

    const value: FeatureCollection<Geometry, DataBlockGeoJsonProperties> = {
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

    return value;
  }

  private static generateGeometryAndLegendForSelectedOptions(
    data: DataBlockData,
    meta: DataBlockMetadata,
    selectedIndicator: string,
    selectedYear: number = 2016,
  ) {
    const displayedFilter = +selectedIndicator;

    const sourceData = this.calculateSourceData(
      data,
      selectedYear,
      meta,
      displayedFilter,
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
    selectedYear: number,
    meta: DataBlockMetadata,
    displayedFilter: number,
  ) {
    const resultsFilteredByYear = data.result.filter(
      result => result.year === selectedYear,
    );

    return resultsFilteredByYear
      .map(result => ({
        location: meta.locations[getLowestLocationCode(result.location)],
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

    return { element: undefined, layer: undefined, feature: undefined };
  }

  private updateGeometryForOptions = () => {
    const { data, meta } = this.props;
    const { selected } = this.state;

    this.setState({
      ...MapBlock.generateGeometryAndLegendForSelectedOptions(
        data,
        meta,
        selected.indicator,
      ),
    });
  };

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

      // @ts-ignore
      this.mapRef.current.leafletElement.fitBounds(selectedLayer.getBounds(), {
        padding: [200, 200],
      });
      selectedLayer.bringToFront();

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
            `${meta.indicators[id].label} : ${value}${
              meta.indicators[id].unit
            }`,
        );

        // @ts-ignore
        content.unshift(`<strong>${meta.locations[feature.id].label}</strong>`);

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
      indicators,
    } = this.props;

    const { selected, options, geometry, legend } = this.state;

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

    const uk = UKGeoJson.UK;

    return (
      <div className="govuk-grid-row">
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
                order={null}
                options={indicators.map(
                  indicator => meta.indicators[indicator],
                )}
              />
            </div>

            <div className="govuk-form-group govuk-!-margin-bottom-6">
              <FormSelect
                name="selectedLocation"
                id="selectedLocation"
                label="Select a location"
                value={selected.location}
                onChange={e => this.selectLocationOption(e.currentTarget.value)}
                order={null}
                options={options.location}
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
                  onChange={e =>
                    this.updateSelectedGroupOption(
                      grouping,
                      e.currentTarget.value,
                    )
                  }
                  order={null}
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
                      {' '}
                      {result.value}
                      {meta.indicators[result.id].unit}{' '}
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

          {
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
          }
        </div>

        <div className={classNames('govuk-grid-column-two-thirds')}>
          <Map
            ref={this.mapRef}
            style={{
              width: (width && `${width}px`) || '100%',
              height: `${height || 600}px`,
            }}
            className={styles.map}
            center={position}
            zoom={6.5}
            // minZoom={6.5}
            // zoomSnap={0.5}
            // maxBounds={this.state.maxBounds}
          >
            {uk ? <GeoJSON data={uk} className={styles.uk} /> : ''}

            {geometry ? (
              <GeoJSON
                ref={this.geoJsonRef}
                data={geometry}
                onEachFeature={this.onEachFeature}
                style={(feature?: Feature) => ({
                  className: `${feature &&
                    feature.properties &&
                    feature.properties.className} ${
                    feature && feature.id === selected.location
                      ? styles.selected
                      : 'hello'
                  } `,
                })}
                onclick={this.onClick}
                // style={this.styleFeature}
                // onClick={this.handleClick}
              />
            ) : (
              ''
            )}
          </Map>
        </div>
      </div>
    );
  }
}

export default MapBlock;
