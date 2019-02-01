import React from 'react';

import 'leaflet/dist/leaflet.css';

import styles from './PrototypeMap.module.scss';

import { Boundaries } from './PrototypeMapBoundaries';

import { Feature, FeatureCollection, GeoJsonObject } from 'geojson';
import { GeoJSON, Map } from 'react-leaflet';

const PrototypeMap = ({}) => {
  const position = {
    lat: 53.009865,
    lng: -3.2524038,
  };

  let mapNode: any;

  const i = () => mapNode.leafletElement.invalidateSize();

  // force a refresh to fix a bug
  requestAnimationFrame(i);

  // @ts-ignore
  const onEachFeature = (f, layer) => {
    // @ts-ignore
    layer.bindTooltip(f.properties.lad17nm, {
      // className: f.properties.toolTipClass,
      direction: 'center',
      opacity: 1.0,
    });
  };

  return (
    <div>
      <Map
        ref={(n: any) => (mapNode = n)}
        center={position}
        className={styles.map}
        zoom={6}
      >
        <GeoJSON data={Boundaries} onEachFeature={onEachFeature} />
      </Map>
    </div>
  );
};

export default PrototypeMap;
