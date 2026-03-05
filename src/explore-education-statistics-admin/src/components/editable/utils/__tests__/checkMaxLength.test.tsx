import checkMaxLength from '@admin/components/editable/utils/checkMaxLength';
import { JsonElement } from 'src/types/ckeditor';

describe('checkMaxLength', () => {
  const testContent: JsonElement[] = [
    {
      name: 'paragraph',
      children: [
        {
          data: 'A line of normal text',
        },
      ],
    },
    {
      name: 'paragraph',
      children: [
        {
          attributes: {
            bold: true,
          },
          data: 'A line of bold text',
        },
      ],
    },
    {
      name: 'paragraph',
      children: [
        {
          data: 'A line with a mix of ',
        },
        {
          attributes: {
            bold: true,
          },
          data: 'bold',
        },
        {
          data: ' and normal text',
        },
      ],
    },
  ];

  test('returns an error if length exceeds maxLength', () => {
    const result = checkMaxLength(testContent, 80);
    expect(result).toEqual('You have used 81 characters and the limit is 80.');
  });

  test('returns null if the length is within range', () => {
    const result = checkMaxLength(testContent, 300);
    expect(result).toBeNull();
  });
});
