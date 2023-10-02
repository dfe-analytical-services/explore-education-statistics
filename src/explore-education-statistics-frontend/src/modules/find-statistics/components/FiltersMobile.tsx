import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import Modal, { ModalCloseButton } from '@common/components/Modal';
import { ThemeSummary } from '@common/services/themeService';
import { ReleaseType } from '@common/services/types/releaseType';
import AdvancedFilters from '@frontend/modules/find-statistics/components/AdvancedFilters';
import { FilterChangeHandler } from '@frontend/modules/find-statistics/components/FiltersDesktop';
import styles from '@frontend/modules/find-statistics/components/FiltersMobile.module.scss';
import ThemeFilters from '@frontend/modules/find-statistics/components/ThemeFilters';
import React from 'react';

interface Props {
  releaseType?: ReleaseType;
  themeId?: string;
  themes: ThemeSummary[];
  totalResults?: number;
  onChange: FilterChangeHandler;
}

const FiltersMobile = ({
  releaseType,
  themeId,
  themes,
  totalResults,
  onChange,
}: Props) => {
  const totalResultsString = `${totalResults} ${
    totalResults !== 1 ? 'results' : 'result'
  }`;

  return (
    <Modal
      fullScreen
      hideTitle
      title="Filter publications"
      triggerButton={<Button>Filter results</Button>}
    >
      <div className={styles.container}>
        <div className={styles.header}>
          <p className="govuk-heading-l govuk-!-margin-bottom-0">
            {totalResultsString}
          </p>
          <ModalCloseButton>
            <ButtonText>Back to results</ButtonText>
          </ModalCloseButton>
        </div>
        <ThemeFilters themeId={themeId} themes={themes} onChange={onChange} />
        <AdvancedFilters releaseType={releaseType} onChange={onChange} />
        <ModalCloseButton>
          <Button>{`Show ${totalResultsString}`}</Button>
        </ModalCloseButton>
      </div>
    </Modal>
  );
};

export default FiltersMobile;
