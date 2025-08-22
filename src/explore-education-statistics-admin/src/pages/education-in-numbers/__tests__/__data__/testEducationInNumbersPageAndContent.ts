import { EinContent } from '@admin/services/educationInNumbersContentService';
import { EducationInNumbersSummaryWithPrevVersion } from '@admin/services/educationInNumbersService';

const testEinPageVersion: EducationInNumbersSummaryWithPrevVersion = {
  id: 'test-ein-page',
  title: 'Test Education in Numbers Page',
  slug: 'test-ein-page',
  description: 'Test description for the Education in Numbers page',
  version: 2,
  previousVersionId: '', // @MarkFix do we want this here? or maybe two - Summary and SummaryWithPrevVersion instead?
};

export const testEinPageContent: EinContent = {
  // @MarkFix currently unused but should be used in tests
  id: 'ein-content-2',
  title: 'The content',
  slug: 'content-1',
  content: [],
};

export default testEinPageVersion;
