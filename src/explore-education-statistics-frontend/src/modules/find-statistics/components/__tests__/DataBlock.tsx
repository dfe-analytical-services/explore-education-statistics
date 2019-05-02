import React from 'react';
import { render } from 'react-testing-library';
import { GeographicLevel } from 'explore-education-statistics-common/src/services/dataBlockService';
import DataBlock from '../DataBlock';

describe('DataBlock', () => {
  test('datablock test', () => {
    const { container } = render(
      <DataBlock
        type="datablock"
        dataQuery={{
          subjectId: 1,
          geographicLevel: GeographicLevel.National,
          startYear: 2014,
          endYear: 2015,
          filters: [1],
          indicators: [23, 26, 28],
        }}
      />,
    );
    expect(container.innerHTML).toMatchSnapshot();
  });
});
