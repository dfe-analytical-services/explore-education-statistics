import { Dictionary } from '@common/types';
import sanitize from 'sanitize-html';

/**
 * Return true to filter the tag from markup.
 */
export type TagFilter = (frame: sanitize.IFrame) => boolean;

export type SanitizeHtmlOptions = {
  allowedTags?: string[];
  allowedAttributes?: Dictionary<string[]>;
  allowedSchemesByTag?: Dictionary<string[]>;
  allowedStyles?: Dictionary<Dictionary<RegExp[]>>;
  filterTags?: Dictionary<TagFilter>;
  parseStyleAttributes?: boolean;
  transformTags?: Dictionary<string | sanitize.Transformer>;
  textFilter?: (text: string, tagName: string) => string;
};

export const defaultSanitizeOptions: SanitizeHtmlOptions = {
  allowedTags: [
    'p',
    'h2',
    'h3',
    'h4',
    'h5',
    'strong',
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
  allowedAttributes: {
    figure: ['class', 'style'],
    a: ['href', 'data-featured-table', 'data-glossary', 'rel', 'target'],
    img: ['alt', 'src', 'srcset', 'sizes', 'width'],
    th: ['colspan', 'rowspan'],
    td: ['colspan', 'rowspan'],
    h2: ['id'],
    h3: ['id'],
    h4: ['id'],
    h5: ['id'],
    p: ['style'],
  },
  allowedSchemesByTag: {
    a: ['http', 'https', 'mailto', 'tel'],
    img: ['http', 'https'],
  },
  // allowedStyles doesn't work in the browser so have to set parseStyleAttributes
  // to false to allow all styles if need to any.
  // https://github.com/apostrophecms/sanitize-html/issues/547
  parseStyleAttributes: false,
};

export const releaseWarningBlockSanitizeOptions: SanitizeHtmlOptions = {
  allowedTags: ['p', 'a'],
  allowedAttributes: {
    a: ['href', 'rel', 'target'],
    p: ['style'],
  },
  allowedSchemesByTag: {
    a: ['http', 'https', 'mailto', 'tel'],
  },
  // Copied from defaultSanitizeOptions
  parseStyleAttributes: false,
};

export const commentTagAttributes: SanitizeHtmlOptions['allowedAttributes'] = {
  'comment-start': ['name'],
  'comment-end': ['name'],
  'resolvedcomment-start': ['name'],
  'resolvedcomment-end': ['name'],
};

export const commentTags = Object.keys(commentTagAttributes);

export default function sanitizeHtml(
  dirtyHtml: string,
  { filterTags, ...options }: SanitizeHtmlOptions = defaultSanitizeOptions,
): string {
  return sanitize(dirtyHtml, {
    ...options,
    exclusiveFilter: frame => {
      if (filterTags?.[frame.tag]) {
        return filterTags[frame.tag](frame);
      }

      return false;
    },
  });
}
