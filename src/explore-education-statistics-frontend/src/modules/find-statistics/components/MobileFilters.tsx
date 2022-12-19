import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import AdvancedFilters from '@frontend/modules/find-statistics/components/AdvancedFilters';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/Filters';
import styles from '@frontend/modules/find-statistics/components/MobileFilters.module.scss';
import ThemeFilters from '@frontend/modules/find-statistics/components/ThemeFilters';
import React from 'react';

interface Props {
  releaseType?: ReleaseType;
  themeId?: string;
  themes: ThemeSummary[];
  totalResults?: number;
  onChange: FilterChangeHandler;
  onClose: () => void;
}

const MobileFilters = ({
  releaseType,
  themeId,
  themes,
  totalResults,
  onChange,
  onClose,
}: Props) => {
  const totalResultsString = `${totalResults} ${
    totalResults !== 1 ? 'results' : 'result'
  }`;

  return (
    <Modal fullScreen hideTitle title="Filter publications" onExit={onClose}>
      <div className={styles.container}>
        <div className={styles.header}>
          <p className="govuk-heading-l govuk-!-margin-bottom-0">
            {totalResultsString}
          </p>
          <ButtonText onClick={onClose}>Back to results</ButtonText>
        </div>
        <ThemeFilters themeId={themeId} themes={themes} onChange={onChange} />
        <AdvancedFilters releaseType={releaseType} onChange={onChange} />
        <Button onClick={onClose}>{`Show ${totalResultsString}`}</Button>
      </div>
    </Modal>
  );
};

export default MobileFilters;
