import getInvalidLinks from '@admin/components/editable/utils/getInvalidLinks';
import { JsonElement } from 'src/types/ckeditor';

describe('getInvalidLinks', () => {
  const testContent: JsonElement[] = [
    { name: 'imageBlock', attributes: { src: 'image-source-1' } },
    {
      name: 'paragraph',
      children: [
        { data: 'invalid link 1', attributes: { linkHref: 'gov.uk' } },
      ],
    },
    {
      name: 'paragraph',
      children: [{ data: 'anchor link', attributes: { linkHref: '#contact' } }],
    },
    {
      name: 'paragraph',
      children: [
        {
          data: 'localhost link',
          attributes: { linkHref: 'http://localhost/url' },
        },
      ],
    },
    { name: 'paragraph', children: [{ data: 'A paragraph' }] },
    {
      name: 'paragraph',
      children: [
        {
          data: 'mailto link',
          attributes: { linkHref: 'mailto:someone@somewhere' },
        },
      ],
    },
    {
      name: 'paragraph',
      children: [
        { data: 'valid link', attributes: { linkHref: 'https://gov.uk' } },
      ],
    },
    {
      name: 'paragraph',
      children: [
        { data: 'invalid link 2', attributes: { linkHref: 'some thing' } },
      ],
    },
  ];

  test('returns an array of invalid link errors', () => {
    const result = getInvalidLinks(testContent);
    expect(result).toEqual([
      { text: 'invalid link 1', url: 'gov.uk' },
      { text: 'invalid link 2', url: 'some thing' },
    ]);
  });
});
