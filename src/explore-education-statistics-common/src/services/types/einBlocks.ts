import { HtmlBlock } from '@common/services/types/blocks';
import { ContentSection } from '@common/services/publicationService';

// Generic Ein block types
export type EinBlockType = 'HtmlBlock';
export type EinContentBlock = EinHtmlBlock;

// EinHtmlBlock uses HtmlBlock, which is shared with release/methodology as well
// See comments in Admin's educationInNumbersContentService for more info
export type EinHtmlBlock = HtmlBlock;

// ContentSection is shared with release/methodology pages too
export type EinContentSection = ContentSection<EinContentBlock>;
