import { NavItem } from '@common/components/PageNavExpandable';
import generateIdFromHeading from '@common/components/util/generateIdFromHeading';
import getNavItemsFromHtml from '@common/components/util/getNavItemsFromHtml';
import {
  ContentSection,
  DataBlockViewModel,
  EmbedBlockViewModel,
  HtmlBlockViewModel,
  ReleaseVersionHomeContentSection,
} from '@common/services/publicationService';
import {
  ContentBlock,
  DataBlock,
  EmbedBlock,
} from '@common/services/types/blocks';

/**
 * Parse content sections to return nav items for each section heading,
 * with nested nav items for any h3s within the section content
 */
export default function getNavItemsFromContentSections(
  content:
    | ContentSection<ContentBlock | DataBlock | EmbedBlock>[]
    | ReleaseVersionHomeContentSection<
        HtmlBlockViewModel | DataBlockViewModel | EmbedBlockViewModel
      >[],
): NavItem[] {
  return content.map(section => {
    const { heading, content: sectionContent } = section;
    const subNavItems = sectionContent
      .flatMap(block => {
        if (block.type === 'HtmlBlock') {
          return getNavItemsFromHtml({
            html: block.body,
            blockId: block.id,
          });
        }
        return null;
      })
      .filter(item => !!item);

    return {
      id: generateIdFromHeading(heading),
      text: heading,
      subNavItems,
    };
  });
}
