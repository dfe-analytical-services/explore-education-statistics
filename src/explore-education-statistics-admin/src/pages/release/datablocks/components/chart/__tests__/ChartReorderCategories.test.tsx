import render from '@common-test/render';
import ChartReorderCategories from '@admin/pages/release/datablocks/components/chart/ChartReorderCategories';
import { testMapDataSetConfigs } from '@admin/pages/release/datablocks/components/chart/__tests__/__data__/testCategoricalData';
import noop from 'lodash/noop';
import { screen, within } from '@testing-library/dom';

describe('ChartReorderCategories', () => {
  test('displays the list of categories', async () => {
    const { user } = render(
      <ChartReorderCategories
        label="Test label"
        mapDataSetConfig={testMapDataSetConfigs[1]}
        onReorder={noop}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Reorder categories for Test label',
      }),
    );

    const modal = within(screen.getByRole('dialog'));
    const listItems = modal.getAllByRole('listitem');
    expect(listItems).toHaveLength(3);
    expect(listItems[0]).toHaveTextContent('large');
    expect(listItems[1]).toHaveTextContent('medium');
    expect(listItems[2]).toHaveTextContent('small');
  });

  test('reordering', async () => {
    const handleReorder = jest.fn();
    const { user } = render(
      <ChartReorderCategories
        label="Test label"
        mapDataSetConfig={testMapDataSetConfigs[1]}
        onReorder={handleReorder}
      />,
    );

    await user.click(
      screen.getByRole('button', {
        name: 'Reorder categories for Test label',
      }),
    );

    const modal = within(screen.getByRole('dialog'));

    await user.click(modal.getByRole('button', { name: 'Move large down' }));
    await user.click(modal.getByRole('button', { name: 'Move small up' }));

    const listItems = modal.getAllByRole('listitem');
    expect(listItems[0]).toHaveTextContent('medium');
    expect(listItems[1]).toHaveTextContent('small');
    expect(listItems[2]).toHaveTextContent('large');

    expect(handleReorder).not.toHaveBeenCalled();

    await user.click(screen.getByRole('button', { name: 'Confirm' }));

    expect(handleReorder).toHaveBeenCalledTimes(1);
    expect(handleReorder).toHaveBeenCalledWith({
      ...testMapDataSetConfigs[1],
      categoricalDataConfig: [
        {
          value: 'medium',
          colour: '#801650',
        },
        {
          value: 'small',
          colour: '#12436D',
        },
        {
          value: 'large',
          colour: '#28A197',
        },
      ],
    });
  });
});
