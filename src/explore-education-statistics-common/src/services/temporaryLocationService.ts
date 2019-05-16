import { FeatureCollection, Geometry } from 'geojson';

import {
  DataBlockGeoJSON,
  DataBlockGeoJsonProperties,
  DataBlockLocation,
} from '@common/services/dataBlockService';
import locationDataImport from './temporaryLocationData.json';

const locationData = (locationDataImport as unknown) as FeatureCollection<
  Geometry,
  DataBlockGeoJsonProperties
>;

locationData.features = locationData.features.map(feature => {
  return {
    ...feature,

    properties: {
      ...feature.properties,
      code:
        feature.properties.lad17cd || feature.properties.ctry17cd || 'unknown',
      name:
        feature.properties.lad17nm || feature.properties.ctry17nm || 'unknown',
    },
  };
});

function getGeoJSONForLocation(
  location: DataBlockLocation,
): DataBlockGeoJSON | undefined {
  const result = locationData.features.filter(feature => {
    return (
      (location.country.country_code &&
        feature.properties.code === location.country.country_code) ||
      (location.region.region_code &&
        feature.properties.code === location.region.region_code)
    );
  });

  return (result.length && result[0]) || undefined;
}

export default {
  getGeoJSONForLocation,
};
