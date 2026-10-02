import { odata } from '@azure/search-documents';

interface PublicationSearchFilters {
  releaseType?: string;
  themeId?: string;
}

export default function buildPublicationSearchFilter({
  releaseType,
  themeId,
}: PublicationSearchFilters): string | undefined {
  if (releaseType && themeId) {
    return odata`releaseType eq ${releaseType} and themeId eq ${themeId}`;
  }

  if (releaseType) {
    return odata`releaseType eq ${releaseType}`;
  }

  if (themeId) {
    return odata`themeId eq ${themeId}`;
  }

  return undefined;
}
