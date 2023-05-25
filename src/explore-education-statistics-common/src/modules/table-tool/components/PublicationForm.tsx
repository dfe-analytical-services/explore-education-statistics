import {
  Form,
  FormFieldRadioGroup,
  FormFieldset,
  FormGroup,
  FormRadioGroup,
  FormTextSearchInput,
} from '@common/components/form';
import InsetText from '@common/components/InsetText';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import WizardStepSummary from '@common/modules/table-tool/components/WizardStepSummary';
import {
  PublicationTreeSummary,
  Theme,
} from '@common/services/publicationService';
import Yup from '@common/validation/yup';
import { InjectedWizardProps } from '@common/modules/table-tool/components/Wizard';
import WizardStepFormActions from '@common/modules/table-tool/components/WizardStepFormActions';
import WizardStepHeading from '@common/modules/table-tool/components/WizardStepHeading';
import styles from '@common/modules/table-tool/components/PublicationForm.module.scss';
import { orderBy } from 'lodash';
import { Formik } from 'formik';
import React, { ReactNode, useMemo, useState } from 'react';

export interface PublicationFormValues {
  publicationId: string;
}

export type PublicationFormSubmitHandler = (values: {
  publication: PublicationTreeSummary;
}) => void;

const formId = 'publicationForm';

interface Props extends InjectedWizardProps {
  initialValues?: PublicationFormValues;
  themes: Theme[];
  onSubmit: PublicationFormSubmitHandler;
  renderSummaryAfter?: ReactNode;
}

const PublicationForm = ({
  initialValues = {
    publicationId: '',
  },
  themes,
  onSubmit,
  renderSummaryAfter,
  ...stepProps
}: Props) => {
  const { isActive, goToNextStep } = stepProps;

  const [searchTerm, setSearchTerm] = useState('');
  const [selectedThemeId, setSelectedThemeId] = useState<string>('');

  const publications = useMemo(() => {
    if (selectedThemeId) {
      return (
        themes
          .find(theme => theme.id === selectedThemeId)
          ?.topics.flatMap(topic => topic.publications) ?? []
      );
    }
    if (searchTerm) {
      return themes
        .flatMap(theme => theme.topics.flatMap(topic => topic.publications))
        .filter(publication =>
          publication.title.toLowerCase().includes(searchTerm.toLowerCase()),
        );
    }

    return [];
  }, [themes, searchTerm, selectedThemeId]);

  const getThemeForPublication = (publicationId: string) => {
    return themes.find(theme =>
      theme.topics
        .flatMap(topic => topic.publications)
        .find(pub => pub.id === publicationId),
    )?.title;
  };

  const getSelectedPublication = (publicationId: string) =>
    themes
      .flatMap(theme => theme.topics)
      .flatMap(topic => topic.publications)
      .find(publication => publication.id === publicationId);

  const stepHeading = (
    <WizardStepHeading {...stepProps} fieldsetHeading>
      Choose a publication
    </WizardStepHeading>
  );

  if (!themes.length) {
    return <p>No publications found</p>;
  }

  return (
    <Formik<PublicationFormValues>
      enableReinitialize
      initialValues={initialValues}
      validateOnBlur={false}
      validateOnChange={false}
      validationSchema={Yup.object<PublicationFormValues>({
        publicationId: Yup.string().required('Choose publication'),
      })}
      onSubmit={async ({ publicationId }) => {
        const publication = publications.find(p => p.id === publicationId);

        if (!publication) {
          throw new Error('Selected publication not found');
        }

        await goToNextStep(async () => {
          await onSubmit({ publication });
        });
      }}
    >
      {form => {
        if (isActive) {
          return (
            <Form {...form} id={formId} showSubmitError>
              <FormFieldset id="publicationForm" legend={stepHeading}>
                <p>Search or select a theme to find publications</p>
                <FormGroup className="govuk-!-margin-bottom-3">
                  <FormTextSearchInput
                    id={`${formId}-publicationIdSearch`}
                    label="Search publications"
                    name="publicationSearch"
                    onChange={event => {
                      setSearchTerm(event.target.value);
                      setSelectedThemeId('');
                      if (!event.target.value) {
                        form.setFieldValue('publicationId', '');
                      }
                    }}
                    onKeyPress={event => {
                      if (event.key === 'Enter') {
                        event.preventDefault();
                      }
                    }}
                    value={searchTerm}
                    width={20}
                  />
                  {searchTerm && publications.length > 0 && (
                    <p>
                      <a
                        href={`#${formId}-publications`}
                        className="govuk-!-margin-top-3 govuk-!-font-size-14"
                      >
                        Skip to search results
                      </a>
                    </p>
                  )}
                </FormGroup>

                <p>or</p>

                <div className={styles.optionsContainer}>
                  <FormRadioGroup
                    id={`${formId}-themes`}
                    legend="Select a theme"
                    legendSize="s"
                    name="themeId"
                    small
                    options={themes.map(theme => {
                      return {
                        label: theme.title,
                        value: theme.id,
                      };
                    })}
                    value={selectedThemeId}
                    onChange={e => {
                      setSelectedThemeId(e.target.value);
                      form.setFieldValue('publicationId', '');
                      setSearchTerm('');
                    }}
                  />

                  <div className={styles.publicationsList}>
                    <FormFieldRadioGroup
                      id="publications"
                      legend={
                        <>
                          Select a publication
                          <span
                            className="govuk-visually-hidden"
                            aria-live="polite"
                            aria-atomic="true"
                          >
                            {` ${publications.length} ${
                              publications.length === 1
                                ? `publication`
                                : `publications`
                            } found`}
                          </span>
                        </>
                      }
                      legendSize="s"
                      name="publicationId"
                      small
                      options={orderBy(publications, 'title').map(
                        publication => {
                          return {
                            hint: searchTerm
                              ? getThemeForPublication(publication.id)
                              : '',
                            hintSmall: true,
                            label: publication.title,
                            value: publication.id,
                          };
                        },
                      )}
                    />

                    {!publications.length && (
                      <>
                        <p>Search or select a theme to view publications</p>
                        {(searchTerm || selectedThemeId) && (
                          <InsetText>No publications found</InsetText>
                        )}
                      </>
                    )}

                    <div className="govuk-!-margin-top-6">
                      <WizardStepFormActions
                        {...stepProps}
                        isSubmitting={form.isSubmitting}
                      />
                    </div>
                  </div>
                </div>
              </FormFieldset>
            </Form>
          );
        }

        return (
          <>
            <WizardStepSummary
              {...stepProps}
              goToButtonText="Change publication"
            >
              {stepHeading}

              <SummaryList noBorder>
                <SummaryListItem term="Publication">
                  {getSelectedPublication(form.values.publicationId)?.title}
                </SummaryListItem>
              </SummaryList>
            </WizardStepSummary>

            {renderSummaryAfter}
          </>
        );
      }}
    </Formik>
  );
};

export default PublicationForm;
