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
          conditional: (
            <FormRadioGroup
              id="topics"
              legend="Choose topic"
              legendSize="s"
              small
              name="topic"
              hint="Select a topic to filter results for this theme"
              value={selectedTopic}
              onChange={e => {
                onSelectTopic(e.target.value);
              }}
              options={[
                {
                  label: `All topics`,
                  value: `all-topics-${theme.id}`,
                },
              ].concat(
                theme.topics.map(top => ({
                  label: top.title,
                  value: top.id,
                })),
              )}
            />
          ),
        };
      }),
    );
  }, [selectedTopic, onSelectTopic, themes]);

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

      <FormRadioGroup
        className="govuk-!-padding-left-2"
        id="theme"
        legend="Filter by theme"
        legendSize="m"
        name="theme"
        value={selectedTheme}
        small
        onChange={e => {
          onSelectTheme(e.target.value);
          onSelectTopic(`all-topics-${e.target.value}`);
        }}
        options={themeFilters}
      />

      <h2 className="govuk-heading-m govuk-!-margin-top-6">Other filters</h2>

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
