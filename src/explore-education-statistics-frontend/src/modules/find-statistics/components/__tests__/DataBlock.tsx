import React from 'react';
import { render, wait } from 'react-testing-library';
import _dataBlockService, {
  DataBlockRequest,
  GeographicLevel,
} from '@common/services/dataBlockService';
import testData from '@common/modules/find-statistics/components/charts/__tests__/__data__/testBlockData';
import domUtil from '@common-test/domUtil';
import { Summary } from 'explore-education-statistics-common/src/services/publicationService';
import DataBlock from '../DataBlock';

jest.mock('@common/services/dataBlockService');

const dataBlockService = _dataBlockService as jest.Mocked<
  typeof _dataBlockService
>;

describe('DataBlock', () => {
  const dataBlockRequest: DataBlockRequest = {
    subjectId: 1,
    geographicLevel: GeographicLevel.National,
    startYear: 2014,
    endYear: 2015,
    filters: [1, 2],
    indicators: [23, 26, 28],
  };

  const summary: Summary = {
    dataKeys: ['23', '26', '28'],
    description: {
      type: 'MarkDownBlock',
      body: `<div>test</div>`,
    },
  };

  test('datablock renders downloads only', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(
      Array.from(container.querySelectorAll('section.govuk-tabs__panel'))
        .length,
    ).toBe(1);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('datablock renders horizontal chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
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
      Array.from(container.querySelectorAll('section.govuk-tabs__panel'))
        .length,
    ).toBe(2);

    expect(
      domUtil.elementContainingText(
        container,
        'section.govuk-tabs__panel h3',
        'Charts',
      ).length,
    ).toBe(1);

    expect(Array.from(container.querySelectorAll('.recharts-bar')).length).toBe(
      3,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('datablock renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
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
      Array.from(container.querySelectorAll('section.govuk-tabs__panel'))
        .length,
    ).toBe(2);

    expect(
      domUtil.elementContainingText(
        container,
        'section.govuk-tabs__panel h3',
        'Charts',
      ).length,
    ).toBe(1);

    expect(Array.from(container.querySelectorAll('.recharts-bar')).length).toBe(
      3,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('datablock renders vertical chart', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
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
      Array.from(container.querySelectorAll('section.govuk-tabs__panel'))
        .length,
    ).toBe(2);

    expect(
      domUtil.elementContainingText(
        container,
        'section.govuk-tabs__panel h3',
        'Charts',
      ).length,
    ).toBe(1);

    expect(Array.from(container.querySelectorAll('.recharts-bar')).length).toBe(
      3,
    );

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('datablock renders table', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.innerHTML).toMatchSnapshot();
  });

  test('datablock renders summary', async () => {
    const getDataBlockForSubject = dataBlockService.getDataBlockForSubject.mockImplementation(
      (_: DataBlockRequest) => {
        return Promise.resolve(testData.response);
      },
    );

    const { container } = render(
      <DataBlock
        type="datablock"
        dataBlockRequest={dataBlockRequest}
        showTables={false}
        summary={summary}
      />,
    );

    await wait();

    expect(getDataBlockForSubject).toBeCalledWith(dataBlockRequest);

    expect(container.querySelector('#datablock_8_summary')).toBeDefined();

    //expect(container.innerHTML).toMatchSnapshot();
  });
});
