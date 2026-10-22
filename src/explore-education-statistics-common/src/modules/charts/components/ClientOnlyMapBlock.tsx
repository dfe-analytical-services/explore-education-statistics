import LoadingSpinner from '@common/components/LoadingSpinner';
import type { MapBlockProps } from '@common/modules/charts/components/MapBlock';
import React, { ComponentType, useEffect, useState } from 'react';

/**
 * Renders {@link MapBlock}, loading it on the client only.
 *
 * Leaflet touches `document` as soon as it is imported, so MapBlock must
 * never enter the server import graph. The import is inside an effect,
 * and effects don't run during SSR.
 */
export default function ClientOnlyMapBlock(props: MapBlockProps) {
  const [MapBlock, setMapBlock] = useState<ComponentType<MapBlockProps>>();
  const [hasError, setError] = useState(false);

  useEffect(() => {
    import('@common/modules/charts/components/MapBlock')
      .then(module => setMapBlock(() => module.default))
      .catch(() => setError(true));
  }, []);

  if (hasError) {
    return <p className="govuk-inset-text">Could not load map</p>;
  }

  return (
    <LoadingSpinner loading={!MapBlock} text="Loading map">
      {MapBlock && <MapBlock {...props} />}
    </LoadingSpinner>
  );
}
