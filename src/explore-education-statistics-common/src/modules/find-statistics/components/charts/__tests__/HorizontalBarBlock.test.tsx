import React from 'react';

import { render } from 'react-testing-library';

import HorzontalBarBlock from '../HorizontalBarBlock';

import testData from './__data__/testBlockData';

jest.mock('recharts/lib/util/LogUtils');

describe('HorzontalBarBlock', () => {
  test('renders with correct output', () => {
    const { container } = render(
      <HorzontalBarBlock
        {...testData.AbstractChartProps}
        stacked
        height={600}
        width={900}
      />,
    );

    /*
    expect(
      container.querySelector('.xAxis text.recharts-label tspan'),
    ).toHaveTextContent('test x axis');
    expect(
      container.querySelector('.yAxis text.recharts-label tspan'),
    ).toHaveTextContent('test y axis');
     */

    expect(
      container.querySelector(
        '.yAxis .recharts-cartesian-axis-tick:nth-child(1) text tspan',
      ),
    ).toHaveTextContent('2014/15');
    expect(
      container.querySelector(
        '.yAxis .recharts-cartesian-axis-tick:nth-child(2) text tspan',
      ),
    ).toHaveTextContent('2015/16');

    expect(container).toMatchSnapshot();
  });
});
