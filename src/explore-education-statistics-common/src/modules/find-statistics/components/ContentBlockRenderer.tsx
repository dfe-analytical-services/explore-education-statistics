import ContentHtml from '@common/components/ContentHtml';
import { ContentBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';
import {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import React, { useMemo } from 'react';
import ReactMarkdown from 'react-markdown';
import { GlossaryEntry } from '@common/services/types/glossary';

interface Props {
  block: ContentBlock;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  trackContentLinks?: (url: string, newTab?: boolean) => void;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
}

const ContentBlockRenderer = ({
  block,
  transformImageAttributes,
  getGlossaryEntry,
  trackContentLinks,
  trackGlossaryLinks,
}: Props) => {
  const { body = '', type } = block;

  const sanitizeOptions: SanitizeHtmlOptions = useMemo(() => {
    return {
      ...defaultSanitizeOptions,
      transformTags: {
        img: (tagName, attribs) => {
          return {
            tagName,
            attribs: transformImageAttributes
              ? transformImageAttributes(attribs)
              : attribs,
          };
        },
      },
    };
  }, [transformImageAttributes]);

  switch (type) {
    case 'MarkDownBlock':
      return <ReactMarkdown className="dfe-content" source={body} />;
    case 'HtmlBlock':
      return (
        <ContentHtml
          html={body}
          sanitizeOptions={sanitizeOptions}
          getGlossaryEntry={getGlossaryEntry}
          trackContentLinks={trackContentLinks}
          trackGlossaryLinks={trackGlossaryLinks}
        />
      );
    default:
      return null;
  }
};

export default ContentBlockRenderer;
