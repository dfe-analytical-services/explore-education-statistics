import { DataBlockGeoJSON } from '@common/services/dataBlockService';
import { Geometry } from 'geojson';
import testGeometry from './testGeometry.json';

const E92000001: DataBlockGeoJSON = {
  type: 'Feature',
  properties: {
    code: 'E92000001',
    name: 'England',
    long: -2.07813,
    lat: 53.230099,
    objectid: 1,
  },
  geometry: testGeometry as Geometry,
};

const S92000001: DataBlockGeoJSON = {
  type: 'Feature',
  properties: {
    code: 'S92000001',
    name: 'Scotland',
    long: -2.07813,
    lat: 53.230099,
    objectid: 2,
  },
  geometry: testGeometry as Geometry,
};

export default {
  E92000001,
  S92000001,
};
