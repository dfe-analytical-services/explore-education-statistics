import Header from '../Header';
import { Filter } from '../../types/filters';
import addFilters from '../addFilters';

describe('addFilters', () => {
  test('converts filters into Header instances and adds them to headers', () => {
    const headers: Header[] = [
      new Header('filter1', 'Filter 1'),
      new Header('filter2', 'Filter 2'),
    ];
    const filters: Filter[] = [
      {
        id: 'filter3',
        label: 'Filter 3',
        value: 'filter3',
        group: 'filter1',
      },
      {
        id: 'filter4',
        label: 'Filter 4',
        value: 'filter4',
        group: 'filter2',
      },
      {
        id: 'filter5',
        label: 'Filter 5',
        value: 'filter5',
        group: 'filter2',
      },
    ];
    const result = addFilters(headers, filters);
    expect(result).toHaveLength(3);

    expect(result[0].id).toBe('filter1');
    expect(result[0].span).toBe(1);

    expect(result[1].id).toBe('filter2');
    expect(result[1].span).toBe(1);

    expect(result[2].id).toBe('filter3');
    expect(result[2].span).toBe(1);

    expect(result[2].children).toHaveLength(1);
    expect(result[2].children[0].id).toBe('filter4');

    expect(result[2].children[0].span).toBe(1);
    expect(result[2].children[0].children).toHaveLength(1);

    expect(result[2].children[0].children[0].id).toBe('filter5');
    expect(result[2].children[0].children[0].span).toBe(1);
  });
});
