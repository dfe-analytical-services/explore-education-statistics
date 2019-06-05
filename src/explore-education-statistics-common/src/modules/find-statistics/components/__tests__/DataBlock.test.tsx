import React from 'react';
import { render, wait } from 'react-testing-library';
import _dataBlockService, {
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import domUtil from '@common-test/domUtil';
import { Summary } from '@common/services/publicationService';
import DataBlock from '../DataBlock';

jest.mock('@common/services/dataBlockService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;

describe('DataBlock', () => {
  const dataBlockRequest: DataBlockRequest = {
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    startYear: '2014',
    endYear: '2015',
    filters: ['1', '2'],
    indicators: ['23', '26', '28'],
  };

  const summary: Summary = {
    dataKeys: ['23', '26', '28'],
    description: {
      type: 'MarkDownBlock',
      body: `<div>test</div>`,
    },
  };

  test('renders horizontal chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        charts={[
          {
            type: 'horizontalbar',
            stacked: true,
            indicators: ['23', '26', '28'],
            xAxis: { title: 'test x axis' },
            yAxis: { title: 'test y axis' },
            width: 800,
            height: 600,
          },
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(
      domUtil.elementContainingText(
        container,
        'section.govuk-tabs__panel h3',
        'Charts',
      ).length,
    ).toBe(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    expect(container.querySelector('svg')).toMatchSnapshot();
  });

  test('renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        charts={[
          {
            type: 'verticalbar',
            indicators: ['23', '26', '28'],
            xAxis: { title: 'test x axis' },
            yAxis: { title: 'test y axis' },
            width: 800,
            height: 600,
          },
        ]}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      container.querySelectorAll('section.govuk-tabs__panel'),
    ).toHaveLength(1);

    expect(
      domUtil.elementContainingText(
        container,
        'section.govuk-tabs__panel h3',
        'Charts',
      ).length,
    ).toBe(1);

    expect(container.querySelectorAll('.recharts-bar')).toHaveLength(3);
    expect(container.querySelector('svg')).toMatchSnapshot();
  });

  test('renders table', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('renders summary', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        id="test"
        type="databock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        summary={summary}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.querySelector('#datablock_8_summary')).toBeDefined();

    // expect(container.innerHTML).toMatchSnapshot();
  });
});
