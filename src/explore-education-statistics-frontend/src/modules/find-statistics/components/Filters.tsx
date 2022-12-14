import { useMobileMedia } from '@common/hooks/useMedia';
import { PublicationFilter } from '@common/services/publicationService';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import AdvancedFilters from '@frontend/modules/find-statistics/components/AdvancedFilters';
import MobileFilters from '@frontend/modules/find-statistics/components/MobileFilters';
import ThemeFilters from '@frontend/modules/find-statistics/components/ThemeFilters';
import React from 'react';

export type FilterChangeHandler = ({
  filterType,
  nextValue,
}: {
  filterType: PublicationFilter;
  nextValue: string;
}) => void;

interface Props {
  releaseType?: ReleaseType;
  showMobileFilters: boolean;
  themeId?: string;
  themes: ThemeSummary[];
  totalResults?: number;
  onChange: FilterChangeHandler;
  onCloseMobileFilters: () => void;
}

const Filters = ({
  releaseType,
  showMobileFilters = false,
  themeId,
  themes,
  totalResults,
  onChange,
  onCloseMobileFilters,
}: Props) => {
  const { isMedia: isMobileMedia } = useMobileMedia();
  return (
    <>
      {!isMobileMedia && (
        <div className="govuk-!-margin-top-3">
          <h2 className="govuk-visually-hidden">Filter publications</h2>
          <ThemeFilters themeId={themeId} themes={themes} onChange={onChange} />
          <AdvancedFilters releaseType={releaseType} onChange={onChange} />
        </div>
      )}
      {isMobileMedia && showMobileFilters && (
        <MobileFilters
          releaseType={releaseType}
          themeId={themeId}
          themes={themes}
          totalResults={totalResults}
          onChange={onChange}
          onClose={onCloseMobileFilters}
        />
      )}
    </>
  );
};

export default Filters;
