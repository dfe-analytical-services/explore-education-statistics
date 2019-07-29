import React from 'react';

import { render } from 'react-testing-library';
import VerticalBarBlock from '../VerticalBarBlock';

import testData from './__data__/testBlockData';

jest.mock('recharts/lib/util/LogUtils');

const props = {
  ...testData.AbstractChartProps,
  height: 900,
};
const { axes } = props;

describe('VerticalBarBlock', () => {
  test('renders basic chart correctly', () => {
    const { container } = render(<VerticalBarBlock {...props} />);

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
      <VerticalBarBlock
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
      <VerticalBarBlock
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
      <VerticalBarBlock
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
    const { container } = render(<VerticalBarBlock {...props} legend="none" />);

    expect(
      container.querySelector('.recharts-default-legend'),
    ).not.toBeInTheDocument();
  });

  test('can stack data', () => {
    const { container } = render(
      <VerticalBarBlock {...props} stacked legend="none" />,
    );

    // Unsure how to tell stacked data apart, other than the snapshot

    expect(container).toMatchSnapshot();

    expect(
      Array.from(container.querySelectorAll('.recharts-rectangle')).length,
    ).toBe(6);
  });

  test('can render major axis reference line', () => {
    const { container } = render(
      <VerticalBarBlock
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
      <VerticalBarBlock
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
});
