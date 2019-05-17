import { FeatureCollection, Geometry } from 'geojson';

import {
  DataBlockGeoJSON,
  DataBlockGeoJsonProperties,
  DataBlockLocation,
  GeographicLevel,
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
        feature.properties.code ||
        feature.properties.lad17cd ||
        feature.properties.ctry17cd ||
        'unknown',
      name:
        feature.properties.name ||
        feature.properties.lad17nm ||
        feature.properties.ctry17nm ||
        'unknown',
    },
  };
});

function getGeoJSONForLocation(
  level: GeographicLevel,
  location: DataBlockLocation,
): DataBlockGeoJSON | undefined {
  const result = locationData.features.filter(feature => {
    let expectedCode;
    if (level === GeographicLevel.National)
      expectedCode = location.country.country_code;
    if (level === GeographicLevel.LocalAuthority)
      expectedCode = location.localAuthority.old_la_code;

    return expectedCode && expectedCode === feature.properties.code;
  });

  return (result.length && result[0]) || undefined;
}

export default {
  getGeoJSONForLocation,
};
