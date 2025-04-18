import { marked } from 'marked';

export default function toHtml(markdown: string) {
  return marked(markdown || '', {
    mangle: false,
    headerIds: false,
  });
}
