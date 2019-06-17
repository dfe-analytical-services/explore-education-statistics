import { DataBlockGeoJSON } from '@common/services/dataBlockService';
import { Geometry } from 'geojson';

const geometry: Geometry = {
  type: 'MultiPolygon',
  coordinates: [
    [
      [
        [400869.793200000189245, 652744.480900000780821],
        [400910.507500000298023, 652415.036599999293685],
        [400178.472299999557436, 652575.4967],
      ],
    ],
  ],
};

const E92000001: DataBlockGeoJSON = {
  type: 'Feature',
  properties: {
    code: 'E92000001',
    name: 'England',
    long: -2.07813,
    lat: 53.230099,
    objectid: 1,
  },

  geometry,
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

  geometry,
};

export default {
  E92000001,
  S92000001,
};
