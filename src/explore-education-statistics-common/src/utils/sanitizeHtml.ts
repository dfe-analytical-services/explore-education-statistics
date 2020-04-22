import sanitize from 'sanitize-html';

const config = {
  allowedTags: [
    'p',
    'h2',
    'h3',
    'h4',
    'h5',
    'strong',
    'i',
    'a',
    'ul',
    'ol',
    'li',
    'blockquote',
    'figure',
    'table',
    'tbody',
    'tr',
    'td',
  ],
  allowedAttributes: {
    figure: ['class'],
    a: ['href'],
  },
};

export default function sanitizeHtml(dirtyHtml: string): string {
  return sanitize(dirtyHtml, config);
}
