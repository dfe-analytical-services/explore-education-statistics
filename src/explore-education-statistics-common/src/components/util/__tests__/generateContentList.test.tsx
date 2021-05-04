import generateContentList from '@common/components/util/generateContentList';

describe('Generate content list', () => {
  test('Outputs a nested list', () => {
    const fakeElement = document.createElement('div');

    const element1 = { ...fakeElement };
    element1.id = '1';
    element1.tagName = 'H3';
    element1.textContent = 'Heading 1';

    const element2 = { ...fakeElement };
    element2.id = '2';
    element2.tagName = 'H3';
    element2.textContent = 'Heading 2';

    const element3 = { ...fakeElement };
    element3.id = '2.1';
    element3.tagName = 'H4';
    element3.textContent = 'Heading 2.1';

    const element4 = { ...fakeElement };
    element4.id = '2.2';
    element4.tagName = 'H4';
    element4.textContent = 'Heading 2.2';
    const input = [element1, element2, element3, element4];
    const output = [
      {
        id: '1',
        tagName: 'H3',
        textContent: 'Heading 1',
        children: [],
      },
      {
        id: '2',
        tagName: 'H3',
        textContent: 'Heading 2',
        children: [
          {
            id: '2.1',
            tagName: 'H4',
            textContent: 'Heading 2.1',
          },
          {
            id: '2.2',
            tagName: 'H4',
            textContent: 'Heading 2.2',
          },
        ],
      },
    ];

    const result = generateContentList(input);
    expect(result).toEqual(output);
  });
});
