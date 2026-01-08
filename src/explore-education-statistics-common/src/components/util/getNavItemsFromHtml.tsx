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
export default function getNavItemsFromHtml({
  html,
  blockId,
  headingLevels = ['h3'],
}: {
  html?: string;
  blockId?: string;
  headingLevels?: string[];
}): NavItem[] {
  const result: NavItem[] = [];

  if (html) {
    parseHtmlString(html, {
      replace: (node: DOMNode) => {
        if (node instanceof Element && headingLevels.includes(node.name)) {
          const text = domToReact(node.children);

          if (typeof text === 'string') {
            result.push({
              // Only use first 4 chars of blockId to keep IDs unique whilst relatively short
              id: generateIdFromHeading(text, blockId?.substring(0, 4)),
              text,
            });
          }
        }
        return undefined;
      },
    });
  }

  return result;
}
