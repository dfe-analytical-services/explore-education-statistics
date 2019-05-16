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
import { Layer } from 'leaflet';
import { DataBlockGeoJSON } from '@common/services/dataBlockService';

export type MapFeature = Feature<Geometry, GeoJsonProperties>;

interface MapProps extends ChartProps {
  position?: { lat: number; lng: number };
  maxBounds?: LatLngBounds;
}

function MapBlock(props: MapProps) {
  const {
    position = { lat: 53.00986, lng: -3.2524038 },
    width = 200,
    height = 200,
    meta,
  } = props;

  const mapRef = React.createRef<Map>();

  const geometry: FeatureCollection<Geometry, GeoJsonProperties> = {
    type: 'FeatureCollection',
    features: Object.values(meta.locations).map(
      metadata => metadata.geoJson as DataBlockGeoJSON,
    ),
  };

  requestAnimationFrame(() => {
    if (mapRef.current) {
      mapRef.current.leafletElement.invalidateSize();
    }
  });

  const onEachFeature = (feature: MapFeature, layer: Layer) => {
    console.log(feature.properties);
  };

  return (
    <div>
      {geometry && (
        <Map
          style={{ width: `${width}px`, height: `${height}px` }}
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
        </Map>
      )}
    </div>
  );
}

export default MapBlock;
