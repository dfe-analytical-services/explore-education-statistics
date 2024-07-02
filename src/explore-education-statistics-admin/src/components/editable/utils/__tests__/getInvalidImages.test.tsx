import getInvalidImages from '@admin/components/editable/utils/getInvalidImages';
import { JsonElement } from 'src/types/ckeditor';

describe('getInvalidImages', () => {
  const testContent: JsonElement[] = [
    { name: 'imageBlock', attributes: { src: 'image-source-1' } },
    {
      name: 'imageBlock',
      attributes: { src: 'image-source-2', alt: 'the alt text' },
    },
    { name: 'imageBlock', attributes: { src: 'image-source-4' } },
    { name: 'imageInline', attributes: { src: 'image-source-5' } },
    { name: 'paragraph', children: [{ data: 'A paragraph' }] },
    {
      name: 'paragraph',
      children: [{ name: 'imageBlock', attributes: { src: 'image-source-6' } }],
    },
  ];

  test('returns an array of invalid images', () => {
    const result = getInvalidImages(testContent);
    expect(result).toEqual([
      { name: 'imageBlock', attributes: { src: 'image-source-1' } },
      { name: 'imageBlock', attributes: { src: 'image-source-4' } },
      { name: 'imageInline', attributes: { src: 'image-source-5' } },
      {
        name: 'paragraph',
        children: [
          { name: 'imageBlock', attributes: { src: 'image-source-6' } },
        ],
      },
    ]);
  });
});
