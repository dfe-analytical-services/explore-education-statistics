import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import styles from '@admin/pages/release/data/components/ApiDataSetLocationCode.module.scss';
import getApiDataSetLocationCodes from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import {
  LocationCodeKey,
  locationCodeKeys,
} from '@common/utils/getLocationCodeEntries';
import React from 'react';

export default function ApiDataSetLocationCode({
  location,
}: {
  location: LocationCandidate;
}) {
  if (
    Object.keys(location).some(key =>
      locationCodeKeys.includes(key as LocationCodeKey),
    )
  ) {
    return (
      <>
        <br />
        <span className={styles.code}>
          {getApiDataSetLocationCodes(location)}
        </span>
      </>
    );
  }
  return null;
}
