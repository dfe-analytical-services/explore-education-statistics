/* eslint-disable */
import React from 'react';

import { render, wait } from 'react-testing-library';

import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import MapBlock from '../MapBlock';
import { DataBlockGeoJSON } from '@common/services/dataBlockService';

describe('MapBlock', () => {
  test('MapBlock renders', async () => {
    expect(
      testData.AbstractChartProps.meta.locations.E92000001.geoJson,
    ).not.toBeUndefined();

    // @ts-ignore
    const geoJson: DataBlockGeoJSON =
      testData.AbstractChartProps.meta.locations.E92000001.geoJson;

    const { container } = render(
      <MapBlock {...testData.AbstractChartProps} height={600} width={900} />,
    );

    await wait();

    expect(container).toMatchSnapshot();
  });
});
