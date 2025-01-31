import Turndown from 'turndown';

const turndownService = new Turndown();

export default function toMarkdown(value: string) {
  return turndownService.turndown(value || '');
}
