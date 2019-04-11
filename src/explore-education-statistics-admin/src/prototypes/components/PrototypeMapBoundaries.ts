import { Feature, FeatureCollection, Geometry } from 'geojson';
import { Path } from 'leaflet';
import originalData from './PrototypeMapBoundariesData.json';

interface BaseMapProperties {
  objectid: 391;
  lad17cd: 'W06000024';
  lad17nm: 'Merthyr Tydfil';
  lad17nmw: 'Merthyr Tudful';
  bng_e: 305916;
  bng_n: 206424;
  long: -3.36425;
  lat: 51.748581;
  st_areasha: 112279997.062914;
  st_lengths: 58706.245945;
}

export interface PrototypeMapBoundariesProperties extends BaseMapProperties {
  selectable: boolean;
  absence: {
    authorised: number;
    overall: number;
    unauthorised: number;
  };
  layer?: Path;
}

export type PrototypeMapBoundariesFeature = Feature<
  Geometry,
  PrototypeMapBoundariesProperties
>;

export type PrototypeMapBoundariesFeatureCollection = FeatureCollection<
  Geometry,
  PrototypeMapBoundariesProperties
>;

export const data = originalData as FeatureCollection<
  Geometry,
  BaseMapProperties
>;

export const boundaries: PrototypeMapBoundariesFeatureCollection = {
  ...data,
  features: data.features.map(feature => {
    const authorised = 3.2 + Math.random() * 3;
    const unauthorised = 1.5 + Math.random();

    return {
      ...feature,
      properties: {
        ...feature.properties,
        absence: {
          authorised: +authorised.toFixed(1),
          overall: +(authorised + unauthorised).toFixed(1),
          unauthorised: +unauthorised.toFixed(1),
        },
        selectable: feature.properties.lad17cd[0] === 'E',
      },
    };
  }),
};
