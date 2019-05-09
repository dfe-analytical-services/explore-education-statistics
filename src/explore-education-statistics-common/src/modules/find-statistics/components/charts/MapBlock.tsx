import { Feature, GeoJsonProperties, Geometry } from 'geojson';
import 'leaflet/dist/leaflet.css';
import React, { Component } from 'react';
import { GeoJSON, Map } from 'react-leaflet';
import { ChartProps } from '@common/modules/find-statistics/components/charts/ChartFunctions';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapState {
  position: { lat: number; lng: number };
  // maxBounds: LatLngBounds;
  geometry?: MapFeature;
}

interface MapProps extends ChartProps {
  geometry?: MapFeature;
}

class MapBlock extends Component<MapProps, MapState> {
  protected mapRef: Map | null = null;

  public state = {
    // maxBounds: new LatLngBounds({ lat: 48, lng: -6.5 }, { lat: 60, lng: 2 }),
    position: {
      lat: 53.009865,
      lng: -3.2524038,
    },
    // eslint-disable-next-line react/destructuring-assignment
    geometry: this.props.geometry,
  };

  public componentDidMount() {
    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());
  }

  public componentDidUpdate() {
    requestAnimationFrame(() => this.refresh());
  }

  public refresh() {
    requestAnimationFrame(() => {
      if (this.mapRef) {
        this.mapRef.leafletElement.invalidateSize();
      }
    });
  }

  private renderGeoJson() {
    const { geometry } = this.state;

    if (geometry === undefined) return '';

    return (
      <GeoJSON
        data={geometry}
        // onEachFeature={this.onEachFeature}
        // style={this.styleFeature}
        // onClick={this.handleClick}
      />
    );
  }

  public render() {
    const { geometry, position } = this.state;
    const { height } = this.props;

    return (
      <div>
        {geometry && (
          <Map
            style={{ height: `${height || 200}px` }}
            ref={el => {
              this.mapRef = el;
            }}
            center={position}
            // className={styles.map}
            zoom={6.5}
            // minZoom={6.5}
            // zoomSnap={0.5}
            // maxBounds={this.state.maxBounds}
          >
            {this.renderGeoJson()}
          </Map>
        )}
      </div>
    );
  }
}

export default MapBlock;
