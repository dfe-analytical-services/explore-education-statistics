/* eslint-disable */
import React from 'react';

import { render, wait } from 'react-testing-library';

import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import MapBlock from '../MapBlock';

describe('MapBlock', () => {
  test('renders', async () => {
    const { container } = render(
      <MapBlock
        {...{
          ...testData.AbstractMultipleChartProps,
          axes: {
            ...testData.AbstractMultipleChartProps.axes,
            major: {
              ...testData.AbstractMultipleChartProps.axes.major,
              groupBy: 'locations',
            },
          },
        }}
        height={600}
        width={900}
      />,
    );

    await wait();

    expect(container).toMatchSnapshot();
  });

  test('includes all locations in select', async () => {
    const { container } = render(
      <MapBlock
        {...{
          ...testData.AbstractMultipleChartProps,
          axes: {
            ...testData.AbstractMultipleChartProps.axes,
            major: {
              ...testData.AbstractMultipleChartProps.axes.major,
              groupBy: 'locations',
            },
          },
        }}
        height={600}
        width={900}
      />,
    );

    await wait();

    const select = container.querySelector('#selectedLocation');

    expect(select).toBeVisible();

    if (select) {
      expect(
        select.querySelector('option[value="E92000001"]'),
      ).toHaveTextContent('England');
      expect(
        select.querySelector('option[value="S92000001"]'),
      ).toHaveTextContent('Scotland');
    }
  });

  test('includes all indicators in select', async () => {
    const { container } = render(
      <MapBlock
        {...{
          ...testData.AbstractMultipleChartProps,
          axes: {
            ...testData.AbstractMultipleChartProps.axes,
            major: {
              ...testData.AbstractMultipleChartProps.axes.major,
              groupBy: 'locations',
            },
          },
        }}
        height={600}
        width={900}
      />,
    );

    await wait();

    const select = container.querySelector('#selectedIndicator');

    expect(select).toBeVisible();

    if (select) {
      expect(select.querySelector('option[value="2"]')).toHaveTextContent(
        'Authorised absence rate',
      );
      expect(select.querySelector('option[value="1"]')).toHaveTextContent(
        'Overall absence rate',
      );
      expect(select.querySelector('option[value="0"]')).toHaveTextContent(
        'Unauthorised absence rate',
      );
    }
  });

  test('include all indicators from reduced selection', async () => {
    const { container } = render(
      <MapBlock
        {...{
          ...testData.AbstractLargeDataChartProps_smaller_datasets,
          axes: {
            ...testData.AbstractLargeDataChartProps_smaller_datasets.axes,
            major: {
              ...testData.AbstractLargeDataChartProps_smaller_datasets.axes
                .major,
              groupBy: 'locations',
            },
          },
        }}
        height={600}
        width={900}
      />,
    );

    await wait();

    const select = container.querySelector('#selectedIndicator');

    expect(select).toBeVisible();

    if (select) {
      expect(select.querySelector('option[value="1"]')).toHaveTextContent(
        'Overall absence rate',
      );
      expect(select.querySelector('option[value="0"]')).toHaveTextContent(
        'Unauthorised absence rate',
      );
    }
  });
});
