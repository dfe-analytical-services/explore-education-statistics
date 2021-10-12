import { Dictionary } from '@common/types';
import sanitize from 'sanitize-html';

export type SanitizeHtmlOptions = {
  allowedTags?: string[];
  allowedAttributes?: Dictionary<string[]>;
  allowedSchemesByTag?: Dictionary<string[]>;
  allowedStyles?: Dictionary<Dictionary<RegExp[]>>;
  transformTags?: Dictionary<string | sanitize.Transformer>;
};

export const defaultSanitizeOptions: SanitizeHtmlOptions = {
  allowedTags: [
    'p',
    'h2',
    'h3',
    'h4',
    'h5',
    'strong',
    'em',
    'i',
    'a',
    'ul',
    'ol',
    'li',
    'blockquote',
    'figure',
    'figcaption',
    'table',
    'thead',
    'tbody',
    'tr',
    'br',
    'td',
    'th',
    'img',
  ],
  allowedStyles: {
    figure: {
      width: [/^\d+%$/],
    },
  },
  allowedAttributes: {
    figure: ['class', 'style'],
    a: ['href', 'data-glossary'],
    img: ['alt', 'src', 'srcset', 'sizes', 'width'],
    th: ['colspan', 'rowspan'],
    td: ['colspan', 'rowspan'],
    h2: ['id'],
    h3: ['id'],
    h4: ['id'],
    h5: ['id'],
  },
  allowedSchemesByTag: {
    a: ['http', 'https', 'mailto', 'tel'],
    img: ['http', 'https'],
  },
};

export default function sanitizeHtml(
  dirtyHtml: string,
  options: SanitizeHtmlOptions = defaultSanitizeOptions,
): string {
  return sanitize(dirtyHtml, options);
}
