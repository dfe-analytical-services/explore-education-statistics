import { DOMNode } from 'html-react-parser';

/**
 * Helper to recursively extract plain text from DOM nodes
 */
export default function extractTextFromDOMNode(node: DOMNode): string {
  // If it's a Text node, return the data
  if (node.type === 'text') {
    return node.data || '';
  }

  // If it's an Element (tag) with children, recurse through them
  if ('children' in node && Array.isArray(node.children)) {
    return (node.children as DOMNode[])
      .map(child => extractTextFromDOMNode(child))
      .join('');
  }

  return '';
}
