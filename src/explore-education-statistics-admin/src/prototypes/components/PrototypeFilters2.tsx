import styles from '@admin/prototypes/PrototypePublicPage.module.scss';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonText from '@common/components/ButtonText';
import FormRadioGroup from '@admin/prototypes/components/PrototypeFormRadioGroup';
import { releaseTypes } from '@common/services/types/releaseType';
import { Theme } from '@common/services/publicationService';
import React, { useMemo } from 'react';
import Modal from '@common/components/Modal';
import useToggle from '@common/hooks/useToggle';
import ModalContent from '@admin/prototypes/components/PrototypeModalContent';
import { FormSelect } from '@common/components/form';

interface Props {
  selectedReleaseType: string;
  selectedTheme: string;
  selectedTopic: string;
  showFilters: boolean;
  themes: Theme[];
  totalResults?: number;
  onCloseFilters: () => void;
  onSelectReleaseType: (type: string) => void;
  onSelectTheme: (theme: string) => void;
  onSelectTopic: (topic: string) => void;
}

const PrototypeFilters = ({
  selectedReleaseType,
  selectedTheme,
  selectedTopic,
  showFilters,
  themes,
  totalResults,
  onCloseFilters,
  onSelectReleaseType,
  onSelectTheme,
  onSelectTopic,
}: Props) => {
  const themeFilters = useMemo(() => {
    return [{ label: 'All themes', value: 'all-themes' }].concat(
      themes.map(theme => {
        return {
          label: theme.title,
          value: theme.id,
        };
      }),
    );
  }, [themes]);

  const getSelectedTheme = (themeId: string) => {
    return themes.find(theme => theme.id === themeId);
  };

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
          className="govuk-!-margin-right-3"
          onClick={() => onCloseFilters()}
        >
          Back to results
        </ButtonText>
      </div>

      <FormSelect
        className="govuk-!-padding-left-2"
        id="theme"
        name="theme"
        label="Filter by theme"
        value={selectedTheme}
        onChange={e => {
          onSelectTheme(e.target.value);
          onSelectTopic(`all-topics-${e.target.value}`);
        }}
        options={themeFilters}
      />

      {selectedTheme !== 'all-themes' && (
        <>
          <FormRadioGroup
            className="govuk-!-margin-top-5"
            id="topics"
            legend={`${getSelectedTheme(selectedTheme)?.title} topics `}
            legendSize="s"
            small
            name="topic"
            hint={getSelectedTheme(selectedTheme)?.summary}
            value={selectedTopic}
            onChange={e => {
              onSelectTopic(e.target.value);
            }}
            options={[
              {
                label: `All topics`,
                value: `all-topics-${selectedTheme}`,
              },
            ].concat(
              getSelectedTheme(selectedTheme)?.topics.map(top => ({
                label: top.title,
                value: top.id,
              })) ?? [],
            )}
          />
          <div className="govuk-!-margin-top-2">
            <ButtonText
              onClick={() => {
                onSelectTheme('all-themes');
              }}
            >
              Clear theme and topic
            </ButtonText>
          </div>
        </>
      )}

      <h2 className="govuk-heading-m govuk-!-margin-top-9">Other filters</h2>

      <Accordion id="filters">
        <AccordionSection heading="Release type" goToTop={false}>
          <FormRadioGroup
            className="govuk-!-padding-left-2"
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
          <a
            href="#"
            onClick={() => {
              toggleHelpTypesModal(true);
            }}
          >
            What are release types?
          </a>
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
