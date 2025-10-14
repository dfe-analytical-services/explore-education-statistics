import { NavItem } from '@common/components/PageNavExpandable';
import getNavItemsFromHtml from '@common/components/util/getNavItemsFromHtml';

describe('Get nav items from html', () => {
  test('Returns h3 headings as nav items', () => {
    const input =
      '<h2>Test Level 2 Heading</h2><h3>Test Heading</h3><p>Lorem ipsum dolor sit amet, sea pertinax pertinacia appellantur in, est ad esse assentior mediocritatem, magna populo menandri cum te.</p><p>Lorema</p><h4>Level 4 Heading</h4><h3>Another H3 Heading!</h3>';
    const output = [
      {
        id: 'section-test-heading',
        text: 'Test Heading',
      },
      {
        id: 'section-another-h-3-heading',
        text: 'Another H3 Heading!',
      },
    ];

    const result = getNavItemsFromHtml(input);
    expect(result).toEqual(output);
  });

  test('Returns other heading levels as items when selected', () => {
    const input =
      '<h2>Test Level 2 Heading</h2><h3>Test Heading</h3><p>Lorem ipsum dolor sit amet, sea pertinax pertinacia appellantur in, est ad esse assentior mediocritatem, magna populo menandri cum te.</p><p>Lorema</p><h4>Level 4 Heading</h4><h3>Another H3 Heading!</h3>';
    const output = [
      {
        id: 'section-test-level-2-heading',
        text: 'Test Level 2 Heading',
      },
      {
        id: 'section-test-heading',
        text: 'Test Heading',
      },
      {
        id: 'section-level-4-heading',
        text: 'Level 4 Heading',
      },
      {
        id: 'section-another-h-3-heading',
        text: 'Another H3 Heading!',
      },
    ];

    const result = getNavItemsFromHtml(input, ['h2', 'h3', 'h4']);
    expect(result).toEqual(output);
  });

  test('Returns an empty array when no headings', () => {
    const input = '<p>Test content</p>';
    const output = [] as NavItem[];

    const result = getNavItemsFromHtml(input);
    expect(result).toEqual(output);
  });
});
