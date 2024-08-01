import { LocationCandidate } from '@admin/services/apiDataSetVersionService';
import styles from '@admin/pages/release/data/components/ApiDataSetLocationCode.module.scss';
import getApiDataSetLocationCodes, {
  LocationField,
  locationFields,
} from '@admin/pages/release/data/utils/getApiDataSetLocationCodes';
import React from 'react';

export default function ApiDataSetLocationCode({
  location,
}: {
  location: LocationCandidate;
}) {
  if (
    Object.keys(location).some(field =>
      locationFields.includes(field as LocationField),
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
