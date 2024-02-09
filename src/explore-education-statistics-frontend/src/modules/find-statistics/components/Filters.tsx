import { PublicationFilter } from '@common/services/publicationService';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import AdvancedFilters from '@frontend/modules/find-statistics/components/AdvancedFilters';
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
  themeId?: string;
  themes: ThemeSummary[];
  onChange: FilterChangeHandler;
}

export default function Filters({
  releaseType,
  themeId,
  themes,
  onChange,
}: Props) {
  return (
    <>
      <ThemeFilters themeId={themeId} themes={themes} onChange={onChange} />
      <AdvancedFilters releaseType={releaseType} onChange={onChange} />
    </>
  );
}
