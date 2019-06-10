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
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import { FormSelect } from '@common/components/form';
import classNames from 'classnames';
import { SelectOption } from '@common/components/form/FormSelect';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import UKGeoJson from '@common/services/UKGeoJson';

import { geoJSON, Layer, LeafletMouseEvent, Path } from 'leaflet';
import styles from './MapBlock.module.scss';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
}

interface MapState {
  meta: DataBlockMetadata;
  data: DataBlockData;
  indicators: string[];

  selectedIndicator: string;

  locationOptions: SelectOption[];

  geometry: FeatureCollection<Geometry, DataBlockGeoJsonProperties> | undefined;

  selectedLocation: string;
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
  private mapRef = createRef<Map>();

  public constructor(props: MapProps) {
    super(props);

    const { meta, data, indicators } = props;

    this.state = {
      meta,
      data,
      indicators,
      selectedIndicator: indicators[0],
      locationOptions: [],
      geometry: undefined,
      selectedLocation: '',
    };

    this.mapRef = React.createRef<Map>();
  }

  public componentDidMount(): void {
    const { data, meta, selectedIndicator } = this.state;

    const locationOptions = MapBlock.getLocationsForIndicator(
      data,
      meta,
      selectedIndicator,
    );

    const geometry = MapBlock.getGeometryForOptions(
      data,
      meta,
      selectedIndicator,
    );

    this.setState({
      locationOptions,
      geometry,
    });
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
    data: DataBlockData,
    meta: DataBlockMetadata,
    selectedIndicator: string,
  ) {
    const selectedYear = 2016;
    const displayedFilter = +selectedIndicator;

    const resultsFilteredByYear = data.result.filter(
      result => result.year === selectedYear,
    );

    const sourceData = resultsFilteredByYear
      .map(result => ({
        location:
          meta.locations[
            result.location.localAuthority.new_la_code ||
              result.location.region.region_code ||
              result.location.country.country_code
          ],
        selectedMeasure: +result.measures[displayedFilter],
        measures: result.measures,
      }))
      .filter(
        r => r.location !== undefined && r.location.geoJson !== undefined,
      );

    const { min, max } = sourceData.reduce(
      // eslint-disable-next-line no-shadow
      ({ min, max }, { selectedMeasure }) => ({
        min: selectedMeasure < min ? selectedMeasure : min,
        max: selectedMeasure > max ? selectedMeasure : max,
      }),
      { min: Number.POSITIVE_INFINITY, max: Number.NEGATIVE_INFINITY },
    );

    const range = max - min;
    const scale = 4.0 / range;

    const calculateColorStyle = (value: number) =>
      styles[`rate${Math.floor((value - min) * scale)}`];

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

  private getFeatureElementById(id: string) {
    const { geometry } = this.state;

    if (geometry) {
      const selectedFeature = geometry.features.find(
        feature => feature.id === id,
      );

      if (selectedFeature) {
        const selectedLayer: Path = selectedFeature.properties.layer as Path;

        return { element: selectedLayer.getElement(), layer: selectedLayer };
      }
    }

    return { element: undefined, layer: undefined };
  }

  private updateGeometryForOptions = () => {
    const { data, meta, selectedIndicator } = this.state;

    this.setState({
      geometry: MapBlock.getGeometryForOptions(data, meta, selectedIndicator),
    });
  };

  private onSelectIndicator = (newSelectedIndicator: string) => {
    const { data, meta } = this.state;

    this.setState({
      selectedIndicator: newSelectedIndicator,
      geometry: MapBlock.getGeometryForOptions(
        data,
        meta,
        newSelectedIndicator,
      ),
      locationOptions: MapBlock.getLocationsForIndicator(
        data,
        meta,
        newSelectedIndicator,
      ),
    });
  };

  private updateSelectedLocation = (value: string) => {
    this.setState({ selectedLocation: value });
  };

  private selectLocationOption(locationValue: string) {
    const { selectedLocation } = this.state;

    const {
      element: currentSelectedLocationElement,
    } = this.getFeatureElementById(selectedLocation);

    if (currentSelectedLocationElement) {
      currentSelectedLocationElement.classList.remove(styles.selected);
    }

    const {
      layer: selectedLayer,
      element: selectedLocationElement,
    } = this.getFeatureElementById(locationValue);

    if (selectedLocationElement && selectedLayer) {
      selectedLocationElement.classList.add(styles.selected);

      // @ts-ignore
      this.mapRef.current.leafletElement.fitBounds(selectedLayer.getBounds(), {
        padding: [200, 200],
      });
      selectedLayer.bringToFront();
    }

    this.updateSelectedLocation(locationValue);
  }

  private updateSelectedGroupOption(group: GroupOption, value: string) {
    // eslint-disable-next-line no-param-reassign
    group.selected = value;
    this.updateGeometryForOptions();
  }

  private onEachFeature = (feature: MapFeature, featureLayer: Path) => {
    const { meta } = this.state;

    featureLayer.bindTooltip(layer => {
      if (feature.properties) {
        // eslint-disable-next-line no-param-reassign
        feature.properties.layer = featureLayer;

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
      dataGroupings = [],
    } = this.props;

    const {
      meta,
      data,
      indicators,
      selectedIndicator,
      selectedLocation,
      locationOptions,
      geometry,
    } = this.state;

    const locationGeoJSON = Object.values(meta.locations)
      .map(({ geoJson }) => geoJson)
      .filter(gj => gj !== undefined)
      .map(geojsonArray => geojsonArray[0]);

    const groupOptions: GroupOption[] = dataGroupings.map(
      ({ timePeriod }, index) => {
        if (timePeriod) {
          return {
            id: `groupOption_year_${index}`,
            title: 'Select a time period',
            type: 'timeperiod',
            selected: `${data.result[0].year}`,
            options: Object.values<SelectOption>(
              data.result.reduce(
                (result: Dictionary<TimePeriod>, { year, timeIdentifier }) => {
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
                value={selectedIndicator}
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
                value={selectedLocation}
                onChange={e => this.selectLocationOption(e.currentTarget.value)}
                order={null}
                options={locationOptions}
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

          {/*
        <div>
          <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
            <h3 className="govuk-heading-m dfe-dash-tiles__heading">
              Overall absence
            </h3>
            <p
              className="govuk-heading-xl govuk-!-margin-bottom-2"
              aria-label="Overall absence"
            >
              <span> ---% </span>
            </p>
            <Details summary="What is overall absence?">
              Overall absence is the adipisicing elit. Dolorum hic nobis
              voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </Details>
          </div>
          <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
            <h3 className="govuk-heading-m dfe-dash-tiles__heading">
              Authorised absence
            </h3>
            <p
              className="govuk-heading-xl govuk-!-margin-bottom-2"
              aria-label="Authorised absence"
            >
              <span> ---% </span>
            </p>
            <Details summary="What is authorised absence?">
              Authorised absence is the adipisicing elit. Dolorum hic nobis
              voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </Details>
          </div>
          <div className="dfe-dash-tiles__tile govuk-!-margin-bottom-6">
            <h3 className="govuk-heading-m dfe-dash-tiles__heading">
              Unauthorised absence
            </h3>
            <p
              className="govuk-heading-xl govuk-!-margin-bottom-2"
              aria-label="Unauthorised absence"
            >
              <span> ---% </span>
            </p>
            <Details summary="What is unauthorised absence?">
              Unauthorised absence is the adipisicing elit. Dolorum hic
              nobis voluptas quidem fugiat enim ipsa reprehenderit nulla.
            </Details>
          </div>
        </div>
        */}

          {/*
        <div className={classNames('')}>
          <h3 className="govuk-heading-s">Key to overall absence rate</h3>
          <dl className="govuk-list">
            {legend &&
            legend.map(({min, max, idx}) => (
              <dd key={idx}>
                <span className={styles[`rate${idx}`]}>&nbsp;</span> {min}%
                to {max}%{' '}
              </dd>
            ))}
          </dl>
        </div>
        */}
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
                data={geometry}
                onEachFeature={this.onEachFeature}
                style={(feature?: Feature) => ({
                  className:
                    feature &&
                    feature.properties &&
                    feature.properties.className,
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
