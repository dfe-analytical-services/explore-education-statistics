import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormRadioGroup from '@admin/prototypes/components/PrototypeFormRadioGroup';
import { releaseTypes } from '@common/services/types/releaseType';
import { Theme } from '@common/services/publicationService';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';
import React from 'react';

interface Props {
  selectedReleaseType: string;
  selectedTheme: string;
  showFilters: boolean;
  themes: Theme[];
  totalResults?: number;
  onCloseFilters: () => void;
  onSelectReleaseType: (type: string) => void;
  onSelectTheme: (theme: string) => void;
}

const PrototypeFilters = ({
  selectedReleaseType,
  selectedTheme,
  showFilters,
  themes,
  totalResults,
  onCloseFilters,
  onSelectReleaseType,
  onSelectTheme,
}: Props) => {
  const themeFilters = [{ label: 'All themes', value: 'all-themes' }].concat(
    themes.map(theme => {
      return {
        label: theme.title,
        value: theme.id,
      };
    }),
  );

  const [showHelpThemesModal, toggleHelpThemesModal] = useToggle(false);
  const [showHelpTypesModal, toggleHelpTypesModal] = useToggle(false);

  return (
    <div
      className={
        showFilters
          ? styles.prototypeShowMobileOverlay
          : styles.prototypeHideMobileOverlay
      }
    >
      <div className={styles.prototypeControlMobileOverlay}>
        <h2 className="govuk-!-margin-0 govuk-!-margin-left-2">
          {' '}
          {totalResults} result(s)
        </h2>
        <ButtonText
          className="govuk-!-margin-right-1"
          onClick={() => onCloseFilters()}
        >
          Back to results
        </ButtonText>
      </div>

      <h3 className="govuk-!-margin-bottom-0">Filter by theme</h3>
      <a
        href="#"
        onClick={() => {
          toggleHelpThemesModal(true);
        }}
      >
        What are themes?
      </a>
      <FormRadioGroup
        legendHidden
        className="govuk-!-padding-left-2 govuk-!-margin-top-3"
        id="theme"
        legend="Filter by theme"
        legendSize="m"
        name="theme"
        value={selectedTheme}
        small
        onChange={e => {
          onSelectTheme(e.target.value);
        }}
        options={themeFilters}
      />

      <h2 className="govuk-heading-m govuk-!-margin-top-6">Advanced filters</h2>

      <Accordion id="filters">
        <AccordionSection heading="Release type" goToTop={false}>
          <h3 className="govuk-!-margin-bottom-0">Filter by release type</h3>
          <a
            href="#"
            onClick={() => {
              toggleHelpTypesModal(true);
            }}
          >
            What are release types?
          </a>
          <FormRadioGroup
            legendHidden
            className="govuk-!-padding-left-2 govuk-!-margin-top-3"
            id="releaseType"
            legend="Filter by release type"
            legendSize="m"
            name="releaseType"
            value={selectedReleaseType}
            small
            onChange={e => {
              onSelectReleaseType(e.target.value);
            }}
            options={[{ label: 'Show all', value: 'all-release-types' }].concat(
              Object.keys(releaseTypes).map(type => {
                return {
                  label: releaseTypes[type as keyof typeof releaseTypes],
                  value: type,
                };
              }),
            )}
          />
        </AccordionSection>
      </Accordion>
      <div
        className={
          showFilters
            ? styles.prototypeShowMobileCTA
            : styles.prototypeHideMobileCTA
        }
      >
        <Button onClick={() => onCloseFilters()}>
          Show {totalResults} result(s)
        </Button>
      </div>
      <Modal
        open={showHelpThemesModal}
        title="Themes guidance"
        className="govuk-!-width-one-half"
      >
        <ModalContent contentType="helpThemes" />
        <Button
          onClick={() => {
            toggleHelpThemesModal(false);
          }}
        >
          Close
        </Button>
      </Modal>
      <Modal
        open={showHelpTypesModal}
        title="Release types guidance"
        className="govuk-!-width-one-half"
      >
        <ModalContent contentType="helpReleaseTypes" />
        <Button
          onClick={() => {
            toggleHelpTypesModal(false);
          }}
        >
          Close
        </Button>
      </Modal>
    </div>
  );
};

export default PrototypeFilters;
