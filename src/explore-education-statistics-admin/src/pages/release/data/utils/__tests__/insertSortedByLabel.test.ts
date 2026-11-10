import insertSortedByLabel from '@admin/pages/release/data/utils/insertSortedByLabel';

describe('insertSortedByLabel', () => {
  interface TestItem {
    label: string;
    key: string;
  }

  const getLabel = (item: TestItem) => item.label;

  const labels = (items: TestItem[]) => items.map(item => item.label);

  test('inserts into the middle of the list in alphabetical order', () => {
    const items: TestItem[] = [
      { label: 'Female', key: 'female' },
      { label: 'Total', key: 'total' },
    ];

    insertSortedByLabel(items, { label: 'Male', key: 'male' }, getLabel);

    expect(labels(items)).toEqual(['Female', 'Male', 'Total']);
  });

  test('inserts at the start and end of the list', () => {
    const items: TestItem[] = [{ label: 'Male', key: 'male' }];

    insertSortedByLabel(items, { label: 'Total', key: 'total' }, getLabel);
    insertSortedByLabel(items, { label: 'Female', key: 'female' }, getLabel);

    expect(labels(items)).toEqual(['Female', 'Male', 'Total']);
  });

  test('inserts into an empty list', () => {
    const items: TestItem[] = [];

    insertSortedByLabel(items, { label: 'Male', key: 'male' }, getLabel);

    expect(labels(items)).toEqual(['Male']);
  });

  test('uses natural ordering for numbers', () => {
    const items: TestItem[] = [
      { label: 'Year 1', key: 'year-1' },
      { label: 'Year 10', key: 'year-10' },
    ];

    insertSortedByLabel(items, { label: 'Year 2', key: 'year-2' }, getLabel);

    expect(labels(items)).toEqual(['Year 1', 'Year 2', 'Year 10']);
  });

  test('ignores case', () => {
    const items: TestItem[] = [
      { label: 'apple', key: 'apple' },
      { label: 'cherry', key: 'cherry' },
    ];

    insertSortedByLabel(items, { label: 'Banana', key: 'banana' }, getLabel);

    expect(labels(items)).toEqual(['apple', 'Banana', 'cherry']);
  });

  test('inserts after existing items with an equal label', () => {
    const items: TestItem[] = [
      { label: 'Barnsley', key: 'barnsley-1' },
      { label: 'Leeds', key: 'leeds' },
    ];

    insertSortedByLabel(
      items,
      { label: 'Barnsley', key: 'barnsley-2' },
      getLabel,
    );

    expect(items.map(item => item.key)).toEqual([
      'barnsley-1',
      'barnsley-2',
      'leeds',
    ]);
  });
});
