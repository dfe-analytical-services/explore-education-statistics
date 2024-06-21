import buildRel from '@common/utils/url/buildRel';

describe('buildRel', () => {
  test('concats new values with existing rel values', () => {
    const result = buildRel(['nofollow'], 'noopener noreferrer');
    expect(result).toBe('noopener noreferrer nofollow');
  });

  test('removes duplicate values', () => {
    const result = buildRel(['noopener'], 'noopener noreferrer');
    expect(result).toBe('noopener noreferrer');
  });

  test('removes empty values', () => {
    const result = buildRel([''], 'noopener noreferrer');
    expect(result).toBe('noopener noreferrer');
  });

  test('returns new values if no existing rel values', () => {
    const result = buildRel(['noopener', 'noreferrer'], undefined);
    expect(result).toBe('noopener noreferrer');
  });
});
