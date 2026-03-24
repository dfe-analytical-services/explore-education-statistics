import ContentHtml from '@common/components/ContentHtml';
import { ContentBlock } from '@common/services/types/blocks';
import { Dictionary } from '@common/types';
import {
  defaultSanitizeOptions,
  SanitizeHtmlOptions,
} from '@common/utils/sanitizeHtml';
import React, { useMemo } from 'react';
import { GlossaryEntry } from '@common/services/types/glossary';

interface Props {
  block: ContentBlock;
  transformImageAttributes?: (
    attributes: Dictionary<string>,
  ) => Dictionary<string>;
  generateH3Ids?: boolean;
  getGlossaryEntry?: (slug: string) => Promise<GlossaryEntry>;
  transformFeaturedTableLinks?: (url: string, text: string) => void;
  trackGlossaryLinks?: (glossaryEntrySlug: string) => void;
}

const ContentBlockRenderer = ({
  block,
  transformImageAttributes,
  generateH3Ids,
  getGlossaryEntry,
  transformFeaturedTableLinks,
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
    case 'HtmlBlock':
      return (
        <ContentHtml
          blockId={block.id}
          generateH3Ids={generateH3Ids}
          getGlossaryEntry={getGlossaryEntry}
          html={body}
          sanitizeOptions={sanitizeOptions}
          trackGlossaryLinks={trackGlossaryLinks}
          transformFeaturedTableLinks={transformFeaturedTableLinks}
        />
      );
    default:
      return null;
  }
};

export default ContentBlockRenderer;
