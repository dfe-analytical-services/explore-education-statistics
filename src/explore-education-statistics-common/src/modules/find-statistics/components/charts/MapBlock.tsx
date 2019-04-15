import { Feature, GeoJsonProperties, Geometry } from 'geojson';
import { LatLngBounds } from 'leaflet';
import 'leaflet/dist/leaflet.css';
import React, { Component } from 'react';
import { GeoJSON, Map } from 'react-leaflet';

import { ChartProps } from './Charts';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapState {
  position: { lat: number; lng: number };
  maxBounds: LatLngBounds;
  geometry?: MapFeature;
}

interface MapProps extends ChartProps {
  geometry?: MapFeature;
}

export class MapBlock extends Component<MapProps, MapState> {
  protected mapRef: Map | null = null;

  public state = {
    maxBounds: new LatLngBounds({ lat: 48, lng: -6.5 }, { lat: 60, lng: 2 }),
    position: {
      lat: 53.009865,
      lng: -3.2524038,
    },
    geometry: this.props.geometry,
  };

  private renderGeoJson() {
    if (this.state.geometry === undefined) return '';

    return (
      <GeoJSON
        data={this.state.geometry}
        // onEachFeature={this.onEachFeature}
        // style={this.styleFeature}
        // onClick={this.handleClick}
      />
    );
  }

  public refresh() {
    requestAnimationFrame(() => {
      if (this.mapRef) {
        this.mapRef.leafletElement.invalidateSize();
      }
    });
  }

  public componentDidMount() {
    // force a refresh to fix a bug
    requestAnimationFrame(() => this.refresh());
  }

  public componentDidUpdate() {
    requestAnimationFrame(() => this.refresh());
  }

  public render() {
    return (
      <div>
        {this.state.geometry && (
          <Map
            style={{ height: `${this.props.height || 200}px` }}
            ref={el => (this.mapRef = el)}
            center={this.state.position}
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
