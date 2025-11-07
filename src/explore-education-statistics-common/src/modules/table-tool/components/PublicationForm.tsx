import { FormGroup, FormTextSearchInput } from '@common/components/form';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import FormFieldRadioGroup from '@common/components/form/FormFieldRadioGroup';
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
import orderBy from 'lodash/orderBy';
import React, { ReactNode, useMemo, useState } from 'react';
import { ObjectSchema } from 'yup';

interface FormValues {
  publicationId: string;
  themeId?: string | null;
}

export type PublicationFormSubmitHandler = (values: {
  publication: PublicationTreeSummary;
}) => void;

const formId = 'publicationForm';

interface Props extends InjectedWizardProps {
  initialValues?: FormValues;
  showSupersededPublications?: boolean;
  stepTitle: string;
  themes: Theme[];
  onSubmit: PublicationFormSubmitHandler;
  renderSummaryAfter?: ReactNode;
}

const PublicationForm = ({
  initialValues,
  showSupersededPublications = false,
  stepTitle,
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
          ?.publications.filter(publication =>
            showSupersededPublications
              ? publication
              : !publication.isSuperseded,
          ) ?? []
      );
    }
    if (searchTerm) {
      return themes
        .flatMap(theme => theme.publications)
        .filter(publication =>
          showSupersededPublications
            ? publication.title.toLowerCase().includes(searchTerm.toLowerCase())
            : !publication.isSuperseded &&
              publication.title
                .toLowerCase()
                .includes(searchTerm.toLowerCase()),
        );
    }

    return [];
  }, [themes, searchTerm, selectedThemeId, showSupersededPublications]);

  const getThemeForPublication = (publicationId: string) => {
    return themes.find(theme =>
      theme.publications.find(publication => publication.id === publicationId),
    )?.title;
  };

  const getSelectedPublication = (publicationId: string) =>
    themes
      .flatMap(theme => theme.publications)
      .find(publication => publication.id === publicationId);

  const stepHeading = (
    <WizardStepHeading {...stepProps}>{stepTitle}</WizardStepHeading>
  );

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      publicationId: Yup.string().required('Choose a publication'),
      themeId: Yup.string()
        .nullable()
        .test(
          'theme',
          'Choose a theme',
          value => !(!publications.length && !value),
        ),
    });
  }, [publications.length]);

  const handleSubmit = async ({ publicationId }: FormValues) => {
    const publication = publications.find(p => p.id === publicationId);

    if (!publication) {
      throw new Error('Selected publication not found');
    }

    await goToNextStep(async () => {
      await onSubmit({ publication });
    });
  };

  if (!themes.length) {
    return <p>No publications found</p>;
  }

  return (
    <FormProvider
      initialValues={initialValues}
      validationSchema={validationSchema}
    >
      {({ formState, getValues, resetField }) => {
        if (isActive) {
          return (
            <Form id={formId} onSubmit={handleSubmit}>
              {stepHeading}
              <p>Search for a publication or show publications by theme</p>
              <FormGroup className="govuk-!-margin-bottom-3">
                <FormTextSearchInput
                  id={`${formId}-publicationIdSearch`}
                  label="Search publications by title"
                  name="publicationSearch"
                  onChange={event => {
                    setSearchTerm(event.target.value);
                    if (searchTerm !== event.target.value) {
                      setSelectedThemeId('');
                      resetField('themeId');
                      resetField('publicationId');
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
                <FormFieldRadioGroup<FormValues>
                  legend="Show publications by theme"
                  legendSize="s"
                  name="themeId"
                  small
                  options={themes.map(theme => ({
                    label: theme.title,
                    value: theme.id,
                  }))}
                  onChange={e => {
                    setSelectedThemeId(e.target.value);
                    resetField('publicationId');
                    setSearchTerm('');
                  }}
                />

                <div className={styles.publicationsList}>
                  <FormFieldRadioGroup<FormValues>
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
                      publication => ({
                        hint: searchTerm
                          ? getThemeForPublication(publication.id)
                          : '',
                        hintSmall: true,
                        label: publication.title,
                        value: publication.id,
                      }),
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
                      isSubmitting={formState.isSubmitting}
                    />
                  </div>
                </div>
              </div>
            </Form>
          );
        }

        const values = getValues();

        return (
          <>
            <WizardStepSummary
              {...stepProps}
              goToButtonText="Change publication"
            >
              {stepHeading}

              <SummaryList noBorder>
                <SummaryListItem term="Publication">
                  {getSelectedPublication(values.publicationId)?.title}
                </SummaryListItem>
              </SummaryList>
            </WizardStepSummary>

            {renderSummaryAfter}
          </>
        );
      }}
    </FormProvider>
  );
};

export default PublicationForm;
