import truncateAround from '../truncateAround';

describe('truncateAround', () => {
  const testText =
    'The temple to herself what tried trusted or a parents creating accordingly';

  test('truncates on both sides', () => {
    const truncated = truncateAround(testText, 35);
    expect(truncated).toBe(
      '...to herself what tried trusted or a parents creating...',
    );
  });

  test('does not truncate start when position is near start', () => {
    const truncated = truncateAround(testText, 20);
    expect(truncated).toBe('The temple to herself what tried trusted or a...');
  });

  test('does not truncate end when position is near end', () => {
    const truncated = truncateAround(testText, 44);
    expect(truncated).toBe(
      '...herself what tried trusted or a parents creating accordingly',
    );
  });

  test('can use custom truncation lengths', () => {
    const truncated = truncateAround(testText, 20, {
      startTruncateLength: 10,
      endTruncateLength: 10,
    });

    expect(truncated).toBe('...to herself what...');
  });

  test('can use custom omission ends', () => {
    const truncated = truncateAround(testText, 35, {
      startOmissionText: '!!!',
      endOmissionText: '!!!',
    });

    expect(truncated).toBe(
      '!!!to herself what tried trusted or a parents creating!!!',
    );
  });
});
