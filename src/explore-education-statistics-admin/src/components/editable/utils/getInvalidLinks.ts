import { JsonElement } from '@admin/types/ckeditor';
import Yup from '@common/validation/yup';

export interface InvalidUrl {
  text: string;
  url: string;
}

export default function getInvalidLinks(elements: JsonElement[]): InvalidUrl[] {
  return elements
    .flatMap(element => element.children?.flatMap(child => child))
    .reduce<InvalidUrl[]>((acc, el) => {
      if (!el?.attributes?.linkHref) {
        return acc;
      }

      const { linkHref } = el.attributes;

      try {
        // exclude anchor links, localhost and emails as they fail Yup url validation.
        if (
          linkHref &&
          !linkHref.startsWith('#') &&
          !linkHref.startsWith('http://localhost') &&
          !linkHref.startsWith('mailto:')
        ) {
          Yup.string().url().validateSync(linkHref.trim());
        }
      } catch {
        acc.push({
          text: el.data ?? '',
          url: linkHref,
        });
      }
      return acc;
    }, []);
}
