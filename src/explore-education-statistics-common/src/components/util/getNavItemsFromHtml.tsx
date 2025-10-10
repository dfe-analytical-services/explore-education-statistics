import parseHtmlString, {
  DOMNode,
  domToReact,
  Element,
} from 'html-react-parser';
import { NavItem } from '../PageNavExpandable';
import generateIdFromHeading from './generateIdFromHeading';

/**
 * Returns the text of headings from an Html string
 */
export default function getNavItemsFromHtml(
  html: string,
  headingLevels = ['h3'],
): NavItem[] {
  const result: NavItem[] = [];

  parseHtmlString(html, {
    replace: (node: DOMNode) => {
      if (node instanceof Element && headingLevels.includes(node.name)) {
        const text = domToReact(node.children);

        if (typeof text === 'string') {
          result.push({
            id: generateIdFromHeading(text),
            text,
          });
        }
      }
      return undefined;
    },
  });

  return result;
}
