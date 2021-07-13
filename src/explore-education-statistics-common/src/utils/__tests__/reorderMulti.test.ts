import reorderMulti from '@common/utils/reorderMulti';
import { Filter } from '@common/modules/table-tool/types/filters';

describe('reorderMulti', () => {
  const list: Filter[] = [
    {
      id: 'id1',
      label: 'label1',
      value: 'value1',
    },
    {
      id: 'id2',
      label: 'label2',
      value: 'value2',
    },
    {
      id: 'id3',
      label: 'label3',
      value: 'value3',
    },
    {
      id: 'id4',
      label: 'label4',
      value: 'value4',
    },
  ];

  test('move multiple items to the top of the list', () => {
    const selectedIds = ['value3', 'value4'];
    const newList = reorderMulti({
      list,
      sourceIndex: 2,
      destinationIndex: 0,
      selectedIds,
    });
    expect(newList).toEqual([
      {
        id: 'id3',
        label: 'label3',
        value: 'value3',
      },
      {
        id: 'id4',
        label: 'label4',
        value: 'value4',
      },
      {
        id: 'id1',
        label: 'label1',
        value: 'value1',
      },
      {
        id: 'id2',
        label: 'label2',
        value: 'value2',
      },
    ]);
  });

  test('move multiple items to the bottom of the list', () => {
    const selectedIds = ['value2', 'value3'];
    const newList = reorderMulti({
      list,
      sourceIndex: 1,
      destinationIndex: 3,
      selectedIds,
    });
    expect(newList).toEqual([
      {
        id: 'id1',
        label: 'label1',
        value: 'value1',
      },
      {
        id: 'id4',
        label: 'label4',
        value: 'value4',
      },
      {
        id: 'id2',
        label: 'label2',
        value: 'value2',
      },
      {
        id: 'id3',
        label: 'label3',
        value: 'value3',
      },
    ]);
  });

  test('move multiple items that were not next to each other', () => {
    const selectedIds = ['value1', 'value3'];
    const newList = reorderMulti({
      list,
      sourceIndex: 0,
      destinationIndex: 1,
      selectedIds,
    });
    expect(newList).toEqual([
      {
        id: 'id2',
        label: 'label2',
        value: 'value2',
      },
      {
        id: 'id1',
        label: 'label1',
        value: 'value1',
      },
      {
        id: 'id3',
        label: 'label3',
        value: 'value3',
      },
      {
        id: 'id4',
        label: 'label4',
        value: 'value4',
      },
    ]);
  });

  test('move multiple items from the top of the list', () => {
    const selectedIds = ['value1', 'value2'];
    const newList = reorderMulti({
      list,
      sourceIndex: 0,
      destinationIndex: 2,
      selectedIds,
    });
    expect(newList).toEqual([
      {
        id: 'id3',
        label: 'label3',
        value: 'value3',
      },

      {
        id: 'id1',
        label: 'label1',
        value: 'value1',
      },
      {
        id: 'id2',
        label: 'label2',
        value: 'value2',
      },
      {
        id: 'id4',
        label: 'label4',
        value: 'value4',
      },
    ]);
  });

  test('move multiple items from the bottom of the list', () => {
    const selectedIds = ['value3', 'value4'];
    const newList = reorderMulti({
      list,
      sourceIndex: 2,
      destinationIndex: 1,
      selectedIds,
    });
    expect(newList).toEqual([
      {
        id: 'id1',
        label: 'label1',
        value: 'value1',
      },
      {
        id: 'id3',
        label: 'label3',
        value: 'value3',
      },
      {
        id: 'id4',
        label: 'label4',
        value: 'value4',
      },
      {
        id: 'id2',
        label: 'label2',
        value: 'value2',
      },
    ]);
  });
});
