import { FeatureCollection, Geometry } from 'geojson';

import {
  DataBlockGeoJSON,
  DataBlockGeoJsonProperties,
  DataBlockLocation,
} from '@common/services/dataBlockService';
import locationDataImport from './temporaryLocationData.json';

const locationData = locationDataImport as FeatureCollection<
  Geometry,
  DataBlockGeoJsonProperties
>;

function getGeoJSONForLocation(
  location: DataBlockLocation,
): DataBlockGeoJSON | undefined {
  const result = locationData.features.filter(feature => {
    return (
      (location.country.country_code &&
        feature.properties.ctry17cd === location.country.country_code) ||
      (location.region.region_code &&
        feature.properties.lad17cd === location.region.region_code)
    );
  });

  return (result.length && result[0]) || undefined;
}

export default {
  getGeoJSONForLocation,
};
