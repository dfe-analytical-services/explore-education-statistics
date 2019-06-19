import React from 'react';

import { render } from 'react-testing-library';
import LineChartBlock from '../LineChartBlock';

import testData from './__data__/testBlockData';

jest.mock('recharts/lib/util/LogUtils');

describe('LineChartBlock', () => {
  test('renders with correct output', () => {
    const { container } = render(
      <LineChartBlock {...testData.AbstractChartProps} />,
    );

    expect(
      container.querySelector('.xAxis text.recharts-label tspan'),
    ).toHaveTextContent('test x axis');
    expect(
      container.querySelector('.yAxis text.recharts-label tspan'),
    ).toHaveTextContent('test y axis');

    expect(
      container.querySelector(
        '.xAxis .recharts-cartesian-axis-tick:nth-child(1) text tspan',
      ),
    ).toHaveTextContent('2014/15');
    expect(
      container.querySelector(
        '.xAxis .recharts-cartesian-axis-tick:nth-child(2) text tspan',
      ),
    ).toHaveTextContent('2015/16');

    expect(
      Array.from(container.querySelectorAll('.recharts-line')).length,
    ).toBe(3);

    expect(container).toMatchSnapshot();
  });
});
