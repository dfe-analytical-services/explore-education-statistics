import findLast from 'lodash/findLast';
import { ParentListItem } from '@common/components/ContentSectionIndex';

export default function generateContentList(elements: HTMLElement[]) {
  const list: ParentListItem[] = [];

  elements.forEach((element: HTMLElement) => {
    if (element.tagName === 'H3') {
      list.push({
        id: element.id,
        tagName: element.tagName,
        textContent: element.textContent || '',
        children: [],
      });
    }
    if (element.tagName === 'H4') {
      const lastH3 = findLast(list, el => el.tagName === 'H3');
      if (lastH3) {
        lastH3.children.push({
          id: element.id,
          tagName: element.tagName,
          textContent: element.textContent || '',
        });
      }
    }
  });

  return list;
}
