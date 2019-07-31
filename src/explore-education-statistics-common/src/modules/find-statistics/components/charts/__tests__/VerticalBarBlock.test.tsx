import React from 'react';

import { render } from 'react-testing-library';
import {
  DataBlockData,
  DataBlockMetadata,
} from '@common/services/dataBlockService';
import { AxesConfiguration } from '@common/modules/find-statistics/components/charts/ChartFunctions';
import Chart from '../VerticalBarBlock';

import testData from './__data__/testBlockData';

jest.mock('recharts/lib/util/LogUtils');

const props = {
  ...testData.AbstractChartProps,
  height: 900,
};
const { axes } = props;

describe('VerticalBarBlock', () => {
  test('renders basic chart correctly', () => {
    const { container } = render(<Chart {...props} />);

    expect(container).toMatchSnapshot();

    // axes
    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).toBeInTheDocument();

    // grid & grid lines
    expect(
      container.querySelector('.recharts-cartesian-grid'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-grid-horizontal'),
    ).toBeInTheDocument();
    expect(
      container.querySelector('.recharts-cartesian-grid-vertical'),
    ).toBeInTheDocument();

    expect(
      container.querySelector('.recharts-default-legend'),
    ).toBeInTheDocument();

    // expect there to be rectangles for all 3 data sets across both years
    expect(
      Array.from(container.querySelectorAll('.recharts-rectangle')).length,
    ).toBe(6);
  });

  test('major axis can be hidden', () => {
    const { container } = render(
      <Chart
        {...props}
        axes={{
          ...axes,
          major: {
            ...axes.major,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).not.toBeInTheDocument();
  });

  test('minor axis can be hidden', () => {
    const { container } = render(
      <Chart
        {...props}
        axes={{
          ...axes,
          minor: {
            ...axes.minor,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).not.toBeInTheDocument();
  });

  test('both axes can be hidden', () => {
    const { container } = render(
      <Chart
        {...props}
        axes={{
          ...axes,
          minor: {
            ...axes.minor,
            visible: false,
          },
          major: {
            ...axes.major,
            visible: false,
          },
        }}
      />,
    );

    expect(
      container.querySelector('.recharts-cartesian-axis.yAxis'),
    ).not.toBeInTheDocument();

    expect(
      container.querySelector('.recharts-cartesian-axis.xAxis'),
    ).not.toBeInTheDocument();
  });

  test('can hide legend', () => {
    const { container } = render(<Chart {...props} legend="none" />);

    expect(
      container.querySelector('.recharts-default-legend'),
    ).not.toBeInTheDocument();
  });

  test('can stack data', () => {
    const { container } = render(<Chart {...props} stacked legend="none" />);

    // Unsure how to tell stacked data apart, other than the snapshot

    expect(container).toMatchSnapshot();

    expect(
      Array.from(container.querySelectorAll('.recharts-rectangle')).length,
    ).toBe(6);
  });

  test('can render major axis reference line', () => {
    const { container } = render(
      <Chart
        {...{
          ...props,
          axes: {
            ...props.axes,
            major: {
              ...props.axes.major,
              referenceLines: [
                {
                  label: 'hello',
                  position: '2014/15',
                },
              ],
            },
          },
        }}
        legend="none"
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toBeInTheDocument();
  });
  test('can render minor axis reference line', () => {
    const { container } = render(
      <Chart
        {...{
          ...props,
          axes: {
            ...props.axes,
            minor: {
              ...props.axes.minor,
              referenceLines: [
                {
                  label: 'hello',
                  position: 0,
                },
              ],
            },
          },
        }}
        legend="none"
      />,
    );

    expect(
      container.querySelector('.recharts-reference-line'),
    ).toBeInTheDocument();
  });

  test('dies gracefully with bad data', () => {
    const invalidData: DataBlockData = (undefined as unknown) as DataBlockData;
    const invalidMeta: DataBlockMetadata = (undefined as unknown) as DataBlockMetadata;
    const invalidAxes: AxesConfiguration = (undefined as unknown) as AxesConfiguration;

    const { container } = render(
      <Chart
        data={invalidData}
        labels={{}}
        meta={invalidMeta}
        axes={invalidAxes}
      />,
    );
    expect(container).toHaveTextContent('Unable to render chart');
  });

  test('Can change width of chart', () => {
    const propsWithSize = {
      ...props,
      width: 200,
    };

    const { container } = render(<Chart {...propsWithSize} />);

    const responsiveContainer = container.querySelector(
      '.recharts-responsive-container',
    );

    expect(responsiveContainer).toHaveProperty('style');

    if (responsiveContainer) {
      const div = responsiveContainer as HTMLElement;
      expect(div.style.width).toEqual('200px');
    }
  });

  test('Can change height of chart', () => {
    const propsWithSize = {
      ...props,
      height: 200,
    };

    const { container } = render(<Chart {...propsWithSize} />);

    const responsiveContainer = container.querySelector(
      '.recharts-responsive-container',
    );

    expect(responsiveContainer).toHaveProperty('style');

    if (responsiveContainer) {
      const div = responsiveContainer as HTMLElement;
      expect(div.style.height).toEqual('200px');
    }
  });
});
