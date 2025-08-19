import { EducationInNumbersPageContent } from '@admin/services/educationInNumbersContentService';
import { EducationInNumbersSummary } from '@admin/services/educationInNumbersService';

const testEinPageVersion: EducationInNumbersSummary = {
  id: 'test-ein-page',
  title: 'Test Education in Numbers Page',
  slug: 'test-ein-page',
  description: 'Test description for the Education in Numbers page',
  version: 2,
  previousVersionId: '',
};

export const testEinPageContent: EducationInNumbersPageContent = {
  id: 'ein-content-2',
  title: 'The content',
  slug: 'content-1',
  content: [],
};

export default testEinPageVersion;
