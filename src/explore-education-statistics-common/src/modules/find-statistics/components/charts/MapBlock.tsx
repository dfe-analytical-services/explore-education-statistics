/* eslint-disable @typescript-eslint/camelcase */
import {
  Feature,
  FeatureCollection,
  GeoJsonProperties,
  Geometry,
} from 'geojson';
import 'leaflet/dist/leaflet.css';
import React from 'react';
import { GeoJSON, LatLngBounds, Map } from 'react-leaflet';
import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import { DataBlockGeoJsonProperties } from '@common/services/dataBlockService';
import { FormSelect } from '@common/components/form';
import classNames from 'classnames';
import { SelectOption } from '@common/components/form/FormSelect';
import TimePeriod from '@common/services/types/TimePeriod';
import { Dictionary } from '@common/types/util';
import UKGeoJson from '@common/services/UKGeoJson';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
}

interface GroupOption {
  id: string;
  title: string;
  type: 'timeperiod' | 'filter' | 'location' | null;
  selected: string;
  options: SelectOption[];
}

function MapBlock(props: MapProps) {
  const {
    position = { lat: 53.00986, lng: -3.2524038 },
    width,
    height,
    meta,
    data,
    dataGroupings = [],
  } = props;

  const locationGeoJSON = Object.values(meta.locations)
    .map(({ geoJson }) => geoJson)
    .filter(gj => gj !== undefined)
    .map(geojsonArray => geojsonArray[0]);

  // Build options for forms
  const locationOptions = [
    { label: 'All', value: '' },

    ...locationGeoJSON
      .map(({ properties: { name, code } }) => ({ label: name, value: code }))
      .sort((a, b) => {
        if (a.label < b.label) return -1;
        if (a.label > b.label) return 1;
        return 0;
      }),
  ];

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

  const mapRef = React.createRef<Map>();

  const getGeometryForOptions = () => {
    const value: FeatureCollection<Geometry, DataBlockGeoJsonProperties> = {
      type: 'FeatureCollection',
      features: locationGeoJSON,
    };

    return value;
  };

  const [geometry, updateGeometry] = React.useState(getGeometryForOptions());

  const updateGeometryForOptions = () => {
    updateGeometry(getGeometryForOptions());
  };

  requestAnimationFrame(() => {
    if (mapRef.current) {
      mapRef.current.leafletElement.invalidateSize();
    }
  });

  const [selectedLocation, updateSelectedLocation] = React.useState('');

  const selectLocationOption = (locationValue: string) => {
    updateSelectedLocation(locationValue);
    // updateGeometryForOptions();
  };

  const updateSelectedGroupOption = (group: GroupOption, value: string) => {
    // eslint-disable-next-line no-param-reassign
    group.selected = value;
    updateGeometryForOptions();
  };

  const onEachFeature = (feature: MapFeature) => {
    // eslint-disable-next-line
    console.log(feature);
  };

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
              name="selectedLocation"
              id="selectedLocation"
              label="Select a location"
              value={selectedLocation}
              onChange={e => selectLocationOption(e.currentTarget.value)}
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
                  updateSelectedGroupOption(grouping, e.currentTarget.value)
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
          style={{
            width: (width && `${width}px`) || '100%',
            height: `${height || 600}px`,
          }}
          ref={mapRef}
          center={position}
          // className={styles.map}
          zoom={6.5}
          // minZoom={6.5}
          // zoomSnap={0.5}
          // maxBounds={this.state.maxBounds}
        >
          <GeoJSON
            data={geometry}
            onEachFeature={onEachFeature}
            // style={this.styleFeature}
            // onClick={this.handleClick}
          />
          {uk ? <GeoJSON data={uk} style={{ color: '777', weight: 1 }} /> : ''}
        </Map>
      </div>
    </div>
  );
}

export default MapBlock;
